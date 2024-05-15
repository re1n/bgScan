using System.Diagnostics;
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
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs")))
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));
                }
                logging.AddFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/app.log"));
                logging.AddEventLog();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<LiteDbContext>();
                services.AddHostedService<AppScannerService>();
            })
            .UseWindowsService();
}