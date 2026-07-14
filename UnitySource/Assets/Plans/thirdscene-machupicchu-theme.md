# Project Overview
- **Game Title:** Food To Go 3D (Overcooked-like cooking game, Peruvian theme)
- **High-Level Concept:** A 3D Overcooked-style cooking game where the player runs a Peruvian kitchen, grabbing ingredients from container counters, chopping/cooking them, plating dishes and delivering recipes against a timer for stars.
- **Players:** Single player
- **Inspiration / Reference Games:** Overcooked, Kitchen Chaos (Code Monkey)
- **Tone / Art Direction:** Stylized 3D, vibrant, realistic elements. The level represents the iconic **Machu Picchu terraces** (Andenes) — open-air, grassy, surrounded by historic Inca dry stone walls, with steep steps descending into the misty Andean valley, and the majestic peak of Huayna Picchu towering in the background.
- **Target Platform:** WebGL
- **Render Pipeline:** URP (URP_WebGl_asset)

# Game Mechanics
## Core Gameplay Loop
Pick up ingredients → process them (cut on `CuttingCounter`, cook on `StoveCounter`/`PotCounter`) → plate them on `PlatesCounter` → deliver at `DeliveryCounter` before the timer ends. This task updates the level theme and environment, keeping gameplay scripts unchanged.

## Controls and Input Methods
Standard keyboard/mouse/gamepad controls provided by the existing `GameInput` asset. No modifications needed.

# UI
Standard gameplay HUD (Timer, DeliveryManager UI, Game Start countdown, Game Over panel). No changes needed.

# Key Asset & Context
- **Active Scene:** `Assets/Scenes/ThirdScene.unity` — currently contains the playable base level inside a colonial room.
- **Floor GameObject:** Plane primitive at `(2.15, 0, 0)` with scale `(4, 4, 4)`, currently using `M_CobbleFloor` (cobblestone).
- **Inca Stone Material:** `Assets/_Assets/Generated/Cusco/M_IncaStone.mat` — pre-existing high-quality dry stone wall material using `BrickWall_Albedo`.
- **Skybox Material:** `Assets/_Assets/Generated/Cusco/M_Sky_Cusco.mat` — pre-existing misty mountain skybox.
- **Terrain Mesh:** `Assets/_Assets/Meshes/Environment/PeruTerrainMesh.asset` — pre-existing mountain terrain mesh we can use to represent the backdrop.
- **Boundary Walls:** `walls` GameObject contains colliders `wall_1`, `wall_1 (1)`, `wall_1 (2)`, and `wall_front` currently styled with a modern `wall-orange` material.

# Implementation Steps

### Step 1 — Deactivate Colonial Room indoor structures
- **Description:** Disable or delete the existing indoor room `Environment` GameObject (which contains child objects `Walls`, `RoofEave_North`, `Beams`, `Balcony`, `Decor`) to transition from an indoor room to an open-air mountain setting.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 2 — Create the Machu Picchu Grass Material
- **Description:** Create a new stylized green grass material `Assets/_Assets/Materials/M_MachuPicchuGrass.mat` using the `Universal Render Pipeline/Lit` shader. Set `BaseColor` to a rich, vibrant green (e.g. `#4E7A27` or `#598F2B`) and increase `Smoothness` to `0.1` and `Roughness` to `0.9` (via Metallic map/settings) to make it look like dry, lush mountain turf. Apply this material to the main `Floor` Plane.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 3 — Build the 3D Stepped Terraces (Andenes)
- **Description:** Construct 3D stepped terraces surrounding the playable kitchen to create the depth of a mountain slope. All objects will be grouped under a new empty parent `MachuPicchu_Environment` at root.
  - **Ascending Terraces (Background / North, `z > 8`):**
    - **Terrace 1:**
      - Retaining wall: 3D Cube named `Terrace1_Wall` at `(2.15, 0.75, 8.5)` with scale `(40, 1.5, 0.5)`. Material: `M_IncaStone`.
      - Terrace top: 3D Cube named `Terrace1_Grass` at `(2.15, 1.45, 11)` with scale `(40, 0.1, 5)`. Material: `M_MachuPicchuGrass`.
    - **Terrace 2:**
      - Retaining wall: 3D Cube named `Terrace2_Wall` at `(2.15, 2.25, 13.5)` with scale `(40, 1.5, 0.5)`. Material: `M_IncaStone`.
      - Terrace top: 3D Cube named `Terrace2_Grass` at `(2.15, 2.95, 16)` with scale `(40, 0.1, 5)`. Material: `M_MachuPicchuGrass`.
    - **Terrace 3:**
      - Retaining wall: 3D Cube named `Terrace3_Wall` at `(2.15, 3.75, 18.5)` with scale `(40, 1.5, 0.5)`. Material: `M_IncaStone`.
      - Terrace top: 3D Cube named `Terrace3_Grass` at `(2.15, 4.45, 21)` with scale `(40, 0.1, 5)`. Material: `M_MachuPicchuGrass`.
  - **Descending Terraces (Foreground / South, `z < -8`):**
    - **Terrace -1:**
      - Retaining wall: 3D Cube named `Terrace_Neg1_Wall` at `(2.15, -0.75, -8.5)` with scale `(40, 1.5, 0.5)`. Material: `M_IncaStone`.
      - Terrace top: 3D Cube named `Terrace_Neg1_Grass` at `(2.15, -1.45, -11)` with scale `(40, 0.1, 5)`. Material: `M_MachuPicchuGrass`.
    - **Terrace -2:**
      - Retaining wall: 3D Cube named `Terrace_Neg2_Wall` at `(2.15, -2.25, -13.5)` with scale `(40, 1.5, 0.5)`. Material: `M_IncaStone`.
      - Terrace top: 3D Cube named `Terrace_Neg2_Grass` at `(2.15, -2.95, -16)` with scale `(40, 0.1, 5)`. Material: `M_MachuPicchuGrass`.
- **Assigned role:** developer
- **Dependencies:** Step 2
- **Parallelizable:** No

### Step 4 — Build Roofless Inca Stone House Ruins
- **Description:** Construct two small, roofless stone huts on the background terraces (representing the archeological ruins of Machu Picchu) using cubes styled with `M_IncaStone`:
  - **Ruins House A (Left side of Terrace 2, around `x = -8, z = 16`):**
    - Left Wall: `(-9.5, 3.75, 16)`, scale `(0.5, 1.5, 3)`
    - Right Wall: `(-6.5, 3.75, 16)`, scale `(0.5, 1.5, 3)`
    - Back Wall: `(-8, 3.75, 17.5)`, scale `(3.5, 1.5, 0.5)`
    - Front Walls (leaving a center door gap): `(-9, 3.75, 14.5)`, scale `(1, 1.5, 0.5)` and `(-7, 3.75, 14.5)`, scale `(1, 1.5, 0.5)`
  - **Ruins House B (Right side of Terrace 3, around `x = 8, z = 21`):**
    - Left Wall: `(6.5, 5.25, 21)`, scale `(0.5, 1.5, 3)`
    - Right Wall: `(9.5, 5.25, 21)`, scale `(0.5, 1.5, 3)`
    - Back Wall: `(8, 5.25, 22.5)`, scale `(3.5, 1.5, 0.5)`
    - Front Walls: `(7, 5.25, 19.5)`, scale `(1, 1.5, 0.5)` and `(9, 5.25, 19.5)`, scale `(1, 1.5, 0.5)`
- **Assigned role:** developer
- **Dependencies:** Step 3
- **Parallelizable:** No

### Step 5 — Position the Huayna Picchu Mountain Backdrop
- **Description:** Create a large mountain peak in the far distance to resemble Huayna Picchu:
  - Create an empty GameObject named `HuaynaPicchu_Backdrop` at `(15, -5, 35)`.
  - Add a `MeshFilter` component and assign the existing `PeruTerrainMesh.asset` mesh (guid `745fb186c3a8dcf0689b2b3bffb5f7ef`).
  - Add a `MeshRenderer` component and assign `M_MachuPicchuGrass` as the material (or a blend with a rock material).
  - Set its scale to `(4, 15, 4)` and rotate it slightly (e.g. `(0, 45, 0)`) so it stands as a steep, lush, towering mountain peak visible in the background.
- **Assigned role:** developer
- **Dependencies:** Step 3
- **Parallelizable:** Yes

### Step 6 — Theme the Boundary Colliders as Stone Ruins
- **Description:** Apply the `M_IncaStone` material to the existing GameObjects under `walls` (`wall_1`, `wall_1 (1)`, `wall_1 (2)`, `wall_front`). This replaces the modern orange appearance with authentic, historic Inca dry-stone boundary walls, integrating gameplay collision boundaries seamlessly into the visual theme.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 7 — Configure Atmospheric Lighting, Fog, and Post-Processing
- **Description:** Create a realistic highland atmosphere:
  - **Directional Light:** Set color to a warm, gentle golden hour color (e.g. `#FFEAD4`), intensity to `1.2`, and Rotation to `(50, -35, 0)` so it casts long, dramatic shadows across the terraces.
  - **Fog (RenderSettings):** Verify fog is enabled. Set color to a soft, misty blue-grey (`#D3E2E6`) with ExponentialSquared mode and density `0.008`, creating the iconic "clouds brushing past the ruins" look.
  - **Post-Processing Volume (PostProcessing_Cusco):** Add or adjust:
    - **Color Adjustments:** Boost saturation (`+15`) and contrast (`+10`) to make the green terraces and stonework pop beautifully.
    - **Depth of Field:** Enable and set focus distance to `15` (the center of the playable area), which will blur the background terraces and Huayna Picchu slightly, giving a cinematic, photorealistic 3D look.
- **Assigned role:** developer
- **Dependencies:** Step 3
- **Parallelizable:** Yes

# Verification & Testing
- **Visual Integrity Check:** Play from `ThirdScene` inside the Unity Editor and verify the following:
  1. The indoor room is gone and the kitchen is set on a grassy green field.
  2. The backdrop displays multi-layered 3D stepped terraces rising up behind the level and descending down in front of it.
  3. Classic roofless Inca stone hut ruins are visible in the background.
  4. The boundary walls are stone and blend perfectly with the scenery.
  5. The towering peak of Huayna Picchu is visible in the distance.
  6. The lighting, misty fog, and cinematic Depth of Field create a highly realistic and beautiful 3D atmosphere.
- **Gameplay Verification:** Run the game and ensure that:
  1. The player can walk freely inside the playable boundary and cannot fall off.
  2. All kitchen counters are fully visible, reachable, and interactive.
  3. Recipes can be cooked and delivered successfully in the new open-air environment.
