using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace McpBridge.Handlers
{
    public static class RecipeHandler
    {
        private static string RecipesPath => Path.Combine(Application.dataPath, "Recipes");
        private static string IngredientsPath => Path.Combine(Application.dataPath, "Ingredients");

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public static UnityResponse CreateRecipe(UnityCommand cmd)
        {
            string name = cmd.Payload["name"]?.Value<string>() ?? "UnnamedRecipe";
            EnsureDir(RecipesPath);

            var data = new JObject
            {
                ["recipeName"] = name,
                ["basePoints"] = cmd.Payload["base_points"]?.Value<int>() ?? 100,
                ["preparationTime"] = cmd.Payload["preparation_time"]?.Value<float>() ?? 30f,
                ["ingredients"] = cmd.Payload["ingredients"] ?? new JArray(),
                ["steps"] = cmd.Payload["steps"] ?? new JArray()
            };

            string filePath = Path.Combine(RecipesPath, $"{name}.json");
            File.WriteAllText(filePath, data.ToString());

            AssetDatabase.Refresh();

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["recipe_name"] = name,
                ["file_path"] = filePath,
                ["note"] = "JSON config written. Use generate_scriptables to create RecipeSO.cs, then create .asset files via Unity menu."
            });
        }

        public static UnityResponse CreateIngredient(UnityCommand cmd)
        {
            string name = cmd.Payload["name"]?.Value<string>() ?? "UnnamedIngredient";
            EnsureDir(IngredientsPath);

            var data = new JObject
            {
                ["ingredientName"] = name,
                ["states"] = cmd.Payload["states"] ?? new JArray()
            };

            string filePath = Path.Combine(IngredientsPath, $"{name}.json");
            File.WriteAllText(filePath, data.ToString());

            AssetDatabase.Refresh();

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["ingredient_name"] = name,
                ["file_path"] = filePath
            });
        }

        public static UnityResponse ConfigureOrderQueue(UnityCommand cmd)
        {
            var config = new JObject
            {
                ["min_time"] = cmd.Payload["min_time"]?.Value<float>() ?? 20f,
                ["max_time"] = cmd.Payload["max_time"]?.Value<float>() ?? 60f,
                ["max_concurrent"] = cmd.Payload["max_concurrent"]?.Value<int>() ?? 3,
                ["tip_curve"] = cmd.Payload["tip_curve"] ?? new JObject()
            };

            string configPath = Path.Combine(Application.dataPath, "order_queue_config.json");
            File.WriteAllText(configPath, config.ToString());
            AssetDatabase.Refresh();

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["config_path"] = configPath,
                ["note"] = "Apply these values to your OrderManager component in the scene."
            });
        }

        public static UnityResponse SetScoring(UnityCommand cmd)
        {
            var config = new JObject
            {
                ["points_per_dish"] = cmd.Payload["points_per_dish"] ?? new JArray(),
                ["star_thresholds"] = cmd.Payload["star_thresholds"] ?? new JArray(new[] { 300, 600, 900 }),
                ["multipliers"] = cmd.Payload["multipliers"] ?? new JArray()
            };

            string configPath = Path.Combine(Application.dataPath, "scoring_config.json");
            File.WriteAllText(configPath, config.ToString());
            AssetDatabase.Refresh();

            return UnityResponse.Success(cmd.Id, new JObject { ["config_path"] = configPath });
        }

        public static UnityResponse SetLevelTimer(UnityCommand cmd)
        {
            float duration = cmd.Payload["duration_seconds"]?.Value<float>() ?? 300f;

            var gm = GameObject.Find("GameManager");
            if (gm != null)
            {
                var gmComp = gm.GetComponent<MonoBehaviour>();
                if (gmComp != null)
                {
                    var field = gmComp.GetType().GetField("levelDuration");
                    if (field != null)
                    {
                        Undo.RecordObject(gmComp, "MCP Set Level Timer");
                        field.SetValue(gmComp, duration);
                    }
                }
            }

            return UnityResponse.Success(cmd.Id, new JObject
            {
                ["duration_seconds"] = duration,
                ["game_manager_found"] = gm != null
            });
        }
    }
}
