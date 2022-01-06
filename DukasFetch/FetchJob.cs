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

        public override string ToString() =>
            $"{Pair} {TradeDate.ToString("MM/dd/yyyy")}";

        public async Task FetchSaveAndLogAsync(ILogger logger,
            string folder, CancellationToken cancellationToken)
        {
            var tickSet = new TickSet(Source.Dukascopy, Pair, TradeDate);

            string Save(DataKind dataKind)
            {
                var fullPath = tickSet.GetFullPath(folder, dataKind);

                fullPath.EnsurePathExists(false);

                using (var stream = File.OpenWrite(fullPath))
                    tickSet.SaveToStream(stream, dataKind);

                return tickSet.GetFileName(dataKind);
            }

            var fetcher = new Fetcher(tickSet);

            for (var hour = 0; hour < 24; hour++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var (success, ticks) =
                    await fetcher.GetTicksAsync(hour, cancellationToken);

                if (!success)
                    throw new InvalidOperationException(
                        $"NO ticks returned for hour {hour} ({this})");

                if (ticks != null)
                    tickSet.AddRange(ticks);
            }

            var csv = Save(DataKind.CSV);

            var sts = Save(DataKind.STS);

            logger.LogInformation(
                $"FETCHED {tickSet.Count:N0} ticks (SAVED to {csv} and {sts})");
        }
    }
}

