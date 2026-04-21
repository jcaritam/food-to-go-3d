using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace McpBridge.Handlers
{
    public static class PlayerHandler
    {
        private static GameObject GetOrCreatePlayersParent()
        {
            var parent = GameObject.Find("Kitchen/Players");
            if (parent != null) return parent;
            var kitchen = GameObject.Find("Kitchen") ?? new GameObject("Kitchen");
            var p = new GameObject("Players");
            p.transform.SetParent(kitchen.transform);
            Undo.RegisterCreatedObjectUndo(p, "MCP Players Parent");
            return p;
        }

        private static Vector3 ParseVec3(JToken token)
        {
            return new Vector3(
                token["x"]?.Value<float>() ?? 0f,
                token["y"]?.Value<float>() ?? 0f,
                token["z"]?.Value<float>() ?? 0f
            );
        }

        private static readonly Color[] PlayerColors = {
            new Color(0.2f, 0.6f, 1.0f),
            new Color(1.0f, 0.4f, 0.2f),
            new Color(0.3f, 0.9f, 0.3f),
            new Color(0.9f, 0.3f, 0.9f)
        };

        public static UnityResponse SetPlayerCount(UnityCommand cmd)
        {
            int count = cmd.Payload["count"]?.Value<int>() ?? 2;
            count = Mathf.Clamp(count, 1, 4);

            var parent = GetOrCreatePlayersParent();

            for (int i = 0; i < count; i++)
            {
                if (GameObject.Find($"Kitchen/Players/Player_{i}") != null) continue;

                var playerObj = new GameObject($"Player_{i}");
                playerObj.transform.SetParent(parent.transform);
                playerObj.transform.position = new Vector3(i * 2f, 0.5f, 0f);

                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.SetParent(playerObj.transform);
                body.transform.localPosition = Vector3.zero;
                var r = body.GetComponent<Renderer>();
                if (r != null)
                    r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                        { color = PlayerColors[i] };

                Undo.RegisterCreatedObjectUndo(playerObj, "MCP Player");
            }

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["player_count"] = count,
                ["note"] = "Attach PlayerController.cs to each Player_N object."
            });
        }

        public static UnityResponse PlaceSpawnPoint(UnityCommand cmd)
        {
            int playerIndex = cmd.Payload["player_index"]?.Value<int>() ?? 0;
            var pos = ParseVec3(cmd.Payload["position"]);

            string spawnName = $"SpawnPoint_P{playerIndex}";
            var existing = GameObject.Find($"Kitchen/Players/{spawnName}");

            if (existing == null)
            {
                existing = new GameObject(spawnName);
                existing.transform.SetParent(GetOrCreatePlayersParent().transform);
                Undo.RegisterCreatedObjectUndo(existing, "MCP Spawn Point");
            }

            Undo.RecordObject(existing.transform, "MCP Move Spawn");
            existing.transform.position = pos;

            var player = GameObject.Find($"Kitchen/Players/Player_{playerIndex}");
            if (player != null)
            {
                Undo.RecordObject(player.transform, "MCP Move Player");
                player.transform.position = pos + new Vector3(0, 0.5f, 0);
            }

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["spawn_point"] = spawnName,
                ["position"] = new JObject { ["x"] = pos.x, ["y"] = pos.y, ["z"] = pos.z }
            });
        }

        public static UnityResponse ConfigureControls(UnityCommand cmd)
        {
            int playerIndex = cmd.Payload["player_index"]?.Value<int>() ?? 0;
            var mapping = cmd.Payload["mapping"] as JObject;

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["player_index"] = playerIndex,
                ["mapping"] = mapping ?? new JObject(),
                ["note"] = "Apply these mappings to the PlayerInput component or InputActionAsset in Unity."
            });
        }

        public static UnityResponse SetPlayerAbilities(UnityCommand cmd)
        {
            int playerIndex = cmd.Payload["player_index"]?.Value<int>() ?? 0;
            bool canDash = cmd.Payload["can_dash"]?.Value<bool>() ?? true;
            bool canThrow = cmd.Payload["can_throw"]?.Value<bool>() ?? true;
            float interactRange = cmd.Payload["interact_range"]?.Value<float>() ?? 1.5f;

            var player = GameObject.Find($"Kitchen/Players/Player_{playerIndex}");
            if (player == null)
                return UnityResponse.Fail(cmd.Id, $"Player_{playerIndex} not found. Call set_player_count first.");

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["player_index"] = playerIndex,
                ["can_dash"] = canDash,
                ["can_throw"] = canThrow,
                ["interact_range"] = interactRange,
                ["note"] = "Set these fields on the PlayerController component in the Inspector."
            });
        }
    }
}
