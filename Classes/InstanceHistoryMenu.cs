using ChilloutButtonAPI.UI;
using Classes;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace AutoConnect {
    public class InstanceHistoryMenu {
        public SubMenu Parent { get; set; }
        public SubMenu Menu { get; set; }
        public InstanceHistoryMenu(SubMenu parent) {
            Parent = parent;
            _ = Create();
        }
        public InstanceHistoryMenu Create() {
            Menu = Parent.AddSubMenu("Instance History", "Instance History");
            return this;
        }
        public GameObject Add(CVRUrl url) {
            return Add(url.WorldId, url.InstanceId);
        }

        public GameObject Add(string worldId, string instanceId) {
            return Menu.AddButton($"{DateTime.Now}", GetInstanceToolTip(worldId, instanceId), () => {
                ABI_RC.Core.Networking.IO.Instancing.Instances.SetJoinTarget(instanceId, worldId);
            });
        }

        public static string GetInstanceToolTip(string worldId, string instanceId) {
            return $"WorldID: {worldId}\nInstanceID: {instanceId}";
        }
    }
}
