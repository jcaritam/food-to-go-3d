using System;
using McpBridge;
using McpBridge.Handlers;

namespace McpBridge
{
    public static class McpCommandDispatcher
    {
        public static UnityResponse Dispatch(UnityCommand cmd)
        {
            try
            {
                return cmd.Type switch
                {
                    "generate_layout"         => LayoutHandler.GenerateLayout(cmd),
                    "set_tile"                => LayoutHandler.SetTile(cmd),
                    "split_kitchen"           => LayoutHandler.SplitKitchen(cmd),
                    "add_obstacle"            => LayoutHandler.AddObstacle(cmd),
                    "set_theme"               => LayoutHandler.SetTheme(cmd),
                    "get_layout"              => LayoutHandler.GetLayout(cmd),
                    "place_station"           => StationHandler.PlaceStation(cmd),
                    "place_counter"           => StationHandler.PlaceCounter(cmd),
                    "place_serving_hatch"     => StationHandler.PlaceServingHatch(cmd),
                    "place_ingredient_crate"  => StationHandler.PlaceIngredientCrate(cmd),
                    "place_trash"             => StationHandler.PlaceTrash(cmd),
                    "place_dish_return"       => StationHandler.PlaceDishReturn(cmd),
                    "configure_station"       => StationHandler.ConfigureStation(cmd),
                    "create_recipe"           => RecipeHandler.CreateRecipe(cmd),
                    "create_ingredient"       => RecipeHandler.CreateIngredient(cmd),
                    "configure_order_queue"   => RecipeHandler.ConfigureOrderQueue(cmd),
                    "set_scoring"             => RecipeHandler.SetScoring(cmd),
                    "set_level_timer"         => RecipeHandler.SetLevelTimer(cmd),
                    "add_moving_platform"     => DisruptionHandler.AddMovingPlatform(cmd),
                    "add_conveyor_belt"       => DisruptionHandler.AddConveyorBelt(cmd),
                    "add_hazard"              => DisruptionHandler.AddHazard(cmd),
                    "add_timed_event"         => DisruptionHandler.AddTimedEvent(cmd),
                    "add_portal"              => DisruptionHandler.AddPortal(cmd),
                    "configure_schedule"      => DisruptionHandler.ConfigureSchedule(cmd),
                    "set_player_count"        => PlayerHandler.SetPlayerCount(cmd),
                    "place_spawn_point"       => PlayerHandler.PlaceSpawnPoint(cmd),
                    "configure_controls"      => PlayerHandler.ConfigureControls(cmd),
                    "set_player_abilities"    => PlayerHandler.SetPlayerAbilities(cmd),
                    "validate_reachability"   => ValidationHandler.ValidateReachability(cmd),
                    "analyze_difficulty"      => ValidationHandler.AnalyzeDifficulty(cmd),
                    "check_completability"    => ValidationHandler.CheckCompletability(cmd),
                    "estimate_paths"          => ValidationHandler.EstimatePaths(cmd),
                    "simulate_playthrough"    => ValidationHandler.SimulatePlaythrough(cmd),
                    "generate_game_manager"   => CodegenHandler.GenerateGameManager(cmd),
                    "generate_station_scripts"=> CodegenHandler.GenerateStationScripts(cmd),
                    "generate_scriptables"    => CodegenHandler.GenerateScriptables(cmd),
                    "generate_order_system"   => CodegenHandler.GenerateOrderSystem(cmd),
                    "generate_player_controller" => CodegenHandler.GeneratePlayerController(cmd),
                    "write_file"              => CodegenHandler.WriteFile(cmd),
                    "asset_refresh"           => CodegenHandler.AssetRefresh(cmd),
                    _ => UnityResponse.Fail(cmd.Id, $"Unknown command type: {cmd.Type}")
                };
            }
            catch (Exception ex)
            {
                return UnityResponse.Fail(cmd.Id, $"Unhandled exception in {cmd.Type}: {ex.Message}");
            }
        }
    }
}
