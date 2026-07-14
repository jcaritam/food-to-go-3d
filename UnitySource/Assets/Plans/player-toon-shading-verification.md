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
  - 1 New Toon material for cute blush cheeks: `Assets/_Assets/Materials/PlayerBlush_Toon.mat` (Soft coral/pink color).
  - 1 New Toon material for the mouth: `Assets/_Assets/Materials/PlayerMouth_Toon.mat` (Dark reddish-brown/black color).
  - 1 New Toon material for the chef apron: `Assets/_Assets/Materials/PlayerApron_Toon.mat` (Crisp white base, solid toon outline).
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

## Step 4: Create Eye Glint Material (Completed)
- **Description**: Create `PlayerEyeGlint_Toon.mat` using the `Custom/PlayerToon` shader. Set `_BaseColor` and `_ShadowTint` to pure white, and `_OutlineWidth` to `0` to create a bright, unshaded reflection dot.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## Step 5: Add Eye Glints and Puffy Hat to PlayerVisual Prefab (Completed)
- **Description**: 
  - Add two tiny sphere child GameObjects under `Eye_R` and `Eye_L` named `Glint_R` and `Glint_L`. Set their localPosition to `(0.25, 0.25, 0.40)` and localScale to `(0.25, 0.12, 0.25)`. Assign the `PlayerEyeGlint_Toon` material to them.
  - Add a puffy sphere child GameObject under `chef_hat` named `chef_hat_puff`. Set its localPosition to `(0.00, 0.80, 0.00)` and localScale to `(1.40, 1.80, 1.40)`. Assign the `PlayerHat_Toon` material to it.
  - These additions will be done programmatically to ensure perfect prefab serialization.
- **Assigned role**: developer
- **Dependencies**: Step 4
- **Parallelizable**: No

## Step 6: Create Blush and Mouth Materials (Completed)
- **Description**: Create `PlayerBlush_Toon.mat` (soft warm pink) and `PlayerMouth_Toon.mat` (dark warm reddish-black) to style the face enhancements.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## Step 7: Enhance Nose, Add Blush Cheeks, and Cute Mouth (Completed)
- **Description**:
  - Replace the capsule mesh of the `noise` (nose) GameObject with a cute round Sphere mesh for a soft button-nose appearance.
  - Add two tiny rosy cheek spheres `Blush_L` and `Blush_R` under the `Head` object. Place them flanking the nose: `(0.26, -0.02, 0.40)` and `(-0.26, -0.02, 0.40)` with scale `(0.12, 0.06, 0.12)`.
  - Add a cute open mouth `Mouth` under the `Head` object just below the nose: `(0.00, -0.08, 0.44)` with scale `(0.08, 0.04, 0.04)`.
- **Assigned role**: developer
- **Dependencies**: Step 6
- **Parallelizable**: No

## Step 8: Create Apron Material
- **Description**: Create `PlayerApron_Toon.mat` using the `Custom/PlayerToon` shader. Set `_BaseColor` to clean white `#FAFAFA` and `_ShadowTint` to `#E0E0E0`, with a solid toon outline width of `0.02`.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## Step 9: Add Chef Apron and Double-Breasted Jacket Buttons
- **Description**:
  - Add a squashed Cube child of `Body` named `Apron_Base` to serve as the chef's apron front bib. Pos: `(0.00, 0.08, 0.34)`, Scale: `(0.65, 0.85, 0.28)`. Assign `PlayerApron_Toon` material.
  - Add shoulder straps: `Strap_L` and `Strap_R` (Cubes, using `PlayerButton_Toon` material) crossing down over the shoulders. Pos: `(-0.25, 0.40, 0.15)` and `(0.25, 0.40, 0.15)`, Scale: `(0.06, 0.65, 0.06)`, Rot: `(35, 0, 0)`.
  - Reorganize the chef buttons on the front into a stylish double-breasted 2x3 button configuration (6 buttons total) sitting perfectly on top of the apron cloth.
- **Assigned role**: developer
- **Dependencies**: Step 8
- **Parallelizable**: No

## Step 10: Validate Final Character Visuals in Scene
- **Description**: Confirm that all new facial elements, puffy hat, chef's apron, and buttons render beautifully in `GameScene` on the player character instance.
- **Assigned role**: developer
- **Dependencies**: Step 9
- **Parallelizable**: No

## Step 11: Redesign Apron — Rounded, Shorter, Half-Body (Refinement)
- **Problem**: The current `Apron_Base` is a large flat Cube that pokes through the round body and juts out to the side, looking bad (see reference image).
- **Description**:
  - Change `Apron_Base` mesh from `Cube` to `Sphere` so its edges are rounded and hug the spherical body naturally.
  - Reshape and reposition it to bulge out ONLY at the front-lower half of the body: localPosition `(0.00, -0.06, 0.32)`, localScale `(0.58, 0.52, 0.55)`. The back of the sphere stays hidden inside the body; only a clean rounded front panel shows, covering roughly the lower/front half.
  - Reposition the 6 double-breasted buttons onto the visible apron front surface (z ~0.54–0.56) in a tidy 2x3 layout.
  - Thin out the shoulder straps (`Strap_L`, `Strap_R`) and re-angle them to neatly connect the bib to the shoulders: scale `(0.045, 0.42, 0.045)`, pos `(±0.15, 0.33, 0.28)`, rot `(28, 0, 0)`.
- **Assigned role**: developer
- **Dependencies**: Step 9
- **Parallelizable**: No

## Step 12: Validate Redesigned Apron in Scene
- **Description**: Revert the `PlayerVisual` instance overrides in `GameScene` and confirm the rounded apron, buttons, and straps render cleanly with no pink shaders or errors.
- **Assigned role**: developer
- **Dependencies**: Step 11
- **Parallelizable**: No

## Step 13: Add Little Arms (Shoulder Pivots + Capsule Meshes)
- **Approach**: Procedural swing (chosen over Animator clip editing because the player's `IsWalking` is not wired and the body is procedurally built from primitives).
- **Description**:
  - Under `Body`, create two empty pivot GameObjects `Arm_L` and `Arm_R` positioned at the shoulders (approx body-local `(±0.42, 0.08, 0.05)`). Rotating these pivots around X swings the arms.
  - Under each pivot, add a capsule mesh child (`Arm_L_Mesh`, `Arm_R_Mesh`) offset downward so it hangs from the shoulder (approx local pos `(0, -0.18, 0)`, scale `(0.14, 0.16, 0.14)`). Assign `PlayerBody_Toon` so the arms match the body color and toon outline.
  - Remove any auto-added Colliders on the new arm objects.
  - Exact positions/scales tuned via Scene View capture of a temporary isolated preview instance.
- **Assigned role**: developer
- **Dependencies**: Step 12
- **Parallelizable**: No

## Step 14: Create PlayerArmSwing Procedural Animation Script
- **Description**: Create `Assets/Scripts/PlayerArmSwing.cs` — a `MonoBehaviour` that:
  - Serializes references to `armLeft` and `armRight` pivot transforms plus tunables (`swingSpeed`, `swingAmplitude`, `restLerpSpeed`, `moveThreshold`, `idleSwayAmplitude`).
  - In `Update`, measures planar world-position delta of its own transform to determine current speed → a `walkWeight` (0..1) eased over time.
  - Advances a phase accumulator and sets each arm's `localRotation` to swing forward/back around X in opposite phase, scaled by `walkWeight`; adds a subtle idle sway when not walking.
  - Caches base rotations on `Awake` so swings are relative to the authored rest pose.
- **Assigned role**: developer
- **Dependencies**: Step 13
- **Parallelizable**: No

## Step 15: Attach & Wire PlayerArmSwing on PlayerVisual, Validate
- **Description**: Add the `PlayerArmSwing` component to the `PlayerVisual` prefab root, assign the `Arm_L`/`Arm_R` pivots, save the prefab, sync the scene instance, and verify in Play mode (or via Scene capture) that arms swing while moving and settle when idle. Confirm no console errors and no pink shaders.
- **Assigned role**: developer
- **Dependencies**: Step 14
- **Parallelizable**: No

# Verification & Testing
- Run an automated check script using `IReadonlyRunCommand` to verify `Glint_R`, `Glint_L`, `chef_hat_puff`, `Blush_L`, `Blush_R`, `Mouth`, `Apron_Base`, and the updated 6 buttons exist, have correct positions/scales/materials, and that no pink shaders or errors exist.
- Verify visually in the editor that the character looks highly polished, cute, and professional.
