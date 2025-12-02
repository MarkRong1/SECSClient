using Microsoft.Extensions.Hosting;
using System.IO;
namespace SECSClient.Logging
{
    public class LogCleanupController : BackgroundService
    {
        private readonly string logDir = Path.Combine(AppContext.BaseDirectory, "logs");
        private readonly int _retentionDays = 90; // 可改為設定檔注入
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0).AddDays(1);
                var delay = nextRun - now;

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                try
                {
                    if (Directory.Exists(logDir))
                    {

                        var threshold = DateTime.Now.AddDays(-_retentionDays);
                        var files = Directory.EnumerateFiles(logDir, "*.log", SearchOption.AllDirectories);

                        foreach (var file in files)
                        {
                            var lastWrite = File.GetLastWriteTime(file);
                            if (lastWrite < threshold)
                            {
                                File.Delete(file);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 可加上 log 記錄清理失敗
                }
            }
        }
    }
}