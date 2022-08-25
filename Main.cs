using ABI_RC.Core.Networking;
using AutoConnect;
using AutoConnect.Classes;
using Classes;
using HarmonyLib;
using MelonLoader;
using System;
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
    }
}

public class Main : MelonMod {
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
    }
    public override void OnApplicationStart() {
        MelonPreferences_Category cat = MelonPreferences.CreateCategory(Guh.Name);
        AutoConnectSetting = cat.CreateEntry<bool>("Enable Autoconnect", true, "Enable URI Protocol handler");
        WorldIdSetting = cat.CreateEntry<string>("WorldID", "46c5f43c-4492-40af-94ce-f3ab559ed65c", "Debug World ID");
        InstanceIdSetting = cat.CreateEntry<string>("InstanceID", "i+a94d415425f2546a-415006-3f6e77-1868cdc4", "Debug Instance ID");
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

    public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
        if (!fully_loaded && sceneName == "Virtual Apartment") {
            fully_loaded = true;
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
}