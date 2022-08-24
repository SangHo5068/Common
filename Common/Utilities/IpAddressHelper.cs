using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Common.Utilities
{
    public static class IpAddressHelper
    {
        //public static string GetLocalIPv4(NetworkInterfaceType _type)
        //{
        //    string output = string.Empty;
        //    foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        //    {
        //        if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
        //        {
        //            IPInterfaceProperties adapterProperties = item.GetIPProperties();

        //            if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
        //            {
        //                foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
        //                {
        //                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        //                    {
        //                        output = ip.Address.ToString();
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return output;
        //}

        /// <summary>
        /// Gets the local Ipv4.
        /// </summary>
        /// <returns>The local Ipv4.</returns>
        /// <param name="networkInterfaceType">Network interface type.</param>
        public static IPAddress GetLocalIPv4(NetworkInterfaceType networkInterfaceType)
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces().
                                        Where(i => i.NetworkInterfaceType == networkInterfaceType && i.OperationalStatus == OperationalStatus.Up);
                foreach (var networkInterface in networkInterfaces)
                {
                    var adapterProperties = networkInterface.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.FirstOrDefault() == null)
                        continue;
                    foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;

                        return ip.Address;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return IPAddress.None;
        }

        public static string GetClientIP()
        {
            string result = string.Empty;
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < hostEntry.AddressList.Length; i++)
            {
                if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    result = hostEntry.AddressList[i].ToString();
                    break;
                }
            }
            return result;
        }
    }
}
