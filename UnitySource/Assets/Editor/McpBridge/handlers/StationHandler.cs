using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace McpBridge.Handlers
{
    public static class StationHandler
    {
        private static GameObject GetOrCreateStationsParent()
        {
            var parent = GameObject.Find("Kitchen/Stations");
            if (parent != null) return parent;
            var kitchen = GameObject.Find("Kitchen") ?? new GameObject("Kitchen");
            var stations = new GameObject("Stations");
            stations.transform.SetParent(kitchen.transform);
            Undo.RegisterCreatedObjectUndo(stations, "MCP Stations Parent");
            return stations;
        }

        private static Vector3 ParseVec3(JToken token)
        {
            return new Vector3(
                token["x"]?.Value<float>() ?? 0f,
                token["y"]?.Value<float>() ?? 0f,
                token["z"]?.Value<float>() ?? 0f
            );
        }

        private static GameObject CreateStationObject(string typeName, Vector3 pos, float rotation)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"{CapFirst(typeName)}_{(int)pos.x}_{(int)pos.z}";
            go.transform.SetParent(GetOrCreateStationsParent().transform);
            go.transform.position = pos + new Vector3(0, 0.5f, 0);
            go.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            go.transform.eulerAngles = new Vector3(0f, rotation, 0f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = StationColor(typeName)
                };
            }

            Undo.RegisterCreatedObjectUndo(go, $"MCP Place {typeName}");
            return go;
        }

        private static Color StationColor(string type) => type switch
        {
            "chopping" => new Color(0.7f, 0.9f, 0.7f),
            "stove"    => new Color(0.9f, 0.5f, 0.3f),
            "oven"     => new Color(0.8f, 0.4f, 0.2f),
            "fryer"    => new Color(0.9f, 0.8f, 0.3f),
            "sink"     => new Color(0.4f, 0.6f, 0.9f),
            _          => Color.gray
        };

        private static string CapFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static UnityResponse PlaceStation(UnityCommand cmd)
        {
            string stationType = cmd.Payload["station_type"]?.Value<string>() ?? "chopping";
            var pos = ParseVec3(cmd.Payload["position"]);
            float rotation = cmd.Payload["rotation"]?.Value<float>() ?? 0f;
            var go = CreateStationObject(stationType, pos, rotation);
            return UnityResponse.Success(cmd.Id, new JObject { ["object_path"] = go.name });
        }

        public static UnityResponse PlaceCounter(UnityCommand cmd)
        {
            var pos = ParseVec3(cmd.Payload["position"]);
            float rotation = cmd.Payload["rotation"]?.Value<float>() ?? 0f;
            var go = CreateStationObject("counter", pos, rotation);
            go.GetComponent<Renderer>().sharedMaterial.color = new Color(0.85f, 0.8f, 0.7f);
            return UnityResponse.Success(cmd.Id, new JObject { ["object_path"] = go.name });
        }

        public static UnityResponse PlaceServingHatch(UnityCommand cmd)
        {
            var pos = ParseVec3(cmd.Payload["position"]);
            string facing = cmd.Payload["facing"]?.Value<string>() ?? "north";
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"ServingHatch_{(int)pos.x}_{(int)pos.z}";
            go.transform.SetParent(GetOrCreateStationsParent().transform);
            go.transform.position = pos + new Vector3(0, 0.5f, 0);
            go.transform.localScale = new Vector3(0.9f, 0.3f, 0.9f);
            go.transform.eulerAngles = new Vector3(0f, FacingAngle(facing), 0f);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.3f, 0.8f, 0.3f) };
            Undo.RegisterCreatedObjectUndo(go, "MCP Place Serving Hatch");
            return UnityResponse.Success(cmd.Id, new JObject { ["object_path"] = go.name });
        }

        public static UnityResponse PlaceIngredientCrate(UnityCommand cmd)
        {
            var pos = ParseVec3(cmd.Payload["position"]);
            string ingredientId = cmd.Payload["ingredient_id"]?.Value<string>() ?? "unknown";
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Crate_{ingredientId}_{(int)pos.x}_{(int)pos.z}";
            go.transform.SetParent(GetOrCreateStationsParent().transform);
            go.transform.position = pos + new Vector3(0, 0.5f, 0);
            go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.9f, 0.75f, 0.4f) };
            Undo.RegisterCreatedObjectUndo(go, "MCP Place Crate");
            return UnityResponse.Success(cmd.Id, new JObject { ["object_path"] = go.name, ["ingredient_id"] = ingredientId });
        }

        public static UnityResponse PlaceTrash(UnityCommand cmd)
        {
            var pos = ParseVec3(cmd.Payload["position"]);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = $"Trash_{(int)pos.x}_{(int)pos.z}";
            go.transform.SetParent(GetOrCreateStationsParent().transform);
            go.transform.position = pos + new Vector3(0, 0.5f, 0);
            go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.3f, 0.3f, 0.3f) };
            Undo.RegisterCreatedObjectUndo(go, "MCP Place Trash");
            return UnityResponse.Success(cmd.Id, new JObject { ["object_path"] = go.name });
        }

        public static UnityResponse PlaceDishReturn(UnityCommand cmd)
        {
            var pos = ParseVec3(cmd.Payload["position"]);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"DishReturn_{(int)pos.x}_{(int)pos.z}";
            go.transform.SetParent(GetOrCreateStationsParent().transform);
            go.transform.position = pos + new Vector3(0, 0.25f, 0);
            go.transform.localScale = new Vector3(0.9f, 0.5f, 0.9f);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.5f, 0.7f, 0.9f) };
            Undo.RegisterCreatedObjectUndo(go, "MCP Place Dish Return");
            return UnityResponse.Success(cmd.Id, new JObject { ["object_path"] = go.name });
        }

        public static UnityResponse ConfigureStation(UnityCommand cmd)
        {
            string targetPath = cmd.Payload["target_path"]?.Value<string>() ?? "";
            var target = GameObject.Find(targetPath);
            if (target == null)
                return UnityResponse.Fail(cmd.Id, $"Station not found: {targetPath}");

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["configured"] = targetPath,
                ["note"] = "Station script properties must be set via Inspector or generated scripts"
            });
        }

        private static float FacingAngle(string facing) => facing switch
        {
            "south" => 180f,
            "east"  => 90f,
            "west"  => 270f,
            _       => 0f
        };
    }
}
