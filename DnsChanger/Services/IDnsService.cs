using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public interface IDnsService
    {
        void SetDns(DnsProvider provider);
        void UnsetDns();
    }
}
