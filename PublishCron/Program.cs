using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;

namespace PublishCron
{
    class Program
    {
        static AppSettings GetAppSettings(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("appsettings.json was not found or is not accesible");
            }

            var appSettingsContent = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(appSettingsContent))
                throw new Exception("Appsettings file is empty");

            return JsonConvert.DeserializeObject<AppSettings>(appSettingsContent);
        }

        static void Main(string[] args)
        {
            try
            {
                //var currentDir = Directory.GetCurrentDirectory();
                //var configFilePath = Path.Combine(currentDir, "appsettings.json");
                //https://github.com/dotnet/project-system/issues/2239
                //->https://github.com/dotnet/project-system/issues/3619

                var appSettingsFilePath = "/var/netcore/console/PublishCron/appsettings.json";
                var appSettings = GetAppSettings(appSettingsFilePath);

                Log.Logger = new LoggerConfiguration()
                                    .MinimumLevel.Information()
                                    .WriteTo.File(appSettings.LogPath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                                    .CreateLogger();

                Log.Information($"Starting PublishCron");
                var dbService = new DbService(appSettings.DbConnection);
                dbService.PublishScheduledArticles(item =>
                {
                    Log.Information(item);
                });

                Log.Information($"PublishCron finished. Closing & flushing logs...");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"GoogleCron app failed with error: {ex.Message}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
