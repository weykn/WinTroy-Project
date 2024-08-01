using System;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

public static class WinDefender
{
    static string pslog = "";

    public static string EnableMain()
    {
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) return "Administrator rights are required for this function.";

        string log = "";

        log += RegistryEdit(@"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "1");
        log += RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", "0");
        log += RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", "0");
        log += RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", "0");
        log += RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", "0");

        CheckDefender_Enable();

        return $"{log}\r\n{pslog}";
    }

    public static string DisableMain()
    {
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) return "Administrator rights are required for this function.";

        string log = "";

        log += RegistryEdit(@"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "0"); //Windows 10 1903 Redstone 6
        log += RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", "1");
        log += RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", "1");
        log += RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", "1");
        log += RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", "1");

        CheckDefender_Disable();

        return $"{log}\r\n{pslog}";
    }

    private static void CheckDefender_Enable()
    {
        Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "Get-MpPreference -verbose",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            }
        };
        proc.Start();
        while (!proc.StandardOutput.EndOfStream)
        {
            string line = proc.StandardOutput.ReadLine();

            if (line.StartsWith(@"DisableRealtimeMonitoring") && line.EndsWith("True"))
                RunPS("Set-MpPreference -DisableRealtimeMonitoring $false"); //real-time protection

            else if (line.StartsWith(@"DisableBehaviorMonitoring") && line.EndsWith("True"))
                RunPS("Set-MpPreference -DisableBehaviorMonitoring $false"); //behavior monitoring

            else if (line.StartsWith(@"DisableBlockAtFirstSeen") && line.EndsWith("True"))
                RunPS("Set-MpPreference -DisableBlockAtFirstSeen $false");

            else if (line.StartsWith(@"DisableIOAVProtection") && line.EndsWith("True"))
                RunPS("Set-MpPreference -DisableIOAVProtection $false"); //scans all downloaded files and attachments

            else if (line.StartsWith(@"DisablePrivacyMode") && line.EndsWith("True"))
                RunPS("Set-MpPreference -DisablePrivacyMode $false"); //displaying threat history

            else if (line.StartsWith(@"SignatureDisableUpdateOnStartupWithoutEngine") && line.EndsWith("True"))
                RunPS("Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $false"); //definition updates on startup

            else if (line.StartsWith(@"DisableArchiveScanning") && line.EndsWith("True"))
                RunPS("Set-MpPreference -DisableArchiveScanning $false"); //scan archive files, such as .zip and .cab files

            else if (line.StartsWith(@"DisableIntrusionPreventionSystem") && line.EndsWith("True"))
                RunPS("Set-MpPreference -DisableIntrusionPreventionSystem $false"); // network protection 

            else if (line.StartsWith(@"DisableScriptScanning") && line.EndsWith("True"))
                RunPS("Set-MpPreference -DisableScriptScanning $false"); //scanning of scripts during scans

            else if (line.StartsWith(@"SubmitSamplesConsent") && !line.EndsWith("1"))
                RunPS("Set-MpPreference -SubmitSamplesConsent 1"); //MAPSReporting 

            else if (line.StartsWith(@"MAPSReporting") && !line.EndsWith("2"))
                RunPS("Set-MpPreference -MAPSReporting 2"); //MAPSReporting 

            else if (line.StartsWith(@"HighThreatDefaultAction") && !line.EndsWith("1"))
                RunPS("Set-MpPreference -HighThreatDefaultAction 1 -Force"); // high level threat // Allow

            else if (line.StartsWith(@"ModerateThreatDefaultAction") && !line.EndsWith("0"))
                RunPS("Set-MpPreference -ModerateThreatDefaultAction 0"); // moderate level threat

            else if (line.StartsWith(@"LowThreatDefaultAction") && !line.EndsWith("0"))
                RunPS("Set-MpPreference -LowThreatDefaultAction 0"); // low level threat

            else if (line.StartsWith(@"SevereThreatDefaultAction") && !line.EndsWith("0"))
                RunPS("Set-MpPreference -SevereThreatDefaultAction 0"); // severe level threat
        }
    }

    private static string RegistryEdit(string regPath, string name, string value)
    {
        try
        {
            string retstr = $"REGEDIT: \\Path: \"{regPath}\", \\Name: \"{name}\", \\Value: {value}\r\n";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null)
                {
                    Registry.LocalMachine.CreateSubKey(regPath).SetValue(name, value, RegistryValueKind.DWord);
                    return retstr;
                }
                if (key.GetValue(name) != (object)value)
                    key.SetValue(name, value, RegistryValueKind.DWord);
            }

            return retstr;
        }
        catch (Exception ex)
        {
            return $"REGEDIT.ERROR: {ex.Message}\r\n";
        }
    }

    private static void CheckDefender_Disable()
    {
        Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "Get-MpPreference -verbose",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            }
        };
        proc.Start();
        while (!proc.StandardOutput.EndOfStream)
        {
            string line = proc.StandardOutput.ReadLine();

            if (line.StartsWith(@"DisableRealtimeMonitoring") && line.EndsWith("False"))
                RunPS("Set-MpPreference -DisableRealtimeMonitoring $true"); //real-time protection

            else if (line.StartsWith(@"DisableBehaviorMonitoring") && line.EndsWith("False"))
                RunPS("Set-MpPreference -DisableBehaviorMonitoring $true"); //behavior monitoring

            else if (line.StartsWith(@"DisableBlockAtFirstSeen") && line.EndsWith("False"))
                RunPS("Set-MpPreference -DisableBlockAtFirstSeen $true");

            else if (line.StartsWith(@"DisableIOAVProtection") && line.EndsWith("False"))
                RunPS("Set-MpPreference -DisableIOAVProtection $true"); //scans all downloaded files and attachments

            else if (line.StartsWith(@"DisablePrivacyMode") && line.EndsWith("False"))
                RunPS("Set-MpPreference -DisablePrivacyMode $true"); //displaying threat history

            else if (line.StartsWith(@"SignatureDisableUpdateOnStartupWithoutEngine") && line.EndsWith("False"))
                RunPS("Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $true"); //definition updates on startup

            else if (line.StartsWith(@"DisableArchiveScanning") && line.EndsWith("False"))
                RunPS("Set-MpPreference -DisableArchiveScanning $true"); //scan archive files, such as .zip and .cab files

            else if (line.StartsWith(@"DisableIntrusionPreventionSystem") && line.EndsWith("False"))
                RunPS("Set-MpPreference -DisableIntrusionPreventionSystem $true"); // network protection 

            else if (line.StartsWith(@"DisableScriptScanning") && line.EndsWith("False"))
                RunPS("Set-MpPreference -DisableScriptScanning $true"); //scanning of scripts during scans

            else if (line.StartsWith(@"SubmitSamplesConsent") && !line.EndsWith("2"))
                RunPS("Set-MpPreference -SubmitSamplesConsent 2"); //MAPSReporting 

            else if (line.StartsWith(@"MAPSReporting") && !line.EndsWith("0"))
                RunPS("Set-MpPreference -MAPSReporting 0"); //MAPSReporting 

            else if (line.StartsWith(@"HighThreatDefaultAction") && !line.EndsWith("6"))
                RunPS("Set-MpPreference -HighThreatDefaultAction 6 -Force"); // high level threat // Allow

            else if (line.StartsWith(@"ModerateThreatDefaultAction") && !line.EndsWith("6"))
                RunPS("Set-MpPreference -ModerateThreatDefaultAction 6"); // moderate level threat

            else if (line.StartsWith(@"LowThreatDefaultAction") && !line.EndsWith("6"))
                RunPS("Set-MpPreference -LowThreatDefaultAction 6"); // low level threat

            else if (line.StartsWith(@"SevereThreatDefaultAction") && !line.EndsWith("6"))
                RunPS("Set-MpPreference -SevereThreatDefaultAction 6"); // severe level threat
        }
    }

    private static void RunPS(string args)
    {
        pslog += $"PS: {args}\r\n";
        Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            }
        };
        proc.Start();
    }

}