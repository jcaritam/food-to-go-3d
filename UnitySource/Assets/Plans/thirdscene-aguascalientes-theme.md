# ThirdScene — Ambiente "Aguas Calientes" (pueblo de valle junto al río)

## Contexto
Reemplaza por completo el ambiente actual "Cusco Colonial" (sala cerrada de adobe,
balaustres, techo — GameObject raíz `Environment`) por un ambiente **exterior** de pueblo
andino estilo Aguas Calientes / Machu Picchu Pueblo: plaza empedrada rodeada de fachadas de
casas de colores vivos, con un valle de montañas verdes cerrando el horizonte y un río
visible de fondo. No se toca ningún script de gameplay ni el layout de counters.

Geometría existente reutilizada como referencia de escala:
- `Floor`: Plane en `(2.15, 0, 0)`, escala `(4, 4, 4)` → cubre aprox. `x[-13.5..17.9], z[-17.8..17.8]`.
- Counters ocupan `x[-12..11], z[-7..7]` (documentado en plan Cusco).
- `walls`/`wall_1`/`wall_1 (1)`/`wall_1 (2)`/`wall_front`: colliders límite del área jugable,
  ya usan `M_IncaStone` en el plan Machu Picchu (no ejecutado) — aquí se re-tematizan igual.

## Assets a reutilizar (ya existen en el proyecto)
- `Assets/_Assets/Meshes/Environment/PeruTerrainMesh.asset` (guid `745fb186c3a8dcf0689b2b3bffb5f7ef`) — montañas de fondo.
- `Assets/_Assets/Generated/Cusco/M_CobbleFloor.mat` — piso de plaza empedrada.
- `Assets/_Assets/Generated/Cusco/M_IncaStone.mat` — bordes/baranda de piedra.
- `PostProcessing_Cusco` (Volume existente en la escena) — se reajusta, no se recrea.

## Assets nuevos a crear
- `Assets/_Assets/Materials/M_Casa_Naranja.mat`, `M_Casa_Rojo.mat`, `M_Casa_Azul.mat`,
  `M_Casa_Blanco.mat` (URP/Lit, colores planos vibrantes tipo fachada andina).
  Techos: `M_Techo_Teja.mat` (naranja-terracota, `Smoothness` bajo).
- `Assets/_Assets/Materials/M_Rio_AguasCalientes.mat` (URP/Lit, azul-blanco translúcido o
  con `Surface Type: Transparent`, `Smoothness` alto para simular agua rápida con espuma).

## Implementación

### Paso 1 — Desmontar el ambiente Cusco Colonial indoor
Desactivar (`SetActive(false)`, no eliminar, para poder revertir) el GameObject raíz
`Environment` (contiene `Walls`, `RoofEave_North`, `Beams`, `Balcony` y sus hijos:
`Wall_North/South/East/West`, `Baluster` x24, `Beam_X/Z`, etc.).

### Paso 2 — Piso de plaza empedrada
Reutilizar el `Floor` existente en `(2.15, 0, 0)` escala `(4,4,4)`, aplicar
`M_CobbleFloor.mat` (ya existe, solo reasignar si no está puesto).

### Paso 3 — Fachadas del pueblo (agrupadas bajo `Pueblo_Environment`)
Dos hileras de casas siguiendo el patrón de cubos usado en Machu Picchu (pared = Cube
escalado, techo = Cube rotado 45° en X o Cube aplanado a dos aguas), variando colores
(naranja, rojo, azul, blanco) y alturas (2-3 pisos) para dar variedad tipo la foto:

- **Hilera Norte** (detrás de los counters norte, `z ≈ 14–16`): 5-6 casas repartidas en
  `x` desde `-14` hasta `16`, ancho variable `2.5–4`, alto `3–5`, profundidad `3`.
- **Hilera Sur** (detrás de los counters sur, `z ≈ -14 a -16`): 4-5 casas similares,
  `x` desde `-10` hasta `12`.

Cada casa: `Casa_N_Pared` (Cube, material de color aleatorio entre los 4) +
`Casa_N_Techo` (Cube aplanado y rotado, `M_Techo_Teja`) + opcionalmente
`Casa_N_Ventana` (Cube pequeño oscuro insertado en la pared, sin collider).
Todas con `BoxCollider` activo (son parte del boundary visual/físico del pueblo).

### Paso 4 — Valle de montañas de fondo
4 instancias de `PeruTerrainMesh.asset` bajo `Valle_Montañas`, rodeando el perímetro más
allá de las casas (a partir de `|x| > 20` o `|z| > 20`), escaladas `(4, 10-15, 4)` y
rotadas en Y (`0°, 90°, 180°, 270°` aprox, con variación) para cerrar el horizonte en las
4 direcciones — mismo enfoque que el paso 5 del plan Machu Picchu pero repetido en anillo
en vez de un solo pico.

### Paso 5 — Río de fondo (decorativo, sin colisión de gameplay)
Un `Plane` llamado `Rio_Fondo` posicionado más allá de la hilera Sur (o la que se decida
como "lado del río"), p. ej. `(2.15, -0.3, -24)`, escala `(6, 1, 2)`, material
`M_Rio_AguasCalientes`. Sin `BoxCollider` de gameplay (o con uno solo para bloquear visión/
movimiento si el jugador pudiera llegar tan lejos — se decide en implementación según si
el boundary existente ya lo cubre).

### Paso 6 — Re-tematizar boundary walls
Aplicar `M_IncaStone.mat` a `wall_1`, `wall_1 (1)`, `wall_1 (2)`, `wall_front` (reemplaza
el material `wall-orange` moderno actual), como baranda/borde de piedra de la plaza.

### Paso 7 — Cielo, luz y niebla
- Skybox: **None** (ya se dejó así en el cambio anterior) o Skybox procedural celeste
  simple de Unity — decidir en implementación, sin volver a usar `Sky_Cusco.png` como
  panorama.
- Directional Light: cálida, tipo tarde andina (`#FFEAD4`~, intensidad `1.1-1.2`,
  rotación con ángulo bajo para sombras largas).
- Fog: verde-azulada suave, `ExponentialSquared`, densidad baja (`0.006-0.01`) para dar
  profundidad al valle sin ocultar el pueblo.

### Paso 8 — Post-processing
Reajustar `PostProcessing_Cusco` (Color Adjustments: saturación/contraste ligero) para
que resalten los colores de las fachadas y el verde de las montañas.

## Verificación
1. Play desde `ThirdScene`: confirmar que el ambiente indoor ya no es visible y el
   jugador está en una plaza empedrada abierta.
2. Confirmar visualmente (captura de cámara vía MCP) que se ven fachadas de colores
   alrededor, montañas verdes cerrando el horizonte, y el río visible de fondo en al
   menos un lado.
3. Confirmar que todos los counters siguen siendo alcanzables y el jugador no puede
   salir del área jugable (colliders de `walls` siguen funcionando).
4. Repetir el flujo de entrega de una receta completa para confirmar que el gameplay no
   se rompió.
5. `Unity_ReadConsole` sin errores/warnings nuevos.
