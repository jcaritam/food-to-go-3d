# Project Overview
- **Game Title:** Food To Go 3D (Overcooked-like cooking game, Peruvian theme)
- **High-Level Concept:** A 3D Overcooked-style cooking game where the player runs a Peruvian kitchen, grabbing ingredients from container counters, chopping/cooking them, plating dishes and delivering recipes against a timer for stars.
- **Players:** Single player
- **Inspiration / Reference Games:** Overcooked, Kitchen Chaos (Code Monkey)
- **Tone / Art Direction:** Low-poly, stylized, Peruvian culinary theme (Lomo Saltado, Ají Amarillo, Camote, etc.)
- **Target Platform:** WebGL
- **Render Pipeline:** URP (URP_WebGl_asset)
- **This task scope:** **Build ONLY the functional base of `ThirdScene`** — a working, playable kitchen level with the standard managers, player, camera, UI and a counter layout, plus the minimal registration needed to launch it. **Peru-specific environment/skybox theming is explicitly OUT OF SCOPE for this task** and will be a follow-up.

# Game Mechanics
## Core Gameplay Loop
Pick up ingredients → process them (cut on `CuttingCounter`, cook on `StoveCounter`/`PotCounter`) → plate them on `PlatesCounter` → deliver at `DeliveryCounter` before the timer ends. Mistakes go to `TrashCounter`. Score determines stars (`LevelConfigSO`). This task only stands up the level; gameplay scripts already exist and are reused unchanged.

## Controls and Input Methods
Existing `GameInput` (Input System asset, guid `052faaac586de48259a63d0c4782560b`) and `Player` script — reused as-is. No input changes in this task.

# UI
Reuse the existing gameplay `Canvas` (DeliveryManagerUI, GameStartCountdownUI, GamePlayingClockUI, GameOverUI, GamePauseUI) exactly as in the other levels. The only UI-adjacent change is wiring the already-existing **"ThirdLevel"** button in `LevelSelectScene` so the new scene is reachable.

# Key Asset & Context

## Current state of ThirdScene (empty)
`Assets/Scenes/ThirdScene.unity` contains only a plain `Main Camera` (Camera + AudioListener, **no** `CameraFollow`, **no** `UniversalAdditionalCameraData` wiring) and a plain `Directional Light`. It must be populated to become a real level.

## Reference levels (existing, working)
- `Assets/Scenes/GameScene.unity` — levelId 1, `LevelConfig_1.asset`. **Cleanest structure**: counters grouped under a single `Counters` parent (35 children), a `walls` group (4), a single `EventSystem`. **Recommended source to copy.**
- `Assets/Scenes/HuariqueScene.unity` — levelId 2, `LevelConfig_2.asset`. Counters scattered at root, has a **duplicate EventSystem** (avoid copying its pitfalls).

## Required ingredients for a valid kitchen level (present in GameScene/Huarique)
`Main Camera` (Camera + AudioListener + `UniversalAdditionalCameraData` + `CameraFollow`→Player), `Directional Light`, `Floor`→`Plane`, `Player` (Player script + visuals + KitchenObjectHoldPoint), `GameInput`, `MusicManager` (AudioSource), `SoundManager`, `KitchenGameManager` (+ `LevelConfigSO`), `DeliveryManager` (+ `_RecipeListSO`), a `Canvas` with the 5 UI components, a single `EventSystem`, and counters.

## Counter prefabs (reusable) — `Assets/Prefacts/Counters/` (note folder spelling "Prefacts")
`_BaseCounter`, `ClearCounter`, `CuttingCounter`, `StoveCounter`, `PotCounter`, `PlatesCounter`, `DeliveryCounter`, `TrashCounter`, `ContainerCounter` (+ variants `_Rice`, `_Bread`, `_Lomo`, `_Tomato`, `_Meat`, `_Cabbage`, `_Cheese`).

## Scripts / configs touched
- `Assets/Scripts/Loader.cs` — `Scene` enum (currently: MenuScene, LevelSelectScene, GameScene, HuariqueScene, LoadingScene). **ThirdScene missing.**
- `ProjectSettings/EditorBuildSettings.asset` — registered scenes (ThirdScene missing).
- `Assets/Scripts/UI/GameOverUI.cs` — line 33 retry mapping `id == 2 ? HuariqueScene : GameScene` (no case for levelId 3).
- `Assets/ScriptableObjects/LevelConfig_2.asset` — template for a new `LevelConfig_3.asset` (LevelConfigSO, script guid `c15a57df441653be999068467827c342`).
- `LevelSelectScene` — `ThirdLevel` button under `Canvas/Container/ContainerGrid` exists but lacks `LevelButtonLoader` + onClick wiring. `LevelSelectUI.levelButtons[2]` already references it (levelIndex 3).

# Implementation Approach (recommended)
**Populate `ThirdScene` by copying the working contents of `GameScene`** (cleanest structure), then re-stamp it as level 3 and register it. This is the fastest, lowest-risk way to get a correct, playable base, because every required manager, the Player, the Camera-follow wiring, and the full Canvas UI come over intact and proven.

*Alternative considered:* building from scratch by instancing prefabs — rejected for the base task (tedious, error-prone wiring of Camera/Canvas/managers for no benefit while theming is out of scope).

# Implementation Steps

### Step 1 — Populate ThirdScene with the functional base
- **Description:** Copy the full working content of `GameScene` into `ThirdScene`: `Main Camera` (with `CameraFollow`→Player + `UniversalAdditionalCameraData`), `Directional Light`, `Floor`→`Plane`, `Player`, `GameInput`, `MusicManager`, `SoundManager`, `KitchenGameManager`, `DeliveryManager`, `Canvas` (5 UI components), a **single** `EventSystem`, and the `Counters` group (Delivery, Cutting, Stove, Pot, Plates, Trash, Clear, and Container variants) + `walls` group. Remove the pre-existing empty `Main Camera`/`Directional Light` so there are no duplicates. Verify exactly one `AudioListener` and one `EventSystem` remain.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** No

### Step 2 — Create LevelConfig_3 and stamp the scene as level 3
- **Description:** Create `Assets/ScriptableObjects/LevelConfig_3.asset` (LevelConfigSO) mirroring `LevelConfig_2.asset` with `levelId: 3` (tune `gamePlayingTimerMax`/star thresholds/`requiredStarsToUnlock` to match the level-3 difficulty intent; default to the same values as LevelConfig_2 for the base). On ThirdScene's `KitchenGameManager`, set `levelId = 3` and assign `levelConfig = LevelConfig_3.asset`. Confirm `DeliveryManager.recipeListSO` still points to `_RecipeListSO.asset`.
- **Assigned role:** developer
- **Dependencies:** Depends on Step 1
- **Parallelizable:** No

### Step 3 — Register ThirdScene in the loader enum
- **Description:** In `Assets/Scripts/Loader.cs`, append `ThirdScene` to the end of the `Scene` enum (after `LoadingScene`) to avoid shifting existing indices (GameScene=2, HuariqueScene=3 are referenced by serialized button values).
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (with Steps 1–2)

### Step 4 — Add ThirdScene to Build Settings
- **Description:** Add `Assets/Scenes/ThirdScene.unity` (guid `c9d8472f485594b089ace59cf5d04edc`, enabled) to `ProjectSettings/EditorBuildSettings.asset` so it can be loaded by name via `SceneManager.LoadScene`.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 5 — Wire the "ThirdLevel" button in LevelSelectScene
- **Description:** On the existing `ThirdLevel` button (under `Canvas/Container/ContainerGrid`), add a `LevelButtonLoader` component, set its `targetScene` to the new `ThirdScene` enum value, and wire `Button.onClick` → `LevelButtonLoader.LoadLevel` (mirroring `FirstLevelBtn`/`SecondLevelBtn`). Confirm `LevelSelectUI.levelButtons[2]` (levelIndex 3) still references this button.
- **Assigned role:** developer
- **Dependencies:** Depends on Step 3 (enum value must exist)
- **Parallelizable:** No

### Step 6 — Fix GameOverUI retry mapping for level 3
- **Description:** In `Assets/Scripts/UI/GameOverUI.cs` (line ~33), extend the retry scene mapping so `levelId == 3` reloads `Loader.Scene.ThirdScene` (currently only handles 2→Huarique, else GameScene). E.g. a small switch on `id`.
- **Assigned role:** developer
- **Dependencies:** Depends on Step 3
- **Parallelizable:** No

# Verification & Testing
- **Compile check:** No console errors after editing `Loader.cs` and `GameOverUI.cs` (the new enum value resolves; `ThirdScene` name matches the scene file).
- **Scene integrity (ThirdScene):** Exactly one `Main Camera` (with `CameraFollow` target = Player and `UniversalAdditionalCameraData`), one `AudioListener`, one `EventSystem`, one `Directional Light`, a `Floor`, a `Player`, `GameInput`, `KitchenGameManager` (levelId=3, levelConfig=LevelConfig_3), `DeliveryManager` (recipeListSO set), `SoundManager`, `MusicManager`, full `Canvas` UI, and the counter set. No missing-script ("None") references on any object.
- **Build settings:** ThirdScene appears enabled in Build Settings; `Loader.Scene.ThirdScene` exists and other indices unchanged.
- **Play test (Editor):**
  1. Enter Play from `MenuScene` → LevelSelect → click **ThirdLevel** → LoadingScene → ThirdScene loads and is playable (countdown starts, timer runs, counters interactable, delivery works).
  2. Pick up/cut/cook/plate/deliver one recipe to confirm managers function.
  3. Let the timer end → GameOver shows; press **Retry** → it reloads **ThirdScene** (not GameScene), confirming Step 6.
- **Regression:** Launch GameScene (level 1) and HuariqueScene (level 2) from LevelSelect to confirm their buttons/retry still work (enum indices unchanged).
