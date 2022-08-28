using ABI_RC.Core.Player;
using ABI_RC.Systems.MovementSystem;
using Assets.ABI_RC.Systems.Safety.AdvancedSafety;
using Bluscream;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Web;

namespace Classes {
    // vrchat://launch?ref=vrchat.com&id=wrld_a6e75419-0f76-402b-966e-3dc8b79a6b30:98085~region(eu)&shortName=a9k0njd0

    // cvr://launch?id=4b60d4ec-7043-4453-82fe-b976a8500a3c:i+7d523258924e4251-559101-94d7bb-16ca42ed&pos=14.0,2.6,75.6&rot=0.0,0.1,0.0,1.0

    // furhub: b307fa06-ed5a-4ec1-8ccd-7ba7b31d063f:i+448bf2c7048b3c58-824507-eb572d-122416f1
    // 46c5f43c-4492-40af-94ce-f3ab559ed65c:i+a94d415425f2546a-415006-3f6e77-1868cdc4
    // 46c5f43c-4492-40af-94ce-f3ab559ed65c:i+a94d415425f2546a-415006-3f6e77-1868cdc4
    // b307fa06-ed5a-4ec1-8ccd-7ba7b31d063f:i+448bf2c7048b3c58-824507-eb572d-122416f1
    public class CVRUrl {
        public Uri Uri { get; set; }
        public string Command { get; set; }
        public List<string> Arguments { get; set; } = new List<string>();
        public string WorldId { get; set; }
        public string InstanceId { get; set; }
        public UnityEngine.Vector3? Position { get; set; }
        public UnityEngine.Quaternion? Rotation { get; set; }
        public CVRUrl(Uri uri) {
            Uri = uri;
            Parse();
        }
        public CVRUrl(string uriString) {
            Uri = new Uri(uriString);
            Parse();
        }
        public CVRUrl(string command, List<string> arguments = null, string worldId = null, string instanceId = null, Vector3? position = null, Quaternion? rotation = null ) {
            UriBuilder builder = new("cvr", command);
            if (arguments != null) builder.Path = string.Join("/", arguments.ToArray());
            var query = HttpUtility.ParseQueryString(builder.Query);
            if (!string.IsNullOrEmpty(worldId) && !string.IsNullOrEmpty(instanceId)) {
                query["id"] = $"{worldId}:{instanceId}";
            }
            if (position != null) query["pos"] = $"{position.Value.X}:{position.Value.Y},{position.Value.Z}";
            if (rotation != null) query["rot"] = $"{rotation.Value.X}:{rotation.Value.Y},{rotation.Value.Z},{rotation.Value.W}";
            builder.Query = query.ToString();
            Uri = builder.Uri;
            Parse();
        }
        public void Parse() {
            if (!IsValid()) {
                throw new Exception($"uri \"{Uri}\" is not a valid cvr:// url!");
            }
            Command = Uri.Host;
            Arguments = Uri.Segments.ToList();
            string worldstr = Uri.CVRGetInstance();
            if (worldstr.Contains(":")) {
                string[] worldarray = worldstr.Split(':');
                WorldId = worldarray[0];
                InstanceId = worldarray[1]; // .Split('+').Last();
            } else {
                WorldId = worldstr;
            }
            var posStr = Uri.CVRGetPosition();
            if (posStr != null) {
                Position = posStr.ParseVector3().Value;
            }
            var rotStr = Uri.CVRGetRotation();
            if (rotStr != null) {
                Rotation = rotStr.ParseQuaternion().Value;
            }
        }
        public static CVRUrl CreateJoinURI(string worldId, string instanceId) => new CVRUrl("launch", null, worldId, instanceId);
        public void Join() {
            MelonLogger.Msg("Joining instance: {0}:{1}", WorldId, InstanceId);
            ABI_RC.Core.Networking.IO.Instancing.Instances.SetJoinTarget(InstanceId, WorldId);
        }
        public bool HasTeleport() {
            return (Position.Value != null && !Position.Value.isZero()) || (Rotation.Value != null && !Rotation.Value.isZero());
        }
        public void Teleport() {
            if (Position != null) {
                if (Rotation != null) MovementSystem.Instance.TeleportToPosRot(Position.Value, Rotation.Value);
                else MovementSystem.Instance.TeleportToPosRot(Position.Value, PlayerSetup.Instance.gameObject.transform.rotation);
            } else if (Rotation != null) MovementSystem.Instance.TeleportToPosRot(PlayerSetup.Instance.gameObject.transform.position, Rotation.Value);
        }
        public bool IsValid() => Uri.CVRIsValid();
        public bool IsValidJoinLink() {
            if (string.IsNullOrWhiteSpace(WorldId) || !Guid.TryParse(WorldId, out _)) {
                return false;
            }
            if (string.IsNullOrWhiteSpace(InstanceId)) return false;
            return true;
        }
        public override string ToString() {
            return Uri.ToString();
        }
        public string ToDebugString() {
            var builder = new StringBuilder();
            builder.AppendLine($"Uri: {ToString()}");
            builder.AppendLine($"Command: {Command}");
            builder.AppendLine($"Arguments: {string.Join(", ", Arguments)}");
            builder.AppendLine($"WorldId: {WorldId}");
            builder.AppendLine($"InstanceId: {InstanceId}");
            builder.AppendLine($"Position: {Position}");
            builder.Append($"Rotation: {Rotation}");
            return builder.ToString();
        }
    }
}
