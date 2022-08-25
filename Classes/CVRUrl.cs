using Bluscream;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Web;

namespace Classes {
    // vrchat://launch?ref=vrchat.com&id=wrld_a6e75419-0f76-402b-966e-3dc8b79a6b30:98085~region(eu)&shortName=a9k0njd0
    // cvr://launch?id=1626f8bb-e113-4132-bf3b-0d20b909c7cb:i+1359e08ef166f26f-581507-aaad7a-103b2a61
    // furhub: b307fa06-ed5a-4ec1-8ccd-7ba7b31d063f:i+448bf2c7048b3c58-824507-eb572d-122416f1
    // 46c5f43c-4492-40af-94ce-f3ab559ed65c:i+a94d415425f2546a-415006-3f6e77-1868cdc4
    // 46c5f43c-4492-40af-94ce-f3ab559ed65c:i+a94d415425f2546a-415006-3f6e77-1868cdc4
    // b307fa06-ed5a-4ec1-8ccd-7ba7b31d063f:i+448bf2c7048b3c58-824507-eb572d-122416f1
    public class CVRUrl : Uri {
        public Dictionary<string, string> QueryDict => Query.ParseQueryString();
        public string WorldId { get; set; }
        public string InstanceId { get; set; }
        public CVRUrl(string uriString) : base(uriString) {
            if (!IsValid()) {
                throw new Exception($"uriString \"{uriString}\" is not a valid cvr:// url!");
            }

            if (QueryDict.ContainsKey("id")) {
                string worldstr = QueryDict["id"].Replace("%3A", ":").Replace(" ", "+");
                if (worldstr.Contains(":")) {
                    string[] worldarray = worldstr.Split(':');
                    WorldId = worldarray[0];
                    InstanceId = worldarray[1]; // .Split('+').Last();
                } else {
                    WorldId = worldstr;
                }
            }
        }
        public static CVRUrl CreateJoinURI(string worldId, string instanceId) {
            UriBuilder builder = new("cvr", "launch");
            System.Collections.Specialized.NameValueCollection query = HttpUtility.ParseQueryString(builder.Query);
            query["id"] = $"{worldId}:{instanceId}";
            builder.Query = query.ToString();
            return new CVRUrl(builder.ToString());
        }
        public void Join() {
            MelonLogger.Msg("Joining instance: {0}:{1}", WorldId, InstanceId);
            ABI_RC.Core.Networking.IO.Instancing.Instances.SetJoinTarget(InstanceId, WorldId);
        }
        public bool IsValid() {
            return Scheme.Equals("cvr", StringComparison.OrdinalIgnoreCase);
        }
        public bool IsValidJoinLink() {
            if (!QueryDict.ContainsKey("id")) {
                return false;
            }

            if (!Guid.TryParse(WorldId, out _)) {
                return false;
            }

            if (InstanceId.Contains("+")) {
                // var t = InstanceId.Split("+");
                // if (!Guid.TryParse(t[1], out _)) return false;
            } else {
                if (!Guid.TryParse(InstanceId, out _)) {
                    return false;
                }
            }
            return true;
        }
    }
}
