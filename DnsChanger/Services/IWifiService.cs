using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public interface IWifiService
    {
        List<WifiNetworkInfo> GetAvailableNetworks();
        Task<bool> ConnectWithPasswordAsync(string ssid, string password, bool isSecured);
        Task<bool> ConnectToSavedProfileAsync(string ssid);
        bool HasSavedProfile(string ssid);
    }
}
