using DnsChanger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DnsChanger.Services
{
    public class DnsService : IDnsService
    {
        public NetworkInterface GetActiveAdapter()
        {
            return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
        }
        public void SetDns(DnsProvider provider)
        {
            var activeInterface = GetActiveAdapter();
            if (activeInterface == null) return;

            string[] dns = {provider.Primary, provider.Secondary };

            ApplyToActiveAdapter(activeInterface, dns);
        }
        public void UnsetDns()
        {
            var activeInterface = GetActiveAdapter();
            if (activeInterface == null) return;

            ApplyToActiveAdapter(activeInterface, null);
        }

        private void ApplyToActiveAdapter(NetworkInterface activeInterface, string[] dns)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach(ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                    if (objdns != null)
                    {
                        objdns["DNSServerSearchOrder"] = dns;
                        objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                    }
                }
            }
        }
    }
}
