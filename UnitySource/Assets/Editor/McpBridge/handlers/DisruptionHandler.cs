using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace McpBridge.Handlers
{
    public static class DisruptionHandler
    {
        private static GameObject GetOrCreateDisruptionsParent()
        {
            var parent = GameObject.Find("Kitchen/Disruptions");
            if (parent != null) return parent;
            var kitchen = GameObject.Find("Kitchen") ?? new GameObject("Kitchen");
            var d = new GameObject("Disruptions");
            d.transform.SetParent(kitchen.transform);
            Undo.RegisterCreatedObjectUndo(d, "MCP Disruptions Parent");
            return d;
        }

        private static Vector3 ParseVec3(JToken token)
        {
            return new Vector3(
                token["x"]?.Value<float>() ?? 0f,
                token["y"]?.Value<float>() ?? 0f,
                token["z"]?.Value<float>() ?? 0f
            );
        }

        public static UnityResponse AddMovingPlatform(UnityCommand cmd)
        {
            float speed = cmd.Payload["speed"]?.Value<float>() ?? 2f;
            string loopMode = cmd.Payload["loop_mode"]?.Value<string>() ?? "ping_pong";
            var path = cmd.Payload["path"] as JArray;

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = $"MovingPlatform_{System.Guid.NewGuid().ToString().Substring(0, 6)}";
            platform.transform.SetParent(GetOrCreateDisruptionsParent().transform);
            platform.transform.localScale = new Vector3(2f, 0.2f, 2f);

            if (path != null && path.Count > 0)
                platform.transform.position = ParseVec3(path[0]);

            var r = platform.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.5f, 0.8f, 0.5f) };
            }

            Undo.RegisterCreatedObjectUndo(platform, "MCP Moving Platform");
            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["object_path"] = platform.name,
                ["speed"] = speed,
                ["loop_mode"] = loopMode,
                ["note"] = "Attach MovingPlatformController script and assign waypoints in Inspector."
            });
        }

        public static UnityResponse AddConveyorBelt(UnityCommand cmd)
        {
            var start = ParseVec3(cmd.Payload["start"]);
            var end = ParseVec3(cmd.Payload["end"]);
            float speed = cmd.Payload["speed"]?.Value<float>() ?? 2f;

            var center = (start + end) * 0.5f;
            var belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            belt.name = $"ConveyorBelt_{(int)center.x}_{(int)center.z}";
            belt.transform.SetParent(GetOrCreateDisruptionsParent().transform);
            belt.transform.position = center + new Vector3(0, 0.05f, 0);

            var size = end - start;
            belt.transform.localScale = new Vector3(
                Mathf.Max(0.3f, Mathf.Abs(size.x)),
                0.1f,
                Mathf.Max(0.3f, Mathf.Abs(size.z))
            );

            if (size != Vector3.zero)
                belt.transform.forward = size.normalized;

            var r = belt.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = new Color(0.4f, 0.4f, 0.4f) };

            Undo.RegisterCreatedObjectUndo(belt, "MCP Conveyor Belt");
            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["object_path"] = belt.name,
                ["speed"] = speed,
                ["note"] = "Attach ConveyorBelt script for item transport logic."
            });
        }

        public static UnityResponse AddHazard(UnityCommand cmd)
        {
            string hazardType = cmd.Payload["hazard_type"]?.Value<string>() ?? "fire";
            var pos = ParseVec3(cmd.Payload["position"]);
            float radius = cmd.Payload["radius"]?.Value<float>() ?? 1f;

            var hazard = new GameObject($"Hazard_{hazardType}_{(int)pos.x}_{(int)pos.z}");
            hazard.transform.SetParent(GetOrCreateDisruptionsParent().transform);
            hazard.transform.position = pos;

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(hazard.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * (radius * 2f);

            var r = sphere.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    { color = HazardColor(hazardType) };

            var col = sphere.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Undo.RegisterCreatedObjectUndo(hazard, "MCP Add Hazard");
            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["object_path"] = hazard.name,
                ["hazard_type"] = hazardType,
                ["radius"] = radius,
                ["note"] = $"Attach {CapFirst(hazardType)}Hazard script for effect logic."
            });
        }

        public static UnityResponse AddTimedEvent(UnityCommand cmd)
        {
            string eventType = cmd.Payload["event_type"]?.Value<string>() ?? "unknown";
            float triggerTime = cmd.Payload["trigger_time"]?.Value<float>() ?? 0f;

            var parent = GetOrCreateDisruptionsParent();
            var eventObj = new GameObject($"TimedEvent_{eventType}_{(int)triggerTime}s");
            eventObj.transform.SetParent(parent.transform);
            Undo.RegisterCreatedObjectUndo(eventObj, "MCP Timed Event");

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["object_path"] = eventObj.name,
                ["event_type"] = eventType,
                ["trigger_time"] = triggerTime,
                ["note"] = "Attach DynamicEventTrigger script and configure in Inspector."
            });
        }

        public static UnityResponse AddPortal(UnityCommand cmd)
        {
            string zoneA = cmd.Payload["zone_a"]?.Value<string>() ?? "A";
            string zoneB = cmd.Payload["zone_b"]?.Value<string>() ?? "B";
            var posA = ParseVec3(cmd.Payload["portal_position_a"]);
            var posB = ParseVec3(cmd.Payload["portal_position_b"]);

            var parent = GetOrCreateDisruptionsParent();

            var portalA = CreatePortalObject($"Portal_{zoneA}_to_{zoneB}", posA, new Color(0.5f, 0.2f, 0.9f), parent);
            var portalB = CreatePortalObject($"Portal_{zoneB}_to_{zoneA}", posB, new Color(0.9f, 0.2f, 0.5f), parent);

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["portal_a"] = portalA.name,
                ["portal_b"] = portalB.name,
                ["note"] = "Attach PortalTeleporter scripts and link the two portals together."
            });
        }

        public static UnityResponse ConfigureSchedule(UnityCommand cmd)
        {
            var timeline = cmd.Payload["timeline"] as JArray;
            if (timeline == null)
                return UnityResponse.Fail(cmd.Id, "Missing 'timeline' array");

            var parent = GetOrCreateDisruptionsParent();
            var scheduler = GameObject.Find("Kitchen/Disruptions/EventScheduler");
            if (scheduler == null)
            {
                scheduler = new GameObject("EventScheduler");
                scheduler.transform.SetParent(parent.transform);
                Undo.RegisterCreatedObjectUndo(scheduler, "MCP Event Scheduler");
            }

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["event_count"] = timeline.Count,
                ["note"] = "Attach LevelEventScheduler script and import the timeline JSON."
            });
        }

        private static GameObject CreatePortalObject(string name, Vector3 pos, Color color, GameObject parent)
        {
            var portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            portal.name = name;
            portal.transform.SetParent(parent.transform);
            portal.transform.position = pos;
            portal.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
            var r = portal.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            var col = portal.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            Undo.RegisterCreatedObjectUndo(portal, "MCP Portal");
            return portal;
        }

        private static Color HazardColor(string type) => type switch
        {
            "fire"  => new Color(1f, 0.3f, 0f),
            "ice"   => new Color(0.6f, 0.9f, 1f),
            "water" => new Color(0.2f, 0.5f, 1f),
            "rats"  => new Color(0.4f, 0.3f, 0.2f),
            _       => Color.yellow
        };

        private static string CapFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
