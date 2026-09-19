using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public class IpInfoService : IIpInfoService
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        public async Task<IpInfoResult> GetIpInfoAsync(string ip = null)
        {
            string url = string.IsNullOrEmpty(ip)
                ? "https://free.freeipapi.com/api/v1/json"
                : $"https://free.freeipapi.com/api/v1/json/{ip}";

            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string timeZone = "-";
            if (root.TryGetProperty("timeZone", out var tz) && tz.GetArrayLength() > 0)
                timeZone = tz[0].GetString();

            return new IpInfoResult
            {
                IpAddress = root.GetProperty("ipAddress").GetString(),
                CountryName = root.GetProperty("countryName").GetString(),
                CityName = root.GetProperty("cityName").GetString(),
                Isp = root.TryGetProperty("asnOrganization", out var org) ? org.GetString() : "-",
                TimeZone = timeZone,
                IsProxy = root.TryGetProperty("isProxy", out var proxy) && proxy.GetBoolean()
            };
        }
    }
}
