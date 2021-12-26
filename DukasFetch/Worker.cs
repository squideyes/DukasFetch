// ********************************************************
// Copyright (C) 2021 Louis S. Berman (louis@squideyes.com) 
// 
// This file is part of DukasFetch
// 
// The use of this source code is licensed under the terms 
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SquidEyes.Trading.Context;
using SquidEyes.Trading.FxData;
using System.Text;
using SquidEyes.Basics;
using static SquidEyes.Trading.FxData.Source;

namespace DukasFetch;

internal class Worker : BackgroundService
{
    private readonly IHost host;
    private readonly ILogger logger;
    private readonly Settings settings;

    public Worker(IHost host, ILogger<Worker> logger, Settings settings)
    {
        this.host = host;
        this.logger = logger;
        this.settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(settings.Folder))
            Directory.CreateDirectory(settings.Folder!);

        var sb = new StringBuilder();

        sb.Append($"Symbols: {string.Join(",", settings.Symbols!)}");
        sb.Append($"; MinYear: {settings.MinYear}");
        sb.Append($"; Replace: {settings.Replace}");
        sb.Append($"; Folder: \"{settings.Folder}\"");

        logger.LogInformation(sb.ToString());

        if (await ProcessFetchJobsAsync(stoppingToken))
            ProcessBundleJobs();

        await host.StopAsync(stoppingToken);
    }

    private void ProcessBundleJobs()
    {
        var (jobs, skipped) = GetBundleJobs();

        if (jobs.Count == 0)
        {
            logger.LogWarning("There are NO MISSING bundle files!");
        }
        else
        {
            logger.LogInformation($"ENQUEUED {jobs.Count:N0} BUNDLE jobs");

            try
            {
                foreach (var job in jobs)
                    job.BundleSaveAndLog(logger, settings.Folder!);

                logger.LogInformation(
                    $"CREATED {jobs.Count:N0} bundles (skipped {skipped:N0})");
            }
            catch (Exception error)
            {
                logger.LogError(error.Message);
            }
        }
    }

    private async Task<bool> ProcessFetchJobsAsync(CancellationToken stoppingToken)
    {
        var (jobs, skipped) = GetFetchJobs();

        if (jobs.Count == 0)
        {
            logger.LogWarning("There are NO UNFETCHED tick-data files!");

            return true;
        }
        else
        {
            logger.LogInformation($"ENQUEUED {jobs.Count:N0} tick-data FETCH + SAVE jobs");

            try
            {
                foreach (var job in jobs)
                {
                    if (stoppingToken.IsCancellationRequested)
                        return false;

                    await job.FetchSaveAndLogAsync(
                        logger, settings.Folder!, stoppingToken);
                }

                logger.LogInformation(
                    $"FETCHED & SAVED {jobs.Count:N0} tick-sets (skipped {skipped:N0})");

                return true;
            }
            catch (Exception error)
            {
                logger.LogError(error.Message);

                return false;
            }
        }
    }

    private HashSet<string> GetFileNames()
    {
        return new HashSet<string>(Directory.GetFiles(settings.Folder!, "*.*",
            SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
    }

    private (List<BundleJob> Jobs, int Skipped) GetBundleJobs()
    {
        var fileNames = GetFileNames();

        var jobs = new List<BundleJob>();
        var skipped = 0;

        var limitDate = DateTime.UtcNow.ToEasternFromUtc()
            .AsFunc(d => new DateOnly(d.Year, d.Month, 1));

        for (var year = Known.MinYear; year <= Known.MaxYear; year++)
        {
            for (var month = 1; month <= 12; month++)
            {
                var jobDate = new DateOnly(year, month, 1);

                if (jobDate >= limitDate)
                    continue;

                foreach (var pair in Known.Pairs.Values)
                {
                    var bundle = new Bundle(Dukascopy, pair, year, month);

                    if (fileNames.Contains(bundle.FileName))
                        skipped++;
                    else
                        jobs.Add(new BundleJob(pair.Symbol, year, month));
                }
            }
        }

        return (jobs, skipped);
    }

    private (List<FetchJob> Jobs, int Skipped) GetFetchJobs()
    {
        var fileNames = GetFileNames();

        var minTradeDate = new DateOnly(settings.MinYear, 1, 1);

        while (minTradeDate.DayOfWeek != DayOfWeek.Monday)
            minTradeDate = minTradeDate.AddDays(1);

        var maxTradeDate = DateTime.UtcNow.ToTradeDate(false).AddDays(-1);

        var skipped = 0;

        var jobs = new List<FetchJob>();

        bool Exists(Pair pair, DateOnly tradeDate)
        {
            var tickSet = new TickSet(Dukascopy, pair, tradeDate);

            var csv = tickSet.GetFileName(SaveAs.CSV);
            var sts = tickSet.GetFileName(SaveAs.STS);

            return fileNames!.Contains(csv) && fileNames.Contains(sts);
        }

        foreach (var pair in settings.Symbols!.Select(s => Known.Pairs[s]))
        {
            for (var tradeDate = minTradeDate;
                tradeDate <= maxTradeDate; tradeDate = tradeDate.AddDays(1))
            {
                if (!tradeDate.IsTradeDate())
                    continue;

                if (Exists(pair, tradeDate))
                    skipped++;
                else
                    jobs.Add(new FetchJob(pair.Symbol, tradeDate));
            }
        }

        return (jobs, skipped);
    }
}