using ABI_RC.Core.Networking;
using ABI_RC.Core.Player;
using AutoConnect;
using Classes;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ButtonAPI = ChilloutButtonAPI.ChilloutButtonAPIMain;

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
            //MelonLogger.Msg("Patching PopulatePresenceFromNetwork");
            //_ = harmonyInstance.Patch(typeof(ABI_RC.Core.Networking.RichPresence).GetMethod("PopulatePresenceFromNetwork"), prefix: new HarmonyMethod(typeof(Patches).GetMethod("PopulatePresenceFromNetwork")));
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
        MelonLogger.Msg("SetJoinTarget: {0}:{1}", worldId, instanceId);
        if (ButtonAPI.HasInit) {
            _ = Main.instanceHistoryMenu.Add(worldId, instanceId);
            // Main.instanceHistory.Add($"{worldId}:{instanceId}");
        }
        InstanceHistory.Add(worldId, instanceId);
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
    public static InstanceHistoryMenu instanceHistoryMenu;
    // public static ObservableCollection<string> instanceHistory = new ObservableCollection<string>();
    public CVRUrl StartupURI;
    public MelonPreferences_Entry AutoConnectSetting, WorldIdSetting, InstanceIdSetting;

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
        WorldIdSetting = cat.CreateEntry<string>("WorldID", "46c5f43c-4492-40af-94ce-f3ab559ed65c", "Debug World ID");
        InstanceIdSetting = cat.CreateEntry<string>("InstanceID", "i+a94d415425f2546a-415006-3f6e77-1868cdc4", "Debug Instance ID");
        InstanceHistory.Init("UserData/InstanceHistory.json");
        ButtonAPI.OnInit += ButtonAPI_OnInit;
        // instanceHistory = new ObservableCollection<string>();
        // instanceHistory.CollectionChanged += InstanceHistory_CollectionChanged;
        Patches.Init(HarmonyInstance);
    }

    private void ButtonAPI_OnInit() {
        instanceHistoryMenu = new InstanceHistoryMenu(ButtonAPI.MainPage);
        string worldId = (string)WorldIdSetting.BoxedValue;
        string instanceId = (string)InstanceIdSetting.BoxedValue;
        ButtonAPI.MainPage.AddButton("Connect", InstanceHistoryMenu.GetInstanceToolTip(worldId, instanceId), () => {
            CVRUrl.CreateJoinURI(worldId, instanceId).Join();
        });
    }

    //private void InstanceHistory_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
    //    var instance = ((string)e.NewItems[0]).Split(":");
    //    instanceHistoryMenu.Add(instance[0], instance[1]);
    //}
    //bool waitForNextSceneForInit = false;
    //public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
    //    LoggerInstance.Msg("!fully_loaded: {0}", !fully_loaded);
    //    LoggerInstance.Msg("!waitForNextSceneForInit: {0}", !waitForNextSceneForInit);
    //    LoggerInstance.Msg("buildIndex == 1: {0}", buildIndex == 1);
    //    LoggerInstance.Msg("sceneName == \"Headquarters\": {0}", sceneName == "Headquarters");
    //    if (!fully_loaded && !waitForNextSceneForInit && buildIndex == 1 && sceneName == "Headquarters") {
    //        LoggerInstance.Msg("Waiting for next scene to init...");
    //        waitForNextSceneForInit = true;
    //    }
    //}

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