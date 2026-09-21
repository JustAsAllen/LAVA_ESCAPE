<p align="center">
  <img src="Report/shots_small/01_hero_wide.jpg" width="100%" alt="LAVA ESCAPE - the course from above">
</p>

<h1 align="center">LAVA ESCAPE</h1>

<p align="center">
  <b>First-Person Lava Parkour Speedrunner</b><br>
  <i>Extended Realities &middot; ADS351 &middot; B.Tech Artificial Intelligence &amp; Machine Learning &middot; CHRIST (Deemed to be University), Bengaluru</i>
</p>

<p align="center">
  <code>Unity 6000.5.10f1</code> <code>Universal Render Pipeline</code> <code>C# / .NET</code> <code>Unity Input System</code> <code>ShaderLab / HLSL</code> <code>Windows Standalone</code>
</p>

---

## Abstract

**LAVA ESCAPE** is an immersive first-person parkour speedrunning experience built in the Unity Engine using the Universal Render Pipeline (URP). The player flows through a procedurally constructed multi-stage obstacle course — precision platforming, curved suspension-bridge beams, and ascending jumps — while chasing checkpoints, collecting **13 diamonds**, and beating a live split timer. A custom ShaderLab **Voronoi–fBM lava shader** floods the canyon floor beneath the entire course, turning every mistimed jump into a fiery respawn back to the last checkpoint. The project pairs a polished game-feel layer (dash slow-motion, coyote time, jump buffering, slide & sprint) with a fully aligned HUD and an ESC pause menu.

### Quick Facts

| | |
|---|---|
| **11** | Checkpoints |
| **13** | Diamonds |
| **6** | Course Stages |
| **0.2×** | Dash Slow-Mo |
| **1920×1080** | Reference UI |

---

## Contributors

This project report is a component of the course work for **Extended Realities (ADS351)**.

| # | Name | Register No. |
|---|------|--------------|
| 1 | Allen Saji | 2562305 |
| 2 | Joy Samson A | 2562325 |
| 3 | Amogh Kadekar | 2562306 |
| 4 | Christo Ajith | 2562311 |

<sub>School of Engineering & Technology · CHRIST (Deemed to be University), Kumbalagodu, Bengaluru-560 074</sub>

---

## How the Game Was Made in Unity

LAVA ESCAPE was built end-to-end in Unity 6000.5.10f1. Rather than hand-placing objects in the editor, the **entire game is authored in C#** — the level, materials, lighting, UI and even the build are generated and verified by code. Here is the process at a glance:

1. **Project setup** — Created with the **Universal Render Pipeline (URP)** template and the new **Input System**. The source scene is blank; nothing is placed by hand.
2. **Procedural level generation** — An **Editor tool (`CourseBuilder`)** builds the world from code: a linear cursor advances along +Z and lays out decks, micro-platforms, a curved beam bridge, gates, 11 checkpoints, 13 diamonds and hazards — then adds lighting, the skybox and every material in the same pass.
3. **Shading & the lava** — Materials are created in code, but the hero look is a **custom ShaderLab/HLSL shader** (`MC_Lava`) that combines Voronoi cell cracking with 5-octave fractional-Brownian-motion noise for flow and glow, lit by URP forward rendering with bloom.
4. **Gameplay systems** — **`PlayerController`** adds coyote time (0.18 s), jump buffering (0.14 s), sprint, slide, and a slow-motion dash (`Time.timeScale` → 0.2×). **`GameManager`** owns the run timer, best time, deaths, checkpoint respawns, gem collection and all game events.
5. **UI, pause & feel** — A **ScreenSpace-overlay canvas** is assembled in code with anchored panels (deaths/gems, timer, checkpoint, dash, hints), a victory panel and an **ESC pause menu**. Hazards kill on contact with a respawn cooldown; death bursts sell the feedback.
6. **Automated build** — A **custom editor pipeline** regenerates the whole level and compiles the Windows player via two headless Unity jobs, verified by `REGEN_SUCCEEDED` and `BUILD_SUCCEEDED` log markers.

---

## Game Overview

LAVA ESCAPE drops the player at a floating stone gate above a boiling lava canyon. Every stage of the linear course is generated procedurally from code: start sprint deck, precision-hop micro platforms, a curved wooden beam bridge suspended over the lava, ascending stair-step platforms, and a glowing finish gate that triggers the victory panel. A roaming in-game clock tracks the run; the best completed time is remembered across attempts.

### Feature Highlights

**Living lava ocean** — A 1600×2400 unit emissive sea sits beneath the entire course. Its surface is animated by a custom Voronoi + Fractal-Brownian-Motion shader (`MC_Lava`) that layers crack formation, churning crust, and hot/white glow that responds to world-space flow.

**Checkpoint & respawn loop** — Falling into lava or touching a spinning hazard scripts a death — a brief ghost burst, a 0.6 s cooldown, then a teleport back to the most recent checkpoint. 11 checkpoints divide the course, and the HUD badge updates live (**CP 3 / 11**).

**Game-feel movement stack** — WASD movement with 0.18 s **coyote time** and 0.14 s **jump buffer**; quick-tap Shift triggers a **dash** that drops time to **0.2×** slow-motion; holding Shift sprints; Ctrl slides; R instantly restarts.

**Cinematic day sky & lighting** — `RenderSettings` draws a partly-cloudy HDRI panorama; a warm key sun (intensity 1.15), a cool fill light (0.28), sky-box ambient, and ExponentialSquared haze give the canyon a bright, readable, premium look under bloom.

**Clean, aligned UI + pause** — Canvas UI at a 1920×1080 reference with balanced scaling keeps every panel inside the frame. Top-left deaths/gems card, top-center live timer + best, top-right checkpoint badge, bottom-left dash status, bottom-centre control hints. **ESC** opens a full pause menu (Resume / Restart / Quit) with lock-step time handling.

---

## Core Systems & Architecture

| | |
|---|---|
| **Procedural Level Builder** — An Editor tool (`CourseBuilder`) generates the entire world from code — decks, platforms, curved beams, gates, checkpoints, gems, hazards — pushing a linear **cursor** along +Z so stages lay out deterministically on every regen. Lighting, skybox and materials are created in the same pass. | **GameManager** — A single runtime manager owns the run clock, best time, death counter, checkpoint index, 13-gem collection, and events (**OnDeath / OnCheckpointChanged / OnGemCollected / OnFinish**) that drive every HUD element without polling. |
| **Hazard & Kill System** — Two kill paths: `Hazard` markers kill on the very first physics *or* trigger contact (no lucky phase-through), and a giant kill plane at the lava surface catches falls. Respawn is guarded by a 0.6 s cooldown and fires a particle burst at the death point. | **Player Controller** — A single first-person controller handles ground detection, coyote time & jump buffering, sprint/dash/slide states, and a slow-motion time-scale (0.2×) that is safely restored — even if the game pauses mid-dash. Teleport-to-checkpoint is non-physics (rigidbody-disabled) for glitch-free landings. |
| **HUD Canvas Stack** — A ScreenSpace-overlay canvas builds rounded-corner pills from a procedurally generated sprite — no external art assets. Every element gets explicit anchor & pivot so nothing drifts off-screen, with CanvasScaler matching width/height at 0.5. | **Pause Menu (ESC)** — `ESC` freezes `Time.timeScale` and shows a full-screen dim panel with Resume (`ESC`), Restart (`R`), Quit (`Q`). Restart while paused re-wires itself to avoid the stuck-at-0-timescale trap. |

---

## Rendering & Visual Effects

### Custom Lava Shader — `MC_Lava`

The signature look: an emissive Voronoi cell layer merges with 5-octave fbm turbulence to form heat-fractured crust, hot rivulets, and white-overseed glow centers. Properties expose crust / deep / hot / white colors, flow speed, ripple power, churn, and emissive energy, so the whole sea can be re-tuned per level.

| Pass | Method |
|---|---|
| Cell structure | Voronoi seed lattice → distance-to-edge for crack lines |
| Motion | Dual-direction flow UV scrolling + time-driven ripple |
| Glow | Emissive ramp into URP Bloom (0.55 threshold / 0.65 scatter) |
| Output | Universal Forward Lit, 5-octave fbm normal turbulence |

### URP Volume Stack

| Effect | Setting | Purpose |
|---|---|---|
| Bloom | Threshold 0.55, intensity 0.65, scatter 0.65 | Lava & neon gates glow softly |
| Color Adjustments | Post-exposure 0.12, contrast 6, saturation 8 | Punchy, readable day grading |
| Vignette | Intensity 0.26, smoothness 0.4 | Centre-weighted focus for speedruns |
| Sky | Partly-cloudy HDRI panorama (kloofendal) | Bright natural daylight base |
| Fog | ExponentialSquared, density 0.0025 | Atmospheric depth over the canyon |

### Lighting Rig

Warm directional **Sun** (color 1.00/0.94/0.86, intensity **1.15**, soft shadows, angled 52°) is the key; a cool hemisphere **Fill** (0.88/0.90/1.00, intensity **0.28**) lifts shadow areas. Ambient is inherited from the skybox so the horizon tint matches the day panorama. Decking uses a matte PBR finish to keep the lava as the clear hero glow.

---

## In-Game Screenshots

### 01 · The Course From Above

![The Course From Above](Report/shots_small/01_hero_wide.jpg)

> Establishing frame — the full parkour spine stretches over the animated lava canyon under a partly-cloudy day sky.

### 02 · Start Gate

![Start Gate](Report/shots_small/02_start_gate.jpg)

> The spawn deck and glowing start gate — the first diamond hovers just beyond the threshold.

### 03 · Precision Hop Micro-Platforms

![Precision Hop Micro-Platforms](Report/shots_small/03_stage1_jump.jpg)

> Stage-1 hairpin steppers demand timed jumps — missteps meet the lava below.

### 04 · Curved Beam Bridge

![Curved Beam Bridge](Report/shots_small/04_beam_bridge.jpg)

> The wood beam over the chasm doubles as the course's mid-way confidence test.

### 05 · The Lava Sea

![The Lava Sea](Report/shots_small/05_lava_sea.jpg)

> Close-up of the custom Voronoi–fBM shader: crack networks, hot rivulets and white-glow cores.

### 06 · Finish Gate

![Finish Gate](Report/shots_small/06_endgate.jpg)

> Crossing this gate freezes the timer and opens the COURSE CLEARED victory panel with run stats.

---

## Controls

| Key | Action |
|---|---|
| `W A S D` | Move |
| `Mouse` | Look |
| `Space` | Jump (buffered + coyote time) |
| `Shift` tap | Dash — 0.2× slow-motion flair |
| `Shift` hold | Sprint |
| `Ctrl` | Slide |
| `ESC` | Pause menu (Resume / Restart / Quit) |
| `R` | Instant restart |

---

## Build & Testing

### Automated Build Pipeline

Every scene change is verified through two headless Unity jobs — regenerate the level, then compile the Windows player — with explicit success markers in the logs:

```
[BuildHelper] REGEN_SUCCEEDED
[BuildHelper] BUILD_SUCCEEDED C:/Users/.../DESKTOP/PARKOUR_GAME/Build/ParkourGame.exe
```

Editor methods: **`BuildHelper.PerformRegen`** (scene = game state), **`BuildHelper.PerformBuild`** (Windows Standalone player), and **`CaptureShots.Capture`** (offscreen RT rendering, 1920×1080).

### Testing Summary

**Gameplay**
- Hazard kills on physics AND trigger contact — no ghosting
- Respawn cooldown + death burst verified
- Dash slow-mo restores cleanly, incl. pause-during-dash
- Checkpoint counter tracks 11/11 across a full run
- All 13 gems collectible; victory panel totals correct

**UI / Render**
- HUD stays inside frame at 16:9, 16:10 & 21:9 references
- Controls hint no longer collides with dash panel
- Day skybox + fog + bloom render at near 25k-color detail
- No compile errors in regen/build logs

---

## Conclusion & Future Scope

### What was achieved

LAVA ESCAPE ships a complete, playable first-person speedrunning experience: a procedurally generated 6-stage course, a bespoke Voronoi–fBM lava shader, game-feel movement (dash, slide, sprint, coyote time), a checkpoint/respawn loop that humbles every fall, a clean aligned HUD with live splits, and a full ESC pause menu — all rendered in URP with a cinematic day mood. The entire pipeline is automated: one click regenerates the world, another produces a verified Windows build.

### Future scope

- Persistent best-time leaderboards and run replays (ghost)
- VR / Extended Reality camera mode — full-body presence in the canyon
- Moving physics hazards & responsive lava-sea breath animation
- Procedural stage splicing for infinite speedrun variety
- Audio: dynamic music bed that intensifies near checkpoints

---

<p align="center">
  <b>LAVA ESCAPE</b><br>
  <sub>Extended Realities · ADS351 · B.Tech Artificial Intelligence &amp; Machine Learning<br>
  School of Engineering & Technology / CHRIST (Deemed to be University) · Bengaluru</sub>
</p>