using ChilloutButtonAPI.UI;
using Classes;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Policy;
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
        public GameObject Add(CVRUrl url) => Add(url.WorldId, url.InstanceId, DateTime.Now);
        public GameObject Add(string worldId, string instanceId) => Add(worldId, instanceId, DateTime.Now);
        public GameObject Add(string worldId, string instanceId, DateTimeOffset timestamp) {
            return Menu.AddButton($"{timestamp.LocalDateTime}", GetInstanceToolTip(worldId, instanceId), () => {
                ABI_RC.Core.Networking.IO.Instancing.Instances.SetJoinTarget(instanceId, worldId);
            });
        }

        public static string GetInstanceToolTip(string worldId, string instanceId) {
            return $"WorldID: {worldId}\nInstanceID: {instanceId}";
        }
    }
}
