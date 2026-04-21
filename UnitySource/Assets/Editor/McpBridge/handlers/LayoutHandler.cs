using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace McpBridge.Handlers
{
    public static class LayoutHandler
    {
        private static GameObject GetOrCreateKitchenRoot()
        {
            var existing = GameObject.Find("Kitchen");
            if (existing != null) return existing;
            var root = new GameObject("Kitchen");
            Undo.RegisterCreatedObjectUndo(root, "MCP Create Kitchen Root");
            return root;
        }

        public static UnityResponse GenerateLayout(UnityCommand cmd)
        {
            var grid = cmd.Payload["grid"];
            int width = grid["width"].Value<int>();
            int height = grid["height"].Value<int>();
            string theme = cmd.Payload["theme"]?.Value<string>() ?? "default";

            var root = GetOrCreateKitchenRoot();

            var floorsParent = new GameObject("Floors");
            floorsParent.transform.SetParent(root.transform);
            Undo.RegisterCreatedObjectUndo(floorsParent, "MCP Generate Layout");

            for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Floor_{x}_{z}";
                tile.transform.SetParent(floorsParent.transform);
                tile.transform.position = new Vector3(x, -0.5f, z);
                tile.transform.localScale = new Vector3(1f, 0.1f, 1f);
                Undo.RegisterCreatedObjectUndo(tile, "MCP Tile");
            }

            ApplyTheme(root, theme);

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["root_path"] = root.name,
                ["tile_count"] = width * height,
                ["theme"] = theme
            });
        }

        public static UnityResponse SetTile(UnityCommand cmd)
        {
            var pos = ParseVec3(cmd.Payload["position"]);
            string tileType = cmd.Payload["tile_type"]?.Value<string>() ?? "floor";

            string tileName = $"Floor_{(int)pos.x}_{(int)pos.z}";
            var existing = GameObject.Find($"Kitchen/Floors/{tileName}");

            if (existing == null)
            {
                existing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                existing.name = tileName;
                var floorsParent = GameObject.Find("Kitchen/Floors");
                if (floorsParent != null) existing.transform.SetParent(floorsParent.transform);
                existing.transform.position = new Vector3(pos.x, -0.5f, pos.z);
                existing.transform.localScale = new Vector3(1f, 0.1f, 1f);
                Undo.RegisterCreatedObjectUndo(existing, "MCP Set Tile");
            }

            Undo.RecordObject(existing, "MCP Set Tile Type");
            existing.tag = tileType == "wall" ? "Untagged" : "Untagged";
            existing.name = $"{CapitalizeFirst(tileType)}_{(int)pos.x}_{(int)pos.z}";

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["object_path"] = GetPath(existing),
                ["tile_type"] = tileType
            });
        }

        public static UnityResponse SplitKitchen(UnityCommand cmd)
        {
            var root = GetOrCreateKitchenRoot();
            var zones = cmd.Payload["zones"] as JArray;
            var created = new JArray();

            foreach (var zone in zones)
            {
                string name = zone["name"].Value<string>();
                var zoneObj = new GameObject($"Zone_{name}");
                zoneObj.transform.SetParent(root.transform);
                zoneObj.transform.position = new Vector3(
                    zone["x"].Value<float>(),
                    0f,
                    zone["y"].Value<float>()
                );
                Undo.RegisterCreatedObjectUndo(zoneObj, "MCP Split Kitchen");
                created.Add(zoneObj.name);
            }

            return UnityResponse.Success(cmd.Id, new JObject { ["zones_created"] = created });
        }

        public static UnityResponse AddObstacle(UnityCommand cmd)
        {
            string obsType = cmd.Payload["obstacle_type"]?.Value<string>() ?? "wall";
            var pos = ParseVec3(cmd.Payload["position"]);

            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = $"Obstacle_{obsType}_{(int)pos.x}_{(int)pos.z}";

            var root = GetOrCreateKitchenRoot();
            var obsParent = GameObject.Find("Kitchen/Obstacles");
            if (obsParent == null)
            {
                obsParent = new GameObject("Obstacles");
                obsParent.transform.SetParent(root.transform);
                Undo.RegisterCreatedObjectUndo(obsParent, "MCP Obstacles Parent");
            }

            obstacle.transform.SetParent(obsParent.transform);
            obstacle.transform.position = pos;

            switch (obsType)
            {
                case "wall":
                    obstacle.transform.localScale = new Vector3(1f, 2f, 1f);
                    obstacle.transform.position = new Vector3(pos.x, 0.5f, pos.z);
                    break;
                case "gap":
                    Object.DestroyImmediate(obstacle);
                    var floor = GameObject.Find($"Kitchen/Floors/Floor_{(int)pos.x}_{(int)pos.z}");
                    if (floor != null) Undo.DestroyObjectImmediate(floor);
                    return UnityResponse.Success(cmd.Id, new JObject { ["removed_tile"] = true });
                default:
                    obstacle.transform.localScale = Vector3.one;
                    break;
            }

            Undo.RegisterCreatedObjectUndo(obstacle, "MCP Add Obstacle");
            return UnityResponse.Success(cmd.Id, new JObject { ["object_path"] = GetPath(obstacle) });
        }

        public static UnityResponse SetTheme(UnityCommand cmd)
        {
            string themeName = cmd.Payload["theme_name"]?.Value<string>() ?? "default";
            var root = GetOrCreateKitchenRoot();
            ApplyTheme(root, themeName);
            return UnityResponse.Success(cmd.Id, new JObject { ["theme_applied"] = themeName });
        }

        public static UnityResponse GetLayout(UnityCommand cmd)
        {
            var result = new JObject();
            var tiles = new JArray();

            var kitchen = GameObject.Find("Kitchen");
            if (kitchen != null)
            {
                foreach (Transform child in kitchen.transform)
                {
                    foreach (Transform tile in child)
                    {
                        var p = tile.position;
                        tiles.Add(new JObject
                        {
                            ["name"] = tile.name,
                            ["x"] = p.x,
                            ["y"] = p.y,
                            ["z"] = p.z
                        });
                    }
                }
            }

            result["tiles"] = tiles;
            result["tile_count"] = tiles.Count;
            return UnityResponse.Success(cmd.Id, result);
        }

        private static void ApplyTheme(GameObject root, string theme)
        {
            Color baseColor = theme switch
            {
                "restaurant" => new Color(0.85f, 0.75f, 0.60f),
                "food-truck"  => new Color(0.60f, 0.70f, 0.80f),
                "ship"        => new Color(0.40f, 0.55f, 0.65f),
                _             => new Color(0.72f, 0.72f, 0.72f)
            };

            var renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.gameObject.name.StartsWith("Floor_") || r.gameObject.name.StartsWith("Wall_"))
                {
                    r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    r.sharedMaterial.color = baseColor;
                }
            }
        }

        private static Vector3 ParseVec3(JToken token)
        {
            return new Vector3(
                token["x"]?.Value<float>() ?? 0f,
                token["y"]?.Value<float>() ?? 0f,
                token["z"]?.Value<float>() ?? 0f
            );
        }

        private static string GetPath(GameObject go)
        {
            return go == null ? "" : go.name;
        }

        private static string CapitalizeFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
