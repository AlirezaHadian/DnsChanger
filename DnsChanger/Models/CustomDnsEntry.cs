using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Models
{
    public class CustomDnsEntry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Primary { get; set; }
        public string Secondary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
