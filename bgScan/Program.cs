using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }
        catch(Exception e)
        {
            EventLog.WriteEntry("bgScan", $"Unhandled exception: {e.Message}", EventLogEntryType.Error);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                try
                {
                    logging.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                    });
                }
                catch (Exception e)
                {
                    ; // Stop crash when in non-console env
                }
                if (!Directory.Exists("logs"))
                {
                    Directory.CreateDirectory("logs");
                }
                logging.AddFile("logs/app.log");
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<LiteDbContext>();
                services.AddHostedService<AppScannerService>();
            });
}