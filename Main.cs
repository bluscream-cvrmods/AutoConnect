using ABI_RC.Core.Networking;
using AutoConnect;
using Classes;
using HarmonyLib;
using MelonLoader;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
start """" {2}
";
    public bool fully_loaded = false;
    public CVRUrl StartupURI;
    public MelonPreferences_Entry AutoConnectSetting;

    public override void OnPreSupportModule() {
        foreach (string arg in Environment.GetCommandLineArgs()) {
            bool success = Uri.TryCreate(arg.Trim(), UriKind.Absolute, out Uri uri);
            if (success && uri.Scheme == "cvr") {
                StartupURI = new CVRUrl(arg.Trim());
                LoggerInstance.Msg("Found and set StartupURI: {0}", StartupURI);
                return;
            }
        }
        if (File.Exists("reconnect.bat")) File.Delete("reconnect.bat");
    }
    public override void OnApplicationStart() {
        MelonPreferences_Category cat = MelonPreferences.CreateCategory(Guh.Name);
        AutoConnectSetting = cat.CreateEntry<bool>("Enable Autoconnect", true, "Enable URI Protocol handler");
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
            bool valid = StartupURI.IsValidJoinLink();
            MelonLogger.Msg("Checking StartupURI: {0}:{1} ({2})", StartupURI.WorldId, StartupURI.InstanceId, valid ? "Valid" : "Invalid");
            if (valid) {
                StartupURI.Join();
            }
        }
    }

    public static void GenerateReconnectScript(CVRUrl uri) {
        string filename = Process.GetCurrentProcess().ProcessName + ".exe";
        string args = "";
        foreach (var arg in Environment.GetCommandLineArgs()) {
            if (!Uri.TryCreate(arg, UriKind.Absolute, out _))
                args += arg+" ";
        }
        args += uri.ToString();
        File.WriteAllText("reconnect.bat", string.Format(bat_template, filename, 3, args.Replace("%", "%%").Trim()));
    }
}