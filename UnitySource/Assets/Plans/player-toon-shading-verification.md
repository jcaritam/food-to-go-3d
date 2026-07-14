# Project Overview
- Game Title: Food To Go 3D
- High-Level Concept: 3D casual chef delivery/cooking game.
- Players: Single player.
- Inspiration / Reference Games: Overcooked, Diner Dash.
- Tone / Art Direction: Toon / Cel-shaded stylized 3D.
- Target Platform: WebGL.
- Screen Orientation / Resolution: Landscape.
- Render Pipeline: URP (Universal Render Pipeline).

# Game Mechanics
## Core Gameplay Loop
Players control a chef character to pick up ingredients, cook/prepare food, and deliver it within time limits.
## Controls and Input Methods
Movement is controlled via keyboard (WASD/Arrow keys) or touch/gamepad. The project is configured to support both the New Input System and Legacy Input Manager.

# UI
Game scene hud showing order timer, active orders, and score. Main menu for starting the game.

# Key Asset & Context
- **Shader**: `Assets/Shaders/PlayerToon.shader` (Toon shader with outline & light ramp).
- **Materials**:
  - 6 Toon materials under `Assets/_Assets/Materials/` (PlayerBody_Toon, PlayerHead_Toon, PlayerHat_Toon, PlayerNose_Toon, PlayerEye_Toon, PlayerButton_Toon).
  - 1 New Toon material for eye reflections: `Assets/_Assets/Materials/PlayerEyeGlint_Toon.mat` (Pure white base, no outline).
- **Prefab**: `Assets/_Assets/PrefabsVisuals/PlayerVisual.prefab` (Re-mapped mesh renderers pointing to the toon materials, updated hierarchy for improved visuals).
- **Main Prefab**: `Assets/_Assets/PrefabsVisuals/Player/Player.prefab` (Loads `PlayerVisual` as child).

# Implementation Steps
## Step 1: Verify Shader compilation and material links (Completed)
- **Description**: Inspect console logs and the prefabs in the database. Ensure the `Custom/PlayerToon` shader compiles successfully and is assigned to all mesh renderers of `PlayerVisual.prefab`.
- **Assigned role**: explorer
- **Dependencies**: None
- **Parallelizable**: No

## Step 2: Adjust chef_hat Local Position in PlayerVisual Prefab (Completed)
- **Description**: Lower `chef_hat` local Y-position from `0.79` to `0.74` in the `PlayerVisual.prefab` to fix the "floating" hat appearance. This will be done programmatically using `PrefabUtility` in an Editor Script to ensure asset database serialization is perfect.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Validate Active Scene Instances (Completed)
- **Description**: Verify that the Player object in `GameScene` updates its chef hat visual offset correctly.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## Step 4: Create Eye Glint Material
- **Description**: Create `PlayerEyeGlint_Toon.mat` using the `Custom/PlayerToon` shader. Set `_BaseColor` and `_ShadowTint` to pure white, and `_OutlineWidth` to `0` to create a bright, unshaded reflection dot.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## Step 5: Add Eye Glints and Puffy Hat to PlayerVisual Prefab
- **Description**: 
  - Add two tiny sphere child GameObjects under `Eye_R` and `Eye_L` named `Glint_R` and `Glint_L`. Set their localPosition to `(0.25, 0.25, 0.40)` and localScale to `(0.25, 0.12, 0.25)`. Assign the `PlayerEyeGlint_Toon` material to them.
  - Add a puffy sphere child GameObject under `chef_hat` named `chef_hat_puff`. Set its localPosition to `(0.00, 0.80, 0.00)` and localScale to `(1.40, 1.80, 1.40)`. Assign the `PlayerHat_Toon` material to it.
  - These additions will be done programmatically to ensure perfect prefab serialization.
- **Assigned role**: developer
- **Dependencies**: Step 4
- **Parallelizable**: No

## Step 6: Validate Final Character Visuals in Scene
- **Description**: Confirm that the new eye glints and puffy hat render beautifully in `GameScene` on the player character instance.
- **Assigned role**: developer
- **Dependencies**: Step 5
- **Parallelizable**: No

# Verification & Testing
- Run an automated check script using `IReadonlyRunCommand` to verify `Glint_R`, `Glint_L`, and `chef_hat_puff` exist, have correct positions/scales/materials, and that no pink shaders or errors exist.
- Verify visually in the editor that the character looks highly polished, cute, and professional.
