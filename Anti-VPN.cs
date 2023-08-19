using BrokeProtocol.API;
using BrokeProtocol.Entities;
using BrokeProtocol.Utility.Networking;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AntiVPN
{
    internal class AntiVPN
    {
        [Target(GameSourceEvent.PlayerInitialize, ExecutionMode.Event)]
        public async void OnEvent(ShPlayer player)
        {
            if (!player.isHuman)
                return;

            if (!IsLocalIpAddress(player.svPlayer.connection.IP))
            {
                player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 | Checking your connection...");

                IpInfo ipInfo = await IpInfoFetcher.CheckIfProxy(player.svPlayer.connection.IP);

                if (ipInfo != null)
                {
                    if (ipInfo.Status != "success")
                    {
                        try
                        {
                            if (ipInfo.Proxy || ipInfo.Hosting)
                            {
                                player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 | You will be kicked for using vpn!");
                                await Task.Delay(3000);
                                player.svPlayer.svManager.Disconnect(player.svPlayer.connection, DisconnectTypes.Kicked);
                            }
                            else
                            {
                                player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 | You passed the check!");
                            }
                        }
                        catch { }
                    }
                    else if (ipInfo.Status == "fail")
                    {
                        player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 | You passed temporary check due to endpoint error!");
                    }
                }
            }
            else if (IsLocalIpAddress(player.svPlayer.connection.IP) || player.svPlayer.HasPermission("avpn.bypass"))
            {
                player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 | Bypassed!");
            }
        }

        // Class to store information about the IP
        public class IpInfo
        {
            public string Status;
            public bool Proxy;
            public bool Hosting;
        }

        // Class to fetch IP information from an API
        public static class IpInfoFetcher
        {
            public static async Task<IpInfo> CheckIfProxy(string ip)
            {
                string apiUrl = $"https://thingproxy.freeboard.io/fetch/http://ip-api.com/json/{ip}?fields=query,status,proxy,hosting";
                using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
                {
                    var tcs = new TaskCompletionSource<IpInfo>();

                    request.SendWebRequest().completed += operation =>
                    {
                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogError($"Request failed: {request.error}");
                            tcs.SetResult(null);
                            return;
                        }

                        string responseBody = request.downloadHandler.text;
                        IpInfo ipInfo = JsonUtility.FromJson<IpInfo>(responseBody);
                        tcs.SetResult(ipInfo);
                    };

                    return await tcs.Task;
                }
            }
        }

        // Check if an IP address is local
        public static bool IsLocalIpAddress(string host)
        {
            try
            {
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                foreach (IPAddress hostIP in hostIPs)
                {
                    if (IPAddress.IsLoopback(hostIP))
                        return true;

                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP))
                            return true;
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
