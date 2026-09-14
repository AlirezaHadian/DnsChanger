using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public interface IDnsService
    {
        NetworkInterface GetActiveAdapter();
        void SetDns(DnsProvider provider);
        void UnsetDns();
        void RestartActiveAdapter();
    }
}
