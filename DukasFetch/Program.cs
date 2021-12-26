// ********************************************************
// Copyright (C) 2021 Louis S. Berman (louis@squideyes.com) 
// 
// This file is part of DukasFetch
// 
// The use of this source code is licensed under the terms 
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using DukasFetch;
using Fclp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SquidEyes.Basics;
using SquidEyes.Trading.Context;

if (!TryGetSettings(out Settings? settings))
    return;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services
        .AddSingleton(settings!)
        .AddHostedService<Worker>())
    .Build();

await host.RunAsync();

bool TryGetSettings(out Settings? settings)
{
    settings = null;

    var parser = new FluentCommandLineParser<Settings>();

    parser.Setup(x => x.Folder)
        .As('f', "folder")
        .Required()
        .WithDescription("The \"root\" folder to save .STS and .CSV tick-data files to");

    parser.Setup(x => x.Symbols)
        .As('s', "symbols")
        .SetDefault(EnumList.FromAll<Symbol>())
        .WithDescription("Space-separated list of FX pairs (i.e. EURUSD USDJPY)");

    parser.Setup(x => x.Replace)
        .As('r', "replace")
        .SetDefault(true)
        .WithDescription("If present, existing tick-data files will be replaced");

    parser.Setup(x => x.MinYear)
         .As('y', "minyear")
         .SetDefault(2018)
         .WithDescription("The first year to download ticks for (min/default = 2018)");

    parser.SetupHelp("?", "help")
          .Callback(text => Console.WriteLine(text));

    var result = parser.Parse(args);

    if (result.HasErrors)
    {
        Console.Write(result.ErrorText);

        parser.HelpOption.ShowHelp(parser.Options);

        return false;
    }
    else
    {
        settings = parser.Object;

        var hasErrors = false;

        if (settings.Folder!.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        {
            Console.WriteLine($"The \"{settings.Folder}\" folder is invalid!");

            hasErrors = true;
        }

        if (settings.MinYear < 2018)
        {
            Console.WriteLine($"The \"MinYear\" argument must be >= 2018!");

            hasErrors = true;
        }

        return !hasErrors;
    }
}

