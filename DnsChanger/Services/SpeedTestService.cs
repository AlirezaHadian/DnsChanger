using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public class SpeedTestService : ISpeedTestService
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private readonly IPingService _pingService;
        public SpeedTestService(IPingService pingService)
        {
            _pingService = pingService;
        }
        public async Task<SpeedTestResult> RunTestAsync(IProgress<SpeedTestProgress> progress)
        {
            var result = new SpeedTestResult();

            progress?.Report(new SpeedTestProgress { Phase = "در حال تست پینگ...", CurrentMbps = 0, PercentComplete = 0 });
            result.PingMs = await _pingService.PingAsync("1.1.1.1") ?? 0;

            progress?.Report(new SpeedTestProgress { Phase = "در حال شناسایی دیتاسنتر...", CurrentMbps = 0, PercentComplete = 0 });
            result.DataCenter = await GetDataCenterAsync();

            result.DownloadMbps = await TestDownloadAsync(progress);
            result.UploadMbps = await TestUploadAsync(progress);

            progress?.Report(new SpeedTestProgress { Phase = "تمام شد", CurrentMbps = 0, PercentComplete = 100 });
            return result;
        }
        private async Task<string> GetDataCenterAsync()
        {
            try
            {
                var trace = await _http.GetStringAsync("https://speed.cloudflare.com/cdn-cgi/trace");
                var line = trace.Split('\n').FirstOrDefault(l => l.StartsWith("colo="));
                return line != null ? line.Substring(5) : "نامشخص";
            }
            catch { return "نامشخص"; }
        }
        //Check this method
        private async Task<double> TestDownloadAsync(IProgress<SpeedTestProgress> progress)
        {
            try
            {
                const long totalBytes = 25_000_000; // 25 MB
                using var response = await _http.GetAsync(
    $"https://speed.cloudflare.com/__down?bytes={totalBytes}",
    HttpCompletionOption.ResponseHeadersRead);
                using var stream = await response.Content.ReadAsStreamAsync();

                var buffer = new byte[65536];
                long totalRead = 0;
                var sw = Stopwatch.StartNew();
                double lastReportSeconds = 0;

                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    totalRead += bytesRead;
                    double elapsed = sw.Elapsed.TotalSeconds;

                    if (elapsed - lastReportSeconds > 0.15)
                    {
                        double currentMbps = (totalRead * 8.0 / 1_000_000.0) / elapsed;
                        progress?.Report(new SpeedTestProgress
                        {
                            Phase = "در حال تست دانلود...",
                            CurrentMbps = Math.Round(currentMbps, 1),
                            PercentComplete = Math.Min(100, totalRead * 100.0 / totalBytes)
                        });
                        lastReportSeconds = elapsed;
                    }
                }
                sw.Stop();
                return Math.Round((totalRead * 8.0 / 1_000_000.0) / sw.Elapsed.TotalSeconds, 1);
            }
            catch { return 0; }
        }

        private async Task<double> TestUploadAsync(IProgress<SpeedTestProgress> progress)
        {
            try
            {
                const int totalBytes = 6_000_000;
                const int chunkCount = 10;
                const int chunkSize = totalBytes / chunkCount;

                var random = new Random();
                long totalSent = 0;
                var overallSw = Stopwatch.StartNew();

                for (int i = 0; i < chunkCount; i++)
                {
                    var chunk = new byte[chunkSize];
                    random.NextBytes(chunk);

                    var chunkSw = Stopwatch.StartNew();
                    await _http.PostAsync("https://speed.cloudflare.com/__up", new ByteArrayContent(chunk));
                    chunkSw.Stop();

                    totalSent += chunkSize;
                    double chunkMbps = (chunkSize * 8.0 / 1_000_000.0) / chunkSw.Elapsed.TotalSeconds;

                    progress?.Report(new SpeedTestProgress
                    {
                        Phase = "در حال تست آپلود...",
                        CurrentMbps = Math.Round(chunkMbps, 1),
                        PercentComplete = (i + 1) * 100.0 / chunkCount
                    });
                }
                overallSw.Stop();
                return Math.Round((totalSent * 8.0 / 1_000_000.0) / overallSw.Elapsed.TotalSeconds, 1);
            }
            catch { return 0; }
        }
    }
}
