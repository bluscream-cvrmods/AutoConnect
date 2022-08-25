using ABI_RC.Core;
using ABI_RC.Core.Extensions;
using ABI_RC.Core.Networking;
using AutoConnect;
using Bluscream;
using Classes;
using HarmonyLib;
using MelonLoader;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using UnityEngine;
using URIScheme;
using URIScheme.Enums;
using MessageBox = ABI_RC.Core.Extensions.MessageBox;
using MessageBoxButtons = ABI_RC.Core.Extensions.MessageBoxButtons;

[assembly: MelonInfo(typeof(AutoConnect.Main), Guh.Name, Guh.Version, Guh.Author, Guh.DownloadLink)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace AutoConnect;
public static class Guh {
    public const string Name = "AutoConnect";
    public const string Author = "Bluscream";
    public const string Version = "1.0.0";
    public const string DownloadLink = "";
}
public static class Patches {
    public static void Init(HarmonyLib.Harmony harmonyInstance) {
        try {
            MelonLogger.Msg("Patching SetJoinTarget");
            _ = harmonyInstance.Patch(typeof(ABI_RC.Core.Networking.IO.Instancing.Instances).GetMethod("SetJoinTarget"), postfix: new HarmonyMethod(typeof(Patches).GetMethod("SetJoinTarget")));
        } catch (Exception ex) {
            MelonLogger.Error(ex);
        }
        MelonLogger.Msg("Harmony patches completed!");
    }

    public static bool PopulatePresenceFromNetwork(RichPresenceInstance_t instance) {
        MelonLogger.Msg("PopulatePresenceFromNetwork: {0}", instance.InstanceName);
        return false;
    }
    public static void SetJoinTarget(string instanceId, string worldId) {
        Main.GenerateReconnectScript(CVRUrl.CreateJoinURI(worldId, instanceId));
    }
}

public class Main : MelonMod {
    public const string bat_template = @"
taskkill /f /im {0}
timeout /t {1}
start """" ""{2}"" {3}
";
    public bool fully_loaded = false;
    public CVRUrl StartupURI;
    public MelonPreferences_Entry AutoConnectSetting, URIInstallIgnored, URIInstallForced;

    private static bool IsAdministrator() {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
    public void RegisterURI(URISchemeService service) {
        // try { service.Delete(); } catch (Exception ex) { MelonLogger.Error("Failed to delete local cvr:// protocol: {0}", ex.Message); }
        try { service.Set(); if (!service.Check()) throw new Exception("Service not found"); } catch (Exception ex) { MelonLogger.Error("Failed to register local cvr:// protocol: {0}", ex.Message); }
        service = new URISchemeService(service.Key, service.Description, service.RunPath, RegisterType.LocalMachine);
        // try { service.Delete(); } catch (Exception ex) { MelonLogger.Error("Failed to delete global cvr:// protocol: {0}", ex.Message); }
        try { service.Set(); if (!service.Check()) throw new Exception("Service not found"); } catch (Exception ex) { MelonLogger.Error("Failed to register global cvr:// protocol: {0}", ex.Message); }
    }

    public override void OnPreSupportModule() {
        MelonPreferences_Category cat = MelonPreferences.CreateCategory(Guh.Name);
        AutoConnectSetting = cat.CreateEntry<bool>("Enable Autoconnect", true, "Enable URI Protocol handler");
        URIInstallIgnored = cat.CreateEntry("URIInstallIgnored", false, "Disable URI Installation Check");
        URIInstallForced = cat.CreateEntry("URIInstallForced", false, "Force URI Installation Check", true);
        var forced = (bool)URIInstallForced.BoxedValue;
        if (forced || !(bool)URIInstallIgnored.BoxedValue) {
            var service = new URISchemeService("cvr", "URL:cvr Protocol", Environment.GetCommandLineArgs()[0], RegisterType.CurrentUser);
            if (forced || !service.Check()) {
                MelonLogger.Warning("cvr:// URI scheme not registered!");
                MessageBoxResult dr = MessageBox.Show("Would you like to install the cvr:// URI protocol now?(Requires admin permissions)\n\nSelecting \"No\" will suppress this message.", "[MelonLoader] AutoConnect Mod", MessageBoxButtons.YesNoCancel);
                MelonLogger.Msg("MessageBoxResult: {0}", dr);
                switch (dr) {
                    case MessageBoxResult.Yes:
                        RegisterURI(service);
                        break;
                    case MessageBoxResult.No:
                        URIInstallIgnored.BoxedValue = true;
                        MelonPreferences.Save();
                        break;
                }
            }
        }

        foreach (string arg in Environment.GetCommandLineArgs()) {
            bool success = arg.TryParseCVRUri(out var uri);
            if (success) {
                StartupURI = uri;
                break;
            }
        }
        var rec_file = new FileInfo("reconnect.bat");
        if (rec_file.Exists) {
            if (StartupURI is null) {
                foreach (var word in rec_file.ReadAllText().Split(' ')) {
                    bool success = word.TryParseCVRUri(out var uri);
                    if (success) {
                        StartupURI = uri;
                        break;
                    }
                }
            }
            File.Delete("reconnect.bat");
        }
    }
    public override void OnApplicationStart() {
        Patches.Init(HarmonyInstance);
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
        if (!fully_loaded && buildIndex == -1) {
            fully_loaded = true;
            Thread.Sleep(1000);
            OnGameFullyLoaded();
        }
    }
    public void OnGameFullyLoaded() {
        if ((bool)AutoConnectSetting.BoxedValue) {
            bool valid = StartupURI != null && StartupURI.IsValidJoinLink();
            MelonLogger.Msg("Checking StartupURI: {0} ({1})", StartupURI, (valid ? "Valid" : "Invalid"));
            if (valid) {
                StartupURI.Join();
            }
        }
    }

    public static void GenerateReconnectScript(CVRUrl uri) {
        string filename = Process.GetCurrentProcess().ProcessName + ".exe";
        string args = "";
        foreach (string arg in Environment.GetCommandLineArgs().Skip(1)) {
            if (!Uri.TryCreate(arg, UriKind.Absolute, out _)) {
                args += arg + " ";
            }
        }
        args += uri.Uri.ToString();
        File.WriteAllText("reconnect.bat", string.Format(bat_template, filename, 3, Environment.GetCommandLineArgs()[0], args.Replace("%", "%%").Trim()));
    }
}