using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinTroy3_PluginLib;

public class SystemInfo
{
    public static Respond ShowSystemInfoMenu()
    {
        return new Respond
        {
            RespondTitle = "System Info",
            RespondText = "System releated info's about the device",
            RespondButtons = new string[]
            {
                "envvars",
                "fsinfo",
                "services",
                "networkinfo",
                "procinfo",
                "swinfo",
                "hwinfo"
            }
        };
    }

    public static Respond EnvironmentVariables()
    {
        string envInfo = "Environment Variables:" + Environment.NewLine;

        foreach (string key in Environment.GetEnvironmentVariables().Keys)
        {
            envInfo += $" - {key}: {Environment.GetEnvironmentVariable(key)}" + Environment.NewLine;
        }

        return new Respond
        {
            RespondTitle = "Environment Variables",
            RespondText = envInfo
        };
    }

    public static Respond FileSystemInfo()
    {
        string fileSysInfo = "";

        DriveInfo[] drives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in drives)
        {
            fileSysInfo += $"# {drive.Name}" + Environment.NewLine;
            fileSysInfo += $"  Type: `{drive.DriveType}`" + Environment.NewLine;
            if (drive.IsReady)
            {
                fileSysInfo += $"  Format: `{drive.DriveFormat}`" + Environment.NewLine;
                fileSysInfo += $"  Total Size: `{drive.TotalSize}` bytes" + Environment.NewLine;
                fileSysInfo += $"  Free Space: `{drive.TotalFreeSpace}` bytes" + Environment.NewLine;
            }
        }

        return new Respond
        {
            RespondTitle = "File System Info",
            RespondText = fileSysInfo
        };
    }

    public static Respond Services()
    {
        string serviceInfo = "";

        // Query Windows services
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
        ManagementObjectCollection collection = searcher.Get();

        serviceInfo += "Windows Services:" + Environment.NewLine;
        foreach (ManagementObject obj in collection)
        {
            serviceInfo += " - Name: " + obj["Name"] + Environment.NewLine;
            serviceInfo += "   Display Name: " + obj["DisplayName"] + Environment.NewLine;
            serviceInfo += "   Description: " + obj["Description"] + Environment.NewLine;
            serviceInfo += "   State: " + obj["State"] + Environment.NewLine;
            serviceInfo += "   Start Mode: " + obj["StartMode"] + Environment.NewLine;
            serviceInfo += "   Path: " + obj["PathName"] + Environment.NewLine;
            serviceInfo += "   Service Type: " + obj["ServiceType"] + Environment.NewLine;
            serviceInfo += "   Started: " + obj["Started"] + Environment.NewLine;
            serviceInfo += "   Start Name: " + obj["StartName"] + Environment.NewLine;
            serviceInfo += "   Process ID: " + obj["ProcessId"] + Environment.NewLine;
            serviceInfo += Environment.NewLine;
        }

        return new Respond
        {
            RespondTitle = "Services",
            RespondText = serviceInfo
        };
    }

    public static Respond NetworkInfo()
    {
        string networkInfo = "";

        // Get network configuration information
        ManagementObjectSearcher networkSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = true");
        ManagementObjectCollection networkCollection = networkSearcher.Get();

        networkInfo += "Network Configuration:" + Environment.NewLine;
        foreach (ManagementObject obj in networkCollection)
        {
            networkInfo += " - Adapter Description: " + obj["Description"] + Environment.NewLine;
            networkInfo += "    MAC Address: " + obj["MACAddress"] + Environment.NewLine;

            // IP Address information
            string[] ipAddresses = (string[])obj["IPAddress"];
            if (ipAddresses != null)
            {
                networkInfo += "    IP Address(es): ";
                foreach (string ip in ipAddresses)
                {
                    networkInfo += ip + " ";
                }
                networkInfo += Environment.NewLine;
            }

            // Other network settings (e.g., subnet mask, default gateway)
            networkInfo += "    Subnet Mask: " + obj["IPSubnet"] + Environment.NewLine;
            networkInfo += "    Default Gateway: " + obj["DefaultIPGateway"] + Environment.NewLine;
        }

        return new Respond
        {
            RespondTitle = "Network Info",
            RespondText = networkInfo
        };
    }

    public static Respond ProcInfo()
    {
        string processInfo = "";

        // Get information about running processes
        ManagementObjectSearcher processSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process");
        ManagementObjectCollection processCollection = processSearcher.Get();

        processInfo += "Running Processes:" + Environment.NewLine;
        foreach (ManagementObject obj in processCollection)
        {
            processInfo += " - Process Name: " + obj["Name"] + Environment.NewLine;
            processInfo += "    Process ID: " + obj["ProcessId"] + Environment.NewLine;
            processInfo += "    Executable Path: " + obj["ExecutablePath"] + Environment.NewLine;
            processInfo += "    Creation Date: " + ManagementDateTimeConverter.ToDateTime(obj["CreationDate"].ToString()) + Environment.NewLine;
            processInfo += "    Priority: " + obj["Priority"] + Environment.NewLine;
            processInfo += "    Thread Count: " + obj["ThreadCount"] + Environment.NewLine;
            processInfo += "    Virtual Memory Size: " + obj["VirtualSize"] + " bytes" + Environment.NewLine;
            processInfo += "    Working Set Size: " + obj["WorkingSetSize"] + " bytes" + Environment.NewLine;
            processInfo += "    Command Line: " + obj["CommandLine"] + Environment.NewLine;
            processInfo += Environment.NewLine;
        }

        return new Respond
        {
            RespondTitle = "Processes",
            RespondText = processInfo
        };
    }

    public static Respond SwInfo()
    {
        string softwareInfo = "";

        // Get information about operating system
        ManagementObjectSearcher osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        ManagementObjectCollection osCollection = osSearcher.Get();

        foreach (ManagementObject obj in osCollection)
        {
            softwareInfo += "Operating System: " + obj["Caption"] + Environment.NewLine;
            softwareInfo += "Version: " + obj["Version"] + Environment.NewLine;
            softwareInfo += "Service Pack: " + obj["ServicePackMajorVersion"] + "." + obj["ServicePackMinorVersion"] + Environment.NewLine;
            softwareInfo += "Architecture: " + obj["OSArchitecture"] + Environment.NewLine;
        }

        // Get information about installed software
        ManagementObjectSearcher softwareSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product");
        ManagementObjectCollection softwareCollection = softwareSearcher.Get();

        softwareInfo += "Installed Software:" + Environment.NewLine;
        foreach (ManagementObject obj in softwareCollection)
        {
            softwareInfo += " - " + obj["Name"] + Environment.NewLine;
            softwareInfo += "    Version: " + obj["Version"] + Environment.NewLine;
            softwareInfo += "    Vendor: " + obj["Vendor"] + Environment.NewLine;
        }

        return new Respond
        {
            RespondTitle = "Software Info",
            RespondText = softwareInfo
        };
    }

    public static Respond HwInfo()
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
        ManagementObjectCollection collection = searcher.Get();

        string deviceInfo = "";

        foreach (ManagementObject obj in collection)
        {
            deviceInfo += "Manufacturer: " + obj["Manufacturer"] + Environment.NewLine;
            deviceInfo += "Model: " + obj["Model"] + Environment.NewLine;
            deviceInfo += "Total Physical Memory: " + obj["TotalPhysicalMemory"] + " bytes" + Environment.NewLine;
            deviceInfo += "System Type: " + obj["SystemType"] + Environment.NewLine;
        }

        searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
        collection = searcher.Get();

        foreach (ManagementObject obj in collection)
        {
            deviceInfo += "Processor: " + obj["Name"] + Environment.NewLine;
            deviceInfo += "Number of Cores: " + obj["NumberOfCores"] + Environment.NewLine;
            deviceInfo += "Processor ID: " + obj["ProcessorId"] + Environment.NewLine;
        }

        searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
        collection = searcher.Get();

        foreach (ManagementObject obj in collection)
        {
            deviceInfo += "Physical Memory: " + obj["Capacity"] + " bytes" + Environment.NewLine;
            deviceInfo += "Memory Manufacturer: " + obj["Manufacturer"] + Environment.NewLine;
            deviceInfo += "Memory Type: " + obj["MemoryType"] + Environment.NewLine;
        }

        searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
        collection = searcher.Get();

        foreach (ManagementObject obj in collection)
        {
            deviceInfo += "Graphics Card: " + obj["Name"] + Environment.NewLine;
            deviceInfo += "Driver Version: " + obj["DriverVersion"] + Environment.NewLine;
            deviceInfo += "Video Processor: " + obj["VideoProcessor"] + Environment.NewLine;
        }

        return new Respond
        {
            RespondTitle = "Hardware Info",
            RespondText = deviceInfo
        };
    }
}