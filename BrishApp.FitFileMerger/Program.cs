﻿using BrishApp.FitFileMerger.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BrishApp.FitFileMerger;

internal class Program
{
    private static Serilog.Core.Logger _logger;

    private static void Main(string[] args)
    {
        AppStartup();
        var source = new DecodeUtility(_logger).ProcessFiles();
        new EncodeUtility(_logger).EncodeActivityFile(source);
    }

    private static IHost AppStartup()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
        _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .WriteTo.File(path: @"..\..\..\Logs\.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 365, retainedFileTimeLimit: new TimeSpan(365, 0, 0, 0))
            .WriteTo.Console()
            .CreateLogger();
        _logger.Information("Starting application...");
        var host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .Build();

        return host;
    }
}