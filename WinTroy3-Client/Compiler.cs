using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.IO;
using WinTroy3_Client.Menu;
using System.Security.Cryptography;
using WinTroy3_PluginLib;
using Discord;
using Discord.WebSocket;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace WinTroy3_Client
{
    public class Compiler
    {
        private static readonly HttpClient client = new HttpClient();

        private static Assembly GetAssembly(string path)
        {
            if (File.Exists(path)) return Assembly.LoadFrom(path);
            return GetHttpAssemblyAsync(path).GetAwaiter().GetResult();
        }

        private static async Task<Assembly> GetHttpAssemblyAsync(string data)
        {
            var url = $"{Config.PathConfig.GetBaseUrl()}Assemblies/{Path.GetFileName(data)}";
            Control.Log(url);
            HttpResponseMessage response = await client.GetAsync(url);
            return Assembly.Load(await response.Content.ReadAsByteArrayAsync());
        }

        public static Respond RunFunction(string dllPath, string _typeName, string _methodName, object[] parameters)
        {
            string methodName = string.IsNullOrEmpty(_methodName) ? "Main" : _methodName;
            string typeName = string.IsNullOrEmpty(_typeName) ? "Program" : _typeName;

            Assembly assembly = GetAssembly(dllPath)
                ?? throw new ArgumentException($"Assembly not found.");
            Type type = assembly.GetType(typeName)
                ?? throw new ArgumentException($"Type {typeName} not found in the assembly.");
            MethodInfo method = type.GetMethod(methodName)
                ?? throw new ArgumentException($"Method {methodName} not found in the type {typeName}.");

            object instance = Activator.CreateInstance(type);

            return (Respond)method.Invoke(instance, parameters);
        }

        public static Respond RunModal(ModalResult modalResult, string dllFilePath, SocketModal modal)
        {
            ConsoleColor foreColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;

            Respond result;

            result = RunFunction(
                    dllFilePath,
                    modalResult.TypeName,
                    modalResult.Entry,
                    new object[] { modal });
            Control.Log("Result: " + result);

            Console.ForegroundColor = foreColor;

            return result;
        }

        public static Respond Run(Plugin plugin, string dllFilePath)
        {
            ConsoleColor foreColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;

            Respond result;

            result = RunFunction(
                    dllFilePath,
                    plugin.TypeName,
                    plugin.Entry,
                    new object[] { });
            Control.Log("Result: " + result);

            Console.ForegroundColor = foreColor;

            return result;
        }
    }
}
