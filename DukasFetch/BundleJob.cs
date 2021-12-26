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
    internal class BundleJob
    {
        public BundleJob(Symbol symbol, int year, int month)
        {
            Pair = Known.Pairs[symbol];
            Year = year;
            Month = month;
        }

        public Pair Pair { get; }
        public int Year { get; }
        public int Month { get; }

        public void BundleSaveAndLog(ILogger logger, string basePath)
        {
            var bundle = new Bundle(Source.Dukascopy, Pair, Year, Month);

            var tradeDates = Known.GetTradeDates(Year, Month);

            foreach (var tradeDate in tradeDates)
            {
                var tickSet = new TickSet(Source.Dukascopy, Pair, tradeDate);

                using var tickSetStream = File.OpenRead(
                    tickSet.GetFullPath(basePath, SaveAs.STS));

                tickSet.LoadFromStream(tickSetStream, SaveAs.STS);

                bundle.Add(tickSet);

                logger.LogDebug($"ADDED {tickSet} to {bundle}");
            }

            var fullPath = bundle.GetFullPath(basePath);

            fullPath.EnsurePathExists(false);

            using var bundleStream = File.OpenWrite(fullPath);

            bundle.SaveToStream(bundleStream);

            logger.LogInformation($"SAVED {tradeDates.Count} tick-sets to {bundle}");
        }
    }
}

