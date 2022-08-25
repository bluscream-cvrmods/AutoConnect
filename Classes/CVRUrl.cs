using Bluscream;
using MelonLoader;
using System;
using System.Collections.Generic;

namespace Classes {
    // vrchat://launch?ref=vrchat.com&id=wrld_a6e75419-0f76-402b-966e-3dc8b79a6b30:98085~region(eu)&shortName=a9k0njd0
    // cvr://launch?id=1626f8bb-e113-4132-bf3b-0d20b909c7cb:i+1359e08ef166f26f-581507-aaad7a-103b2a61
    public class CVRUrl : Uri {
        public Dictionary<string, string> QueryDict => Query.ParseQueryString();
        public string WorldId { get; set; }
        public string InstanceId { get; set; }
        public CVRUrl(string uriString) : base(uriString) {
            if (!IsValid()) {
                throw new Exception($"uriString \"{uriString}\" is not a valid cvr:// url!");
            }

            if (QueryDict.ContainsKey("id")) {
                string worldstr = QueryDict["id"];
                if (worldstr.Contains(":")) {
                    string[] worldarray = worldstr.Split(':');
                    WorldId = worldarray[0];
                    InstanceId = worldarray[1];
                } else {
                    WorldId = worldstr;
                }
            }
        }
        public void Join() {
            MelonLogger.Msg("Joining instance: {0}:{1}", WorldId, InstanceId);
            ABI_RC.Core.Networking.IO.Instancing.Instances.SetJoinTarget(InstanceId, WorldId);
        }
        public bool IsValid() {
            return Scheme.Equals("cvr", StringComparison.OrdinalIgnoreCase);
        }
        public bool IsValidJoinLink() {
            if (!QueryDict.ContainsKey("id")) return false;
            if (!Guid.TryParse(WorldId, out _)) return false;
            if (InstanceId.Contains("+")) {
                // var t = InstanceId.Split("+");
                // if (!Guid.TryParse(t[1], out _)) return false;
            } else {
                if (!Guid.TryParse(InstanceId, out _)) return false;
            }
            return true;
        }
    }
}
