using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WinTroy3_PluginLib;
using static WinTroy3_PluginLib.Config.PathConfig;

namespace WinTroy3_Client.Menu
{
    public static class Plugins
    {
        public static List<string> IDs = new List<string>();
        public static List<Plugin> MenuButtons = new List<Plugin>();

        public static List<string> ModalIDs = new List<string>();
        public static List<ModalResult> ModalResults = new List<ModalResult>();

        private static HttpClient httpClient = new HttpClient();

        private static bool IsModal(string jsonString)
        {
            return jsonString.Replace(" ", "").Contains("\"IsModal\":true");
        }

        public static async Task LoadMenus()
        {
            IDs.Clear();
            MenuButtons.Clear();
            ModalIDs.Clear();
            ModalResults.Clear();

            Control.Log("hrhr");
            var url = GetBaseUrl() + "Packages.txt";
            Control.Log(url);
            HttpResponseMessage response = await httpClient.GetAsync(url);
            var list = await response.Content.ReadAsStringAsync();
            var JsonFiles = list.Replace("\r", "").Split('\n');

            for (int j = 0; j < JsonFiles.Length; j++)
            {
                string fileName = JsonFiles[j];
                HttpResponseMessage jsonFile = await httpClient.GetAsync($"{GetBaseUrl()}Json/{fileName}");
                object[] objects = JsonConvert.DeserializeObject<object[]>(await jsonFile.Content.ReadAsStringAsync());

                for (int i = 0; i < objects.Length; i++)
                {
                    string value = objects[i].ToString();

                    bool isModal = IsModal(value);

                    if (isModal)
                    {
                        ModalResult modalResult = JsonConvert.DeserializeObject<ModalResult>(value);

                        Control.Log("ModalResult:");
                        PrintModalResult(modalResult);

                        ModalResults.Add(modalResult);

                        string modalId = modalResult.ModalId;

                        ModalIDs.Add(modalId);
                    }
                    else
                    {
                        Plugin menuButton = JsonConvert.DeserializeObject<Plugin>(value);

                        Control.Log("MenuButton:");
                        PrintMenuButton(menuButton);
                        IDs.Add(menuButton.ButtonId);
                        MenuButtons.Add(menuButton);
                    }
                }
            }
        }

        public static void PrintModalResult(ModalResult menuButton)
        {
            foreach (var prop in typeof(ModalResult).GetProperties())
            {
                Control.Log($"\t{prop.Name}: {prop.GetValue(menuButton)}");
            }

            foreach (var field in typeof(ModalResult).GetFields())
            {
                Control.Log($"\t{field.Name}: {field.GetValue(menuButton)}");
            }
        }

        public static void PrintMenuButton(Plugin menuButton)
        {
            foreach (var prop in typeof(Plugin).GetProperties())
            {
                Control.Log($"\t{prop.Name}: {prop.GetValue(menuButton)}");
            }

            foreach (var field in typeof(Plugin).GetFields())
            {
                Control.Log($"\t{field.Name}: {field.GetValue(menuButton)}");
            }
        }
    }
}
