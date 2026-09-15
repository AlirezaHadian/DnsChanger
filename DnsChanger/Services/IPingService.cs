using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public interface IPingService
    {
        Task<long?> PingAsync(string host, int timeout = 1000);
    }
}
