using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

public class AppScannerService : BackgroundService
{
    private readonly ILogger<AppScannerService> _logger;
    private readonly LiteDbContext _db;
    private List<AppInfo> _prevApps;

    public AppScannerService(ILogger<AppScannerService> logger, LiteDbContext db)
    {
        _logger = logger;
        _db = db;
        _prevApps = _db.GetAllApps();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppScannerService started");
        var timer = new Timer(async (_) => await ScanApps(), null, TimeSpan.Zero, TimeSpan.FromHours(2));
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public async Task ScanApps()
    {
        _logger.LogInformation("Scanning installed applications...");
        try
        {
            List<AppInfo> currApps = GetInstalledApps();
            foreach (var currApp in currApps)
            {
                var prevApp = _prevApps.Find(a => a.Name == currApp.Name);
                if (prevApp == null)
                {
                    _db.InsertApp(currApp);
                    _logger.LogInformation($"[+] {currApp.Name} version {currApp.Version} was INSTALLED");
                }
            }
            foreach (var prevApp in _prevApps)
            {
                if (currApps.Find(a => a.Name == prevApp.Name) == null)
                {
                    _db.RemoveApp(prevApp.Name);
                    _logger.LogInformation($"[-] {prevApp.Name} version {prevApp.Version} was REMOVED");
                }
            }

        }
        catch (Exception e)
        {
            _logger.LogError($"Error during scanning: {e.Message}");
        }
    }

    private List<AppInfo> GetInstalledApps()
    {
        List<AppInfo> apps = new List<AppInfo>();
        const string regPath32 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        const string regPath64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        string regPath = Environment.Is64BitOperatingSystem ? regPath64 : regPath32;

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath))
        {
            foreach (string subkeyName in key.GetSubKeyNames())
            {
                using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                {
                    string name = subkey.GetValue("DisplayName") as string;
                    string version = subkey.GetValue("DisplayVersion") as string;
                    if (!string.IsNullOrEmpty(name))
                    {
                        apps.Add(new AppInfo
                        {
                            Name = name,
                            Version = version,
                            InstallDate = DateTime.Now
                        });
                    }
                }
            }
        }
        var scan = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "product get name,version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        scan.Start();
        while (!scan.StandardOutput.EndOfStream)
        {
            string line = scan.StandardOutput.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                string[] parts = line.Split(new string[] { "  " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    if (apps.Find(a => a.Name == parts[0]) == null)
                    {
                        apps.Add(new AppInfo
                        {
                            Name = parts[0],
                            Version = parts[1],
                            InstallDate = DateTime.Now
                        });
                    }
                }
            }
        }
        return apps;
    }
}