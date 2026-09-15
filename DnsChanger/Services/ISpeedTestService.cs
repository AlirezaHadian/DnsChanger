using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public interface ISpeedTestService
    {
        Task<SpeedTestResult> RunTestAsync(IProgress<SpeedTestProgress> progress);
    }
}
