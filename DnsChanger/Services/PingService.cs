using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public class PingService : IPingService
    {
        public async Task<long?> PingAsync(string host, int timeout = 1000)
        {
            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(host, timeout);
                return reply.Status == System.Net.NetworkInformation.IPStatus.Success
                    ? reply.RoundtripTime : (long?)null;
            }
            catch
            {
                return null;
            }
        }
    }
}
