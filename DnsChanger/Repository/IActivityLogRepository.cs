using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Repository
{
    public interface IActivityLogRepository
    {
        List<ActivityLogEntry> GetRecent(int count = 100);
        void Add(string title, string details = null);
        void DeleteAll();
    }
}
