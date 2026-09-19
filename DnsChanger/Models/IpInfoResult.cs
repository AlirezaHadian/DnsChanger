using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Models
{
    public class IpInfoResult
    {
        public string IpAddress { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string Isp { get; set; }
        public string TimeZone { get; set; }
        public bool IsProxy { get; set; }
    }
}
