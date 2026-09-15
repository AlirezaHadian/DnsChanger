using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Models
{
    public class SpeedTestResult
    {
        public double DownloadMbps { get; set; }
        public double UploadMbps { get; set; }
        public long PingMs { get; set; }
        public long JitterMs { get; set; }
        public string DataCenter { get; set; }
    }
}
