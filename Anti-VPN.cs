using BrokeProtocol.API;
using BrokeProtocol.Entities;
using BrokeProtocol.Utility.Networking;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AntiVPN
{
    internal class Anti_VPN
    {
        [Target(GameSourceEvent.PlayerInitialize, ExecutionMode.Event)]
        public async void OnEvent(ShPlayer player)
        {
            if (!player.isHuman) { return; }
            if (!IsLocalIpAddress(player.svPlayer.connection.IP))
            {
                player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 |  Checking your connection.....");
                // using large datasets from different sources with 11,500+ networks from 2,200+ privacy
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://whois.as207111.net/api/lookup?ip_address={player.svPlayer.connection.IP}");
                httpWebRequest.Headers.Add("Authorization", "Bearer 120|1TxZhEle6wZ9fOngdcYzP3ac4i44fZC50TWvuZMW");
                httpWebRequest.Accept = "application/json";
                using (WebResponse response = httpWebRequest.GetResponse())
                {
                    Encoding utf = Encoding.UTF8;
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), utf))
                    {
                        string text = streamReader.ReadToEnd();
                        var privacy = JObject.Parse(text);
                        var proxy = privacy["privacy"]["proxy"];

                        if ((bool)proxy)
                        {
                            player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 |  You will be kicked for using VPN!!!");
                            await Task.Delay(3000);
                            player.svPlayer.svManager.Disconnect(player.svPlayer.connection, DisconnectTypes.Kicked);
                        }
                        else if (!(bool)proxy)
                        {
                            player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 |  You passed the check!!!");
                        }
                        else
                        {
                            player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 |  Error to check your connection.");
                        }
                    }
                }
            }
            else if (IsLocalIpAddress(player.svPlayer.connection.IP) || player.svPlayer.HasPermission("avpn.bypass"))
            {
                player.svPlayer.SendGameMessage("〔<color=#546eff>Anti-VPN</color>〕 |  Bypassed");
            }
        }

        // https://www.tutorialsrack.com/articles/409/how-to-check-if-ipv4-ip-address-is-local-or-not-in-csharp
        public static bool IsLocalIpAddress(string host)
        {
            try
            {
                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
