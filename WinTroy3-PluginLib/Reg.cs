using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static WinTroy3_PluginLib.Config.PathConfig;

namespace WinTroy3_PluginLib
{
    public class Reg
    {
        public static int GetInt(string valueName)
        {
            return (int)GetReg(RegPath, valueName);
        }

        public static void SetInt(string valueName, int value)
        {
            SetReg(RegPath, valueName, value, RegistryValueKind.DWord);
        }

        public static string GetString(string valueName)
        {
            return GetReg(RegPath, valueName).ToString();
        }

        public static void SetString(string valueName, string value)
        {
            SetReg(RegPath, valueName, value, RegistryValueKind.String);
        }

        public static string GetDataString(string valueName)
        {
            return GetReg(MenuDataRegPath, "_menu_" + valueName).ToString();
        }

        public static void SetDataString(string valueName, string value)
        {
            SetReg(MenuDataRegPath, valueName, value, RegistryValueKind.String);
        }

        private static object GetReg(string path, string valueName)
        {
            return Registry.CurrentUser.OpenSubKey(path).GetValue(valueName);
        }

        private static void SetReg(
            string path,
            string valueName,
            object value,
            RegistryValueKind kind)
        {
            Registry.CurrentUser.CreateSubKey(path).SetValue(
                valueName,
                value,
                kind);
        }
    }
}
