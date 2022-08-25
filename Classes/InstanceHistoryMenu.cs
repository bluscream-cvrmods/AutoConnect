using ChilloutButtonAPI.UI;
using Classes;
using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace AutoConnect.Classes {
    public class InstanceHistoryMenu {
        public SubMenu Parent { get; set; }
        public SubMenu Menu { get; set; }
        public List<string> Instances { get; set; }
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
            string instanceStr = $"{worldId}:{instanceId}";
            if (Instances.Contains(instanceStr)) {
                return null;
            }

            Instances.Add(instanceStr);
            return Menu.AddButton($"{DateTime.Now}\n{instanceId}", instanceStr, () => {
                MelonLogger.Msg("Joining instance: {0}", instanceStr);
                ABI_RC.Core.Networking.IO.Instancing.Instances.SetJoinTarget(instanceId, worldId);
            });
        }
    }
}
