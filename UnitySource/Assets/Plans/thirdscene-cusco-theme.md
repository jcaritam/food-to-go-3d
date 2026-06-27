# ThirdScene — Cusco Colonial Ambience Plan

Adds complete Cusco colonial environment to the already-functional ThirdScene (level 3).
Gameplay base (Player, managers, Canvas, 53 counters, camera) is done — this only adds
environment, decoration, lighting, sky, post-processing. Counters occupy ~x[-12..11], z[-7..7].

## Generated AI assets (Assets/_Assets/Generated/Cusco/)
- Sky_Cusco.png (cubemap) — guid c106b2bd170c9444aa47933e494e7408
- M_IncaStone.mat — guid d44a5258262404e789f973c57bb2f723
- M_AdobeWall.mat — guid 104b23577fff34d9f8d9127fa0167400
- M_CobbleFloor.mat — guid d629d52d2ba564b64b641c9c85142684

## Reuse
- WallPicture_Peru.fbx, WallPicture_Virgin.fbx, WallLight.fbx (Assets/_Assets/PrefabsVisuals/)
- Mesa_Comensal.fbx (Assets/_Assets/Meshes/Ingredients/)
- FoodTruck.prefab (Assets/_Assets/Meshes/Environment/)

## Steps
1. Generate AI assets — DONE
2. Verify assets + build themed materials + skybox material
3. Build enclosed Cusco room geometry (floor, walls stone base + adobe, arches, beams, balcony)
4. Place decoration props
5. Lighting & sky (skybox, warm afternoon light, ambient, fog)
6. URP post-processing volume + enable on camera
7. Visual validation + Play Mode smoke test
