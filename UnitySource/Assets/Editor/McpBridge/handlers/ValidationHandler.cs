using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace McpBridge.Handlers
{
    public static class ValidationHandler
    {
        private static Vector3 ParseVec3(JToken token)
        {
            return new Vector3(
                token["x"]?.Value<float>() ?? 0f,
                token["y"]?.Value<float>() ?? 0f,
                token["z"]?.Value<float>() ?? 0f
            );
        }

        public static UnityResponse ValidateReachability(UnityCommand cmd)
        {
            var kitchen = GameObject.Find("Kitchen");
            if (kitchen == null)
                return UnityResponse.Fail(cmd.Id, "No Kitchen found in scene. Call generate_layout first.");

            var stations = new List<Vector3>();
            var stationsParent = GameObject.Find("Kitchen/Stations");
            if (stationsParent != null)
            {
                foreach (Transform child in stationsParent.transform)
                    stations.Add(child.position);
            }

            var spawnPoints = new List<Vector3>();
            var playersParent = GameObject.Find("Kitchen/Players");
            if (playersParent != null)
            {
                foreach (Transform child in playersParent.transform)
                    if (child.name.StartsWith("SpawnPoint_"))
                        spawnPoints.Add(child.position);
            }

            bool passable = stations.Count > 0 && spawnPoints.Count > 0;
            var issues = new JArray();

            if (stations.Count == 0) issues.Add("No stations placed");
            if (spawnPoints.Count == 0) issues.Add("No spawn points placed");

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["reachable"] = passable,
                ["station_count"] = stations.Count,
                ["spawn_count"] = spawnPoints.Count,
                ["issues"] = issues,
                ["note"] = "Full pathfinding BFS requires NavMesh baking. This is a basic structural check."
            });
        }

        public static UnityResponse AnalyzeDifficulty(UnityCommand cmd)
        {
            int stationCount = 0;
            var stationsParent = GameObject.Find("Kitchen/Stations");
            if (stationsParent != null)
                stationCount = stationsParent.transform.childCount;

            int disruptionCount = 0;
            var disruptionsParent = GameObject.Find("Kitchen/Disruptions");
            if (disruptionsParent != null)
                disruptionCount = disruptionsParent.transform.childCount;

            int recipeCount = 0;
            try
            {
                var recipeFiles = System.IO.Directory.GetFiles(
                    System.IO.Path.Combine(Application.dataPath, "Recipes"), "*.json");
                recipeCount = recipeFiles.Length;
            }
            catch { }

            float score = (stationCount * 10f) + (recipeCount * 15f) + (disruptionCount * 20f);

            string rating = score switch
            {
                < 50  => "easy",
                < 100 => "medium",
                < 200 => "hard",
                _     => "expert"
            };

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["difficulty_score"] = score,
                ["rating"] = rating,
                ["station_count"] = stationCount,
                ["recipe_count"] = recipeCount,
                ["disruption_count"] = disruptionCount
            });
        }

        public static UnityResponse CheckCompletability(UnityCommand cmd)
        {
            var stationsParent = GameObject.Find("Kitchen/Stations");
            if (stationsParent == null)
                return UnityResponse.Fail(cmd.Id, "No stations placed in scene.");

            var stationTypes = new HashSet<string>();
            foreach (Transform child in stationsParent.transform)
            {
                string n = child.name.ToLower();
                if (n.Contains("chopping")) stationTypes.Add("chopping");
                else if (n.Contains("stove")) stationTypes.Add("stove");
                else if (n.Contains("oven")) stationTypes.Add("oven");
                else if (n.Contains("fryer")) stationTypes.Add("fryer");
                else if (n.Contains("sink")) stationTypes.Add("sink");
            }

            bool hasHatch = GameObject.Find("Kitchen/Stations/ServingHatch") != null ||
                            GameObject.FindObjectsOfType<GameObject>() is var all &&
                            System.Array.Exists(all, g => g.name.StartsWith("ServingHatch_"));

            var warnings = new JArray();
            if (!hasHatch) warnings.Add("No serving hatch placed — players cannot submit dishes");
            if (stationTypes.Count == 0) warnings.Add("No cooking stations placed");

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["completable"] = warnings.Count == 0,
                ["station_types_present"] = new JArray(new System.Collections.Generic.List<string>(stationTypes).ToArray()),
                ["has_serving_hatch"] = hasHatch,
                ["warnings"] = warnings
            });
        }

        public static UnityResponse EstimatePaths(UnityCommand cmd)
        {
            var from = ParseVec3(cmd.Payload["from"]);
            var toArray = cmd.Payload["to"] as JArray;

            var results = new JArray();
            if (toArray != null)
            {
                foreach (var target in toArray)
                {
                    var dest = ParseVec3(target);
                    float dist = Vector3.Distance(from, dest);
                    results.Add(new JObject
                    {
                        ["to"] = new JObject { ["x"] = dest.x, ["y"] = dest.y, ["z"] = dest.z },
                        ["manhattan_distance"] = Mathf.Abs(dest.x - from.x) + Mathf.Abs(dest.z - from.z),
                        ["euclidean_distance"] = dist,
                        ["note"] = "Exact pathfinding requires NavMesh; these are geometric estimates."
                    });
                }
            }

            return UnityResponse.Success(cmd.Id, new JObject { ["paths"] = results });
        }

        public static UnityResponse SimulatePlaythrough(UnityCommand cmd)
        {
            int playerCount = cmd.Payload["player_count"]?.Value<int>() ?? 2;
            float duration = cmd.Payload["duration_seconds"]?.Value<float>() ?? 300f;

            int stationCount = 0;
            var sp = GameObject.Find("Kitchen/Stations");
            if (sp != null) stationCount = sp.transform.childCount;

            int recipeCount = 1;
            try
            {
                var files = System.IO.Directory.GetFiles(
                    System.IO.Path.Combine(Application.dataPath, "Recipes"), "*.json");
                recipeCount = Mathf.Max(1, files.Length);
            }
            catch { }

            float recipesPerMinute = (playerCount * 0.5f) * (stationCount / 4f);
            float estimatedRecipes = recipesPerMinute * (duration / 60f);
            float avgPoints = 120f;

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["estimated_dishes"] = (int)estimatedRecipes,
                ["estimated_score_min"] = (int)(estimatedRecipes * avgPoints * 0.6f),
                ["estimated_score_max"] = (int)(estimatedRecipes * avgPoints * 1.4f),
                ["recipes_per_minute"] = recipesPerMinute,
                ["player_count"] = playerCount,
                ["duration_seconds"] = duration,
                ["note"] = "Rough estimate — actual performance depends on player skill and level layout."
            });
        }
    }
}
