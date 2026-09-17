using DnsChanger.Models;
using ManagedNativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public class WifiService : IWifiService
    {
        public List<WifiNetworkInfo> GetAvailableNetworks()
        {
            var connectedSsids = NativeWifi.EnumerateConnectedNetworkSsids()
                .Select(s => s.ToString())
                .ToHashSet();

            var networks = NativeWifi.EnumerateAvailableNetworks()
                .Where(n => !string.IsNullOrEmpty(n.Ssid.ToString()))
                .GroupBy(n => n.Ssid.ToString())
                .Select(g => g.OrderByDescending(n => n.SignalQuality).First());

            var result = networks.Select(n => new WifiNetworkInfo
            {
                Name = n.Ssid.ToString(),
                SignalPercent = (int)n.SignalQuality,
                IsSecured = n.IsSecurityEnabled,
                IsConnected = connectedSsids.Contains(n.Ssid.ToString())
            })
                    .OrderByDescending(n => n.IsConnected)
    .ThenByDescending(n => n.SignalPercent)
    .ToList();

            return result;
        }
        public async Task<bool> ConnectWithPasswordAsync(string ssid, string password, bool isSecured)
        {
            var interfaceId = NativeWifi.EnumerateInterfaces().FirstOrDefault()?.Id;
            if (interfaceId == null) return false;

            string profileXml = isSecured ? BuildSecuredProfileXml(ssid, password) : BuildOpenProfileXml(ssid);

            bool profileSet = NativeWifi.SetProfile(
                interfaceId.Value,
                ProfileType.AllUser,
                profileXml,
                null,
                overwrite: true);

            if (!profileSet) return false;

            return await NativeWifi.ConnectNetworkAsync(
                interfaceId.Value,
                ssid,
                BssType.Infrastructure,
                TimeSpan.FromSeconds(10));
        }
        public async Task<bool> ConnectToSavedProfileAsync(string ssid)
        {
            var interfaceId = NativeWifi.EnumerateInterfaces().FirstOrDefault()?.Id;
            if (interfaceId == null) return false;

            return await NativeWifi.ConnectNetworkAsync(
                interfaceId.Value,
                ssid,
                BssType.Infrastructure,
                TimeSpan.FromSeconds(10));
        }
        public bool HasSavedProfile(string ssid)
        {
            var interfaceId = NativeWifi.EnumerateInterfaces().FirstOrDefault()?.Id;
            if (interfaceId == null) return false;

            return NativeWifi.EnumerateProfileNames()
                .Any(name => name == ssid);
        }
        private string BuildSecuredProfileXml(string ssid, string password) => $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>{ssid}</name>
    <SSIDConfig><SSID><name>{ssid}</name></SSID></SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>manual</connectionMode>
    <MSM>
        <security>
            <authEncryption>
                <authentication>WPA2PSK</authentication>
                <encryption>AES</encryption>
                <useOneX>false</useOneX>
            </authEncryption>
            <sharedKey>
                <keyType>passPhrase</keyType>
                <protected>false</protected>
                <keyMaterial>{password}</keyMaterial>
            </sharedKey>
        </security>
    </MSM>
</WLANProfile>";

        private string BuildOpenProfileXml(string ssid) => $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>{ssid}</name>
    <SSIDConfig><SSID><name>{ssid}</name></SSID></SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>manual</connectionMode>
    <MSM>
        <security>
            <authEncryption>
                <authentication>open</authentication>
                <encryption>none</encryption>
                <useOneX>false</useOneX>
            </authEncryption>
        </security>
    </MSM>
</WLANProfile>";
    }
}
