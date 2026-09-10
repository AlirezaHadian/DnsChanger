using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Repository
{
    public interface ICustomDnsRepository
    {
        List<CustomDnsEntry> GetAllDns();
        void Add(CustomDnsEntry dnsEntry);
        void Delete(int id);
    }
}
