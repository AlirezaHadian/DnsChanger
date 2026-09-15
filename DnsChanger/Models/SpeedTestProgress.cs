using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Models
{
    public class SpeedTestProgress
    {
        public string Phase { get; set; }
        public double CurrentMbps { get; set; }
        public double PercentComplete { get; set; }
        public long? PingMs { get; set; }
        public string DataCenter { get; set; }
    }
}
