// ********************************************************
// Copyright (C) 2021 Louis S. Berman (louis@squideyes.com) 
// 
// This file is part of DukasFetch
// 
// The use of this source code is licensed under the terms 
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Microsoft.Extensions.Logging;
using SquidEyes.Basics;
using SquidEyes.Trading.Context;
using SquidEyes.Trading.FxData;

namespace DukasFetch
{
    internal class FetchJob
    {
        public FetchJob(Symbol symbol, DateOnly tradeDate)
        {
            Pair = Known.Pairs[symbol];
            TradeDate = tradeDate;
        }

        public Pair Pair { get; }
        public DateOnly TradeDate { get; }

        public async Task FetchSaveAndLogAsync(ILogger logger,
            string folder, CancellationToken cancellationToken)
        {
            var tickSet = new TickSet(Source.Dukascopy, Pair, TradeDate);

            string Save(SaveAs saveAs)
            {
                var fullPath = tickSet.GetFullPath(folder, saveAs);

                fullPath.EnsurePathExists(false);

                using var stream = File.Create(fullPath);

                tickSet.SaveToStream(stream, saveAs);

                return tickSet.GetFileName(saveAs);
            }

            var fetcher = new Fetcher(tickSet);

            for (int hour = 0; hour < 24; hour++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var (success, ticks) =
                    await fetcher.GetTicksAsync(hour, cancellationToken);

                if (!success)
                    throw new InvalidOperationException();

                if (ticks != null)
                    tickSet.AddRange(ticks);
            }

            var csv = Save(SaveAs.CSV);

            var sts = Save(SaveAs.STS);

            logger.LogInformation(
                $"FETCHED {tickSet.Count:N0} ticks (SAVED to {csv} and {sts})");
        }
    }
}

