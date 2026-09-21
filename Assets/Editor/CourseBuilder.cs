using System.Collections.Generic;
using MainCourse;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MainCourseEditor
{
    public static class CourseBuilder
    {
        const string ScenePath = "Assets/Scenes/MainCourse.unity";
        const float Thickness = 0.6f;
        const float PresetGap = 2.5f;
        const float CorridorWallH = 6f;

        static readonly Color SkyC = new Color(0.49f, 0.78f, 0.99f);
        static readonly Color Yellow = new Color(1f, 0.82f, 0.24f);
        static readonly Color Pink = new Color(1f, 0.36f, 0.54f);
        static readonly Color Teal = new Color(0.26f, 0.86f, 0.80f);
        static readonly Color Purple = new Color(0.60f, 0.36f, 0.90f);
        static readonly Color Red = new Color(0.98f, 0.26f, 0.28f);
        static readonly Color Green = new Color(0.30f, 0.85f, 0.42f);
        static readonly Color Mint = new Color(0.55f, 1f, 0.85f);
        static readonly Color Orange = new Color(1f, 0.55f, 0.12f);
        static readonly Color WaterDeep = new Color(0.10f, 0.35f, 0.78f);
        static readonly Color Skin = new Color(0.96f, 0.76f, 0.60f);
        static readonly Color Shirt = new Color(0.98f, 0.72f, 0.16f);
        static readonly Color CloudW = new Color(0.94f, 0.97f, 1f);
        static readonly Color Distant = new Color(0.33f, 0.29f, 0.42f);
        static readonly Color NeonC = new Color(0.42f, 1f, 0.95f);
        static readonly Color Warm = new Color(1f, 0.88f, 0.45f);

        static Material deckA, deckB, deckC, deckD, hazard, accent, mint, lava, water, ocean, skybox, skin, shirt, cloud, distant, wallBrick, wood, cliff, neon, gold, pink, skyDome, trailMat;
        static Transform world;
        static Transform startMarker;
        static float cursor;

        [MenuItem("Speedrun/Generate Level")]
        public static void Generate()
        {
            Build();
        }

        public static void Build()
        {
            URPSetup.EnsureUrp();

            EnsureFolder("Assets", "Scenes");
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "MainCourse");
            EnsureFolder("Assets", "Settings");

            CreateMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupLighting();
            world = new GameObject("WORLD").transform;

            BuildOcean();
            BuildCourse();
            BuildAmbience();
            BuildSkyRing();

            SpawnPlayer();
            CreateGameManager();
            HUDBuilder.Build();

            BuildVolume();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterScene();

            Debug.Log("[MainCourse] LEVEL_BUILT " + ScenePath);
        }

        /* ---------------- course ---------------- */

        static void BuildCourse()
        {
            cursor = 0f;
            Deck("START", 18f, 14f, deckA);
            startMarker = new GameObject("Start").transform;
            startMarker.SetParent(world, false);
            startMarker.SetPositionAndRotation(new Vector3(0f, 0.2f, 2.5f), Quaternion.identity);
            StartGate();
            Gem(new Vector3(0f, 1.9f, 4.4f), 0.55f);

            // STAGE 1 - precision hop
            Deck("STAGE_01_RUNUP", 16f, 12f, deckB);
            Platform("P1_A", 2.0f, 2.0f, 0f, 0.8f, deckC, 2.8f);
            Platform("P1_B", 2.0f, 2.0f, 0.7f, 1.2f, deckC, 2.8f);
            Platform("P1_C", 2.0f, 2.0f, -0.7f, 1.6f, deckC, 2.8f);
            Deck("STAGE_01_LANDING", 16f, 10f, deckB);
            AddCheckpoint(1, cursor - 4f, 16f, 0f);

            // STAGE 2 - beam walk
            Deck("STAGE_02_ENTRY", 16f, 8f, deckA);
            CurvedBridge("BEAM_BRIDGE", 1.1f, 14f, wood);
            Deck("STAGE_02_EXIT", 16f, 12f, deckA);

            // STAGE 3 - ascending precision jumps
            Deck("STAGE_03_ENTRY", 16f, 10f, deckB);
            Platform("P3_A", 1.8f, 1.8f, 0f, 1.2f, deckC, 2.6f);
            Platform("P3_B", 1.8f, 1.8f, 1.1f, 2.4f, deckD, 2.6f);
            Platform("P3_C", 1.8f, 1.8f, -1.1f, 3.6f, deckC, 2.6f);
            Platform("P3_D", 1.8f, 1.8f, 1.1f, 4.8f, deckD, 2.6f);
            Platform("P3_E", 1.8f, 1.8f, 0f, 6.0f, deckC, 2.6f);
            RaisedDeck("STAGE_03_LANDING", 16f, 12f, deckB, 6f);
            AddCheckpoint(2, cursor - 4f, 16f, 6f);

            // STAGE 4 - wall-run corridor
            Deck("STAGE_04_ENTRY", 14f, 10f, deckC);
            WallRunCorridor(22f, 4.8f, 6f, 6f);
            RaisedDeck("STAGE_04_EXIT", 14f, 14f, deckC, 0f);
            AddCheckpoint(3, cursor - 4f, 14f, 0f);

            // STAGE 5 - rotating hazard beams
            float s5 = cursor;
            Deck("STAGE_05_FLOOR", 12f, 24f, deckD);
            Spinner(-1.5f, s5 + 7f, 110f, 0f);
            Spinner(1.5f, s5 + 13f, 150f, 120f);
            Spinner(-1.5f, s5 + 19f, 95f, 240f);
            AddCheckpoint(4, cursor - 3f, 12f, 0f);

            // STAGE 6 - stair climb
            Deck("STAGE_06_ENTRY", 12f, 8f, deckA);
            Step(1.2f, 3.2f, 9f);
            Step(2.4f, 3.2f, 9f);
            Step(3.6f, 3.2f, 9f);
            Step(4.8f, 3.2f, 9f);
            RaisedDeck("STAGE_06_TOP", 14f, 12f, deckB, 6f);

            // STAGE 7 - long jumps
            Deck("STAGE_07_ENTRY", 12f, 8f, deckC, 6f);
            Platform("P7_A", 3.2f, 3.2f, 0f, 7f, deckD, 4.6f);
            Platform("P7_B", 3.2f, 3.2f, 0f, 8f, deckA, 4.6f);
            Platform("P7_C", 3.2f, 3.2f, 0f, 9f, deckD, 4.6f);
            RaisedDeck("STAGE_07_END", 12f, 12f, deckC, 9f);
            AddCheckpoint(5, cursor - 3f, 12f, 9f);

            // STAGE 8 - narrow high line
            Deck("STAGE_08_ENTRY", 12f, 8f, deckD, 9f);
            Platform("P8_A", 1.3f, 1.3f, 1.2f, 10f, deckA, 2.4f);
            Platform("P8_B", 1.3f, 1.3f, -1.2f, 10.5f, deckC, 2.4f);
            Platform("P8_C", 1.3f, 1.3f, 1.2f, 11f, deckA, 2.4f);
            Platform("P8_D", 1.3f, 1.3f, -1.2f, 11.5f, deckC, 2.4f);
            RaisedDeck("STAGE_08_EXIT", 12f, 10f, deckD, 11.5f);

            // STAGE 9 - final precision run
            Deck("STAGE_09_ENTRY", 12f, 8f, deckB, 11.5f);
            Platform("P9_A", 1.15f, 1.15f, 0f, 12.5f, deckA, 2.2f);
            Platform("P9_B", 1.15f, 1.15f, 0.8f, 13f, deckC, 2.2f);
            Platform("P9_C", 1.15f, 1.15f, -0.8f, 13.5f, deckA, 2.2f);
            Platform("P9_D", 1.15f, 1.15f, 0.8f, 14f, deckC, 2.2f);
            Platform("P9_E", 1.15f, 1.15f, 0f, 14.5f, deckA, 2.2f);
            RaisedDeck("STAGE_09_END", 14f, 12f, deckB, 14.5f);
            AddCheckpoint(6, cursor - 4f, 14f, 14.5f);

            // STAGE 10 - turbine causeway (timed turnstile gates)
            RaisedDeck("STAGE_10_ENTRY", 14f, 10f, deckB, 13f);
            Platform("P10_A", 2.6f, 2.6f, 0f, 13.4f, deckD, 3.2f);
            Platform("P10_B", 2.6f, 2.6f, 0f, 13.8f, deckA, 3.2f);
            RaisedDeck("STAGE_10_BRIDGE", 10f, 24f, deckC, 14.2f);
            TurbineGate(cursor - 11f, 4.4f, 14.4f);
            TurbineGate(cursor - 4f, 4.4f, 14.4f);
            RaisedDeck("STAGE_10_LANDING", 14f, 12f, deckB, 13.4f);
            AddCheckpoint(7, cursor - 4f, 14f, 13.4f);

            // STAGE 11 - flicker steps over the sea
            Deck("STAGE_11_ENTRY", 12f, 8f, deckD, 13.4f);
            FlickerSteps();
            RaisedDeck("STAGE_11_LANDING", 14f, 12f, deckB, 16.5f);
            AddCheckpoint(8, cursor - 4f, 14f, 16.5f);

            // STAGE 12 - bobbing shuttle isles
            Deck("STAGE_12_ENTRY", 12f, 8f, deckA, 16.5f);
            BobSeries();
            RaisedDeck("STAGE_12_LANDING", 14f, 12f, deckD, 20f);
            AddCheckpoint(9, cursor - 4f, 14f, 20f);

            // STAGE 13 - sky beam with a hop spinner
            Deck("STAGE_13_ENTRY", 12f, 8f, deckC, 20f);
            Beam("SKY_BEAM", 1.2f, 40f, wood, 20.4f);
            SpinnerHop(0f, cursor - 22f, 95f, 60f, 20.85f);
            RaisedDeck("STAGE_13_LANDING", 12f, 10f, deckB, 20f);
            AddCheckpoint(10, cursor - 4f, 12f, 20f);

            // STAGE 14 - gauntlet sprint (hop spikes + overhead sweep)
            Deck("STAGE_14_ENTRY", 12f, 8f, deckA, 20f);
            float s14 = cursor;
            SpinnerHop(-2.4f, s14 + 7f, 135f, 0f, 20.55f);
            SpinnerHop(2.4f, s14 + 13f, 135f, 120f, 20.55f);
            Spinner(0f, s14 + 18f, 150f, 40f, 21.8f, 0.3f, 12f);
            Deck("STAGE_14_LANDING", 14f, 12f, deckC, 20f);
            AddCheckpoint(11, cursor - 4f, 14f, 20f);

            // STAGE 15 - final ascent to the finish
            Deck("STAGE_15_ENTRY", 12f, 8f, deckB, 21f);
            Platform("P15_A", 2.2f, 2.2f, 0f, 22f, deckD, 2.6f);
            Platform("P15_B", 2.2f, 2.2f, 0.7f, 23f, deckA, 2.6f);
            FlickerStep("F15_C", 2.2f, -0.5f, 24f, 2.6f, 1.8f, 0.8f, 1.1f);
            Platform("P15_D", 2.2f, 2.2f, 0.5f, 25f, deckD, 2.6f);
            RaisedDeck("STAGE_15_FINISH", 18f, 16f, deckA, 25.5f);
            FinishZone(cursor - 4f, 16f, 25.5f);
        }

        static void StartGate()
        {
            Arch("StartArch", new Vector3(0f, 0f, 1f), 13f, 5.4f, 0.5f, accent);
            Cube("StartArrow", world, new Vector3(0f, 0.7f, 1.4f), new Vector3(2f, 0.15f, 3f), accent);
        }

        static void Arch(string name, Vector3 center, float span, float peak, float thick, Material mat)
        {
            const int N = 11;
            float hw = span * 0.5f;
            for (int i = 0; i < N; i++)
            {
                float t = (float)i / (float)(N - 1);
                float tA = Mathf.Max(0f, (float)(i - 1) / (float)(N - 1));
                float tB = Mathf.Min(1f, (float)(i + 1) / (float)(N - 1));
                float y = 4f * peak * t * (1f - t);
                float xA = Mathf.Lerp(-hw, hw, tA);
                float xB = Mathf.Lerp(-hw, hw, tB);
                float yA = 4f * peak * tA * (1f - tA);
                float yB = 4f * peak * tB * (1f - tB);
                float ang = Mathf.Atan2(yB - yA, xB - xA) * Mathf.Rad2Deg;
                float segLen = (span / (N - 1)) * 1.6f;

                var g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                g.name = name + "_" + i;
                g.transform.SetParent(world, false);
                g.transform.position = center + new Vector3(Mathf.Lerp(-hw, hw, t), y, 0f);
                g.transform.localScale = new Vector3(thick, segLen * 0.5f, thick);
                g.transform.rotation = Quaternion.Euler(0f, 0f, ang - 90f);
                g.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
        }

        static void WallRunCorridor(float length, float innerWidth, float gapStart, float gapLength)
        {
            float z0 = cursor + 0.5f;

            // floor: lead-in, gap, tail
            Cube("WR_FloorLead", world, new Vector3(0f, -Thickness * 0.5f, z0 + 3f), new Vector3(innerWidth, Thickness, 6f), deckD);
            float gapZ0 = z0 + gapStart;
            float tailZ0 = gapZ0 + gapLength;
            float tailLen = length - gapLength - gapStart;
            if (tailLen > 0f)
                Cube("WR_FloorTail", world, new Vector3(0f, -Thickness * 0.5f, tailZ0 + tailLen * 0.5f), new Vector3(innerWidth, Thickness, tailLen), deckD);

            // hazard glow under the pit
            var lavaGo = Cube("WR_Lava", world, new Vector3(0f, -4.5f, gapZ0 + gapLength * 0.5f), new Vector3(innerWidth + 2f, 0.5f, gapLength + 2f), lava, false);
            lavaGo.AddComponent<MainCourse.KillTrigger>();

            cursor = z0 + length;
        }

        static void Spinner(float x, float z, float speed, float phase, float pivotH = 1.0f, float barH = 0.3f, float barLen = 15f)
        {
            var pivot = new GameObject("Spinner_z" + z.ToString("0"));
            pivot.transform.SetParent(world, false);
            pivot.transform.SetPositionAndRotation(new Vector3(x, pivotH, z), Quaternion.Euler(0f, phase, 0f));

            var rb = pivot.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            Cube("Post", pivot.transform, new Vector3(0f, -pivotH - 0.6f, 0f), new Vector3(0.6f, 2f, 0.6f), deckD);
            Cube("Hub", pivot.transform, new Vector3(0f, 0f, 0f), new Vector3(0.7f, 0.7f, 0.7f), accent);
            var bar = Cube("Bar", pivot.transform, new Vector3(0f, barH, 0f), new Vector3(barLen, 0.5f, 0.5f), hazard);
            bar.AddComponent<MainCourse.Hazard>();

            var spin = pivot.AddComponent<MainCourse.HazardSpinner>();
            spin.axis = Vector3.up;
            spin.speed = speed;
        }

        static void SpinnerHop(float x, float z, float speed, float phase, float deckTopY)
        {
            Spinner(x, z, speed, phase, deckTopY + 0.45f, 0f, 12f);
        }

        static void TurbineGate(float z, float corridorW, float topY)
        {
            float half = corridorW * 0.5f;
            float gateY = topY + 0.55f;

            Cube("TG_PostL", world, new Vector3(-half - 0.8f, topY + 1.7f, z), new Vector3(0.8f, 3.4f, 0.8f), deckD);
            Cube("TG_PostR", world, new Vector3(half + 0.8f, topY + 1.7f, z), new Vector3(0.8f, 3.4f, 0.8f), deckD);
            Cube("TG_Axle", world, new Vector3(0f, topY + 3.0f, z), new Vector3(half * 2f + 1.6f, 0.5f, 0.5f), accent);

            var pivot = new GameObject("Gate_z" + z.ToString("0"));
            pivot.transform.SetParent(world, false);
            pivot.transform.SetPositionAndRotation(new Vector3(0f, gateY, z), Quaternion.identity);

            var rb = pivot.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var b1 = Cube("GateBar1", pivot.transform, new Vector3(0f, 1.55f, 0f), new Vector3(0.5f, 1.6f, 0.5f), hazard);
            b1.AddComponent<MainCourse.Hazard>();
            var b2 = Cube("GateBar2", pivot.transform, new Vector3(0f, -1.55f, 0f), new Vector3(0.5f, 1.6f, 0.5f), hazard);
            b2.AddComponent<MainCourse.Hazard>();

            var spin = pivot.AddComponent<MainCourse.HazardSpinner>();
            spin.axis = Vector3.right;
            spin.speed = 140f;
        }

        static void Step(float topY, float depth, float width)
        {
            float z = cursor + PresetGap;
            var go = Cube("Step@" + topY.ToString("0.0"), world, new Vector3(0f, topY - Thickness * 0.5f, z + depth * 0.5f), new Vector3(width, Thickness, depth), deckC);
            Support(go.transform.position + Vector3.up * Thickness * 0.5f, Mathf.Min(topY, 6f), 4f);
            cursor = z + depth;
        }

        static void Support(Vector3 deckCenter, float deckTopY, float halfSpan)
        {
            float bottom = -2f;
            float h = deckTopY - Thickness - bottom;
            if (h <= 0f) return;
            float cy = bottom + h * 0.5f;
            for (int i = 0; i < 2; i++)
            {
                float sx = deckCenter.x + (i == 0 ? -halfSpan : halfSpan) * 0.55f;
                Pill("Pillar", world, new Vector3(sx, cy, deckCenter.z), 0.5f, h, deckA);
            }
        }

        /* ---------------- pieces ---------------- */

        static GameObject Cube(string name, Transform parent, Vector3 localPos, Vector3 size, Material mat, bool solid = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!solid)
                Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject Pill(string name, Transform parent, Vector3 localPos, float diameter, float length, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        static void Orb(string name, Transform parent, Vector3 localPos, float radius, Material mat, bool solid = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * radius * 2f;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!solid)
                Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        static void CurvedBridge(string name, float width, float length, Material mat, float amplitude = 3.2f, float rise = 1.5f)
        {
            const int N = 9;
            float start = cursor;
            float step = length / (N - 1);
            for (int i = 0; i < N; i++)
            {
                float u = (float)i / (float)(N - 1);
                float z = start + i * step;
                float x = Mathf.Sin(u * Mathf.PI) * amplitude * 0.35f;
                float y = Mathf.Sin(u * Mathf.PI) * rise;
                float prevU = (i == 0) ? (float)(i + 1) / (float)(N - 1) : (float)(i - 1) / (float)(N - 1);
                float nextU = (i == N - 1) ? (float)(i - 1) / (float)(N - 1) : (float)(i + 1) / (float)(N - 1);
                float slope = (Mathf.Sin(nextU * Mathf.PI) - Mathf.Sin(prevU * Mathf.PI)) / (nextU - prevU);
                float angle = Mathf.Atan2(slope * amplitude * 0.35f, step) * Mathf.Rad2Deg;
                Cube(name + "_" + i, world, new Vector3(x, y - 0.17f, z), new Vector3(width, 0.35f, step * 1.5f), mat)
                    .transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            cursor = start + length;
        }

        static GameObject Deck(string name, float width, float length, Material mat, float topY = 0f, float gap = -1f)
        {
            float g = gap < 0f ? PresetGap : gap;
            float start = cursor + g;
            float center = start + length * 0.5f;
            var go = Cube(name, world, new Vector3(0f, topY - Thickness * 0.5f, center), new Vector3(width, Thickness, length), mat);
            cursor = start + length;
            return go;
        }

        static GameObject RaisedDeck(string name, float width, float length, Material mat, float topY)
        {
            var go = Deck(name, width, length, mat, topY);
            Support(go.transform.position + Vector3.up * Thickness * 0.5f, topY, width * 0.4f);
            return go;
        }

        static GameObject Platform(string name, float w, float depth, float x, float topY, Material mat, float gap)
        {
            float start = cursor + gap;
            float center = start + depth * 0.5f;
            var go = Cube(name, world, new Vector3(x, topY - Thickness * 0.5f, center), new Vector3(w, Thickness, depth), mat);
            cursor = start + depth;
            return go;
        }

        static GameObject Beam(string name, float width, float length, Material mat, float topY = 0f)
        {
            float start = cursor;
            float center = start + length * 0.5f;
            var go = Cube(name, world, new Vector3(0f, topY - 0.085f, center), new Vector3(width, 0.35f, length), mat);
            cursor = start + length;
            return go;
        }

        static void FlickerSteps()
        {
            FlickerStep("F11_A", 1.9f, 0f, 14.4f, 3.0f, 1.7f, 0.9f, 0f);
            FlickerStep("F11_B", 1.9f, 0.9f, 14.8f, 3.0f, 1.5f, 0.8f, 0.9f);
            FlickerStep("F11_C", 1.9f, -0.9f, 15.2f, 3.0f, 1.7f, 0.9f, 1.8f);
            FlickerStep("F11_D", 1.9f, 0f, 16.0f, 3.0f, 1.6f, 0.8f, 2.6f);
        }

        static void FlickerStep(string name, float size, float x, float topY, float gap, float onTime, float offTime, float phase)
        {
            float start = cursor + gap;
            float center = start + size * 0.5f;
            var go = Cube(name, world, new Vector3(x, topY - Thickness * 0.5f, center), new Vector3(size, Thickness, size), deckC);
            var fl = go.AddComponent<MainCourse.FlickerPlatform>();
            fl.onTime = onTime;
            fl.offTime = offTime;
            fl.startOffset = phase;
            cursor = start + size;
        }

        static void BobSeries()
        {
            MovingStep("M12_A", 2.5f, 0f, 17.2f, 3.0f, Vector3.up, 1.15f, 1.5f, 0f);
            MovingStep("M12_B", 2.5f, 1.1f, 18.1f, 3.0f, Vector3.up, 1.35f, 1.7f, 0.25f);
            MovingStep("M12_C", 2.5f, -1.1f, 19.0f, 3.0f, Vector3.up, 1.2f, 1.6f, 0.5f);
        }

        static void MovingStep(string name, float size, float x, float topY, float gap, Vector3 axis, float travel, float period, float phase)
        {
            float start = cursor + gap;
            float center = start + size * 0.5f;
            var go = Cube(name, world, new Vector3(x, topY - Thickness * 0.5f, center), new Vector3(size, Thickness, size), deckD);
            var mp = go.AddComponent<MainCourse.MovingPlatform>();
            mp.axis = axis;
            mp.halfTravel = travel;
            mp.period = period;
            mp.phase = phase;
            cursor = start + size;
        }

        static void AddCheckpoint(int index, float z, float width, float topY)
        {
            var root = new GameObject("CHECKPOINT_" + index);
            root.transform.SetParent(world, false);
            root.transform.position = new Vector3(0f, topY, z);

            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(width, 6f, 2.2f);
            col.center = new Vector3(0f, 3f, 0f);

            var cp = root.AddComponent<CheckpointTrigger>();
            cp.index = index;

            var spawn = new GameObject("Spawn").transform;
            spawn.SetParent(root.transform, false);
            spawn.localPosition = new Vector3(0f, 0.05f, -3.2f);
            spawn.localRotation = Quaternion.identity;
            cp.spawnPoint = spawn;

            Cube("PostL", root.transform, new Vector3(-width * 0.5f + 0.4f, 2.2f, 0f), new Vector3(0.35f, 4.4f, 0.35f), mint);
            Cube("PostR", root.transform, new Vector3(width * 0.5f - 0.4f, 2.2f, 0f), new Vector3(0.35f, 4.4f, 0.35f), mint);
            Cube("CapL", root.transform, new Vector3(-width * 0.5f + 0.4f, 4.6f, 0f), new Vector3(0.55f, 0.28f, 0.55f), accent);
            Cube("CapR", root.transform, new Vector3(width * 0.5f - 0.4f, 4.6f, 0f), new Vector3(0.55f, 0.28f, 0.55f), accent);

            Orb("CPOrbL_" + index, root.transform, new Vector3(-width * 0.5f + 0.4f, 5.8f, 0f), 0.34f, mint, false);
            Orb("CPOrbR_" + index, root.transform, new Vector3(width * 0.5f - 0.4f, 5.8f, 0f), 0.34f, accent, false);

            var lightGo = new GameObject("CheckpointLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            var lp = lightGo.AddComponent<Light>();
            lp.type = LightType.Point;
            lp.range = 10f;
            lp.intensity = 1.6f;
            lp.color = Mint;

            Gem(new Vector3(0f, topY + 1.9f, z), 0.55f);
        }

        static void Gem(Vector3 pos, float size)
        {
            var go = Cube("Gem@" + Mathf.RoundToInt(pos.z), world, pos, Vector3.one * size, gold);
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            go.AddComponent<MainCourse.Pickup>();
        }

        static void FinishZone(float z, float width, float topY)
        {
            float padY = topY + 0.6f;
            Cube("FinishPad", world, new Vector3(0f, padY, z + 2f), new Vector3(width, 0.15f, 12f), accent);

            var trigger = new GameObject("FINISH_ZONE");
            trigger.transform.SetParent(world, false);
            trigger.transform.position = new Vector3(0f, topY + 2.5f, z);
            var tcol = trigger.AddComponent<BoxCollider>();
            tcol.isTrigger = true;
            tcol.size = new Vector3(width, 8f, 3f);
            trigger.AddComponent<FinishTrigger>();

            Arch("FinishArch", new Vector3(0f, topY, z), width + 2f, 5.6f, 0.6f, hazard);

            float ringZ = z + 10.5f;
            Gem(new Vector3(0f, topY + 1.9f, ringZ - 4f), 0.55f);

            Cube("RingL", world, new Vector3(-width * 0.5f + 0.5f, topY + 3.6f, ringZ), new Vector3(0.45f, 8f, 0.45f), accent);
            Cube("RingR", world, new Vector3(width * 0.5f - 0.5f, topY + 3.6f, ringZ), new Vector3(0.45f, 8f, 0.45f), accent);
            Cube("RingTop", world, new Vector3(0f, topY + 7.5f, ringZ), new Vector3(width + 1f, 0.45f, 0.45f), accent);

            for (int i = 0; i < 2; i++)
            {
                var glow = new GameObject("FinishLight_" + i);
                glow.transform.SetParent(world, false);
                glow.transform.position = new Vector3(i == 0 ? -width * 0.4f : width * 0.4f, topY + 0.8f, ringZ);
                var lp = glow.AddComponent<Light>();
                lp.type = LightType.Point;
                lp.range = 11f;
                lp.intensity = 2.4f;
                lp.color = Green;
            }
        }
static void BuildAmbience()
        {
            Cloud(-260f, 48f, 240f, 1f);
            Cloud(300f, 64f, 440f, -1f);
            Cloud(-70f, 94f, 610f, 1f);
            Cloud(220f, 44f, -40f, -1f);
            Cloud(360f, 88f, 80f, 1f);
        }

        static void BuildSkyDome()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "SkyDome";
            go.transform.SetParent(world, false);
            go.transform.position = new Vector3(0f, 0f, 180f);
            go.transform.localScale = Vector3.one * 1800f;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = skyDome;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }

        static GameObject Crystal(string name, Transform parent, Vector3 pos, float height, Material mat, float tiltX, float tiltZ)
        {
            const int sides = 6;
            const float r = 0.55f;
            int vCount = sides + 2;
            Vector3[] vertices = new Vector3[vCount];
            vertices[0] = Vector3.zero;
            for (int i = 0; i < sides; i++)
            {
                float a = Mathf.PI * 2f * i / sides;
                vertices[1 + i] = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            }
            vertices[vCount - 1] = new Vector3(0f, height, 0f);

            int[] tris = new int[6 * sides];
            int t = 0;
            for (int i = 0; i < sides; i++)
            {
                int b = 1 + i;
                int c = 1 + (i + 1) % sides;
                int ring = vCount - 1;
                tris[t++] = 0; tris[t++] = b; tris[t++] = c;
                tris[t++] = b; tris[t++] = ring; tris[t++] = c;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(tiltX, 0f, tiltZ);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mf.sharedMesh = mesh;
            return go;
        }

        static void BuildFireflies()
        {
            var root = new GameObject("Fireflies");
            root.transform.SetParent(world, false);
            root.transform.SetPositionAndRotation(new Vector3(0f, 10f, 200f), Quaternion.Euler(14f, 0f, 0f));
            var spin = root.AddComponent<MainCourse.AmbientSpin>();
            spin.speed = 1.4f;
            spin.axis = Vector3.forward;
            Material[] mats = { accent, mint, gold, pink, mint, gold };
            int n = 24;
            for (int i = 0; i < n; i++)
            {
                float a = Mathf.PI * 2f * i / n;
                float r = 46f + (float)(i % 3) * 15f;
                Orb("Fly_" + i, root.transform, new Vector3(Mathf.Cos(a) * r, (float)(i % 5) * 1.2f, Mathf.Sin(a) * r), 0.5f + (float)(i % 4) * 0.16f, mats[i % mats.Length], false);
            }
        }

        static void AddAmbientLight(string n, Vector3 pos, float range, float intensity, Color col)
        {
            var go = new GameObject(n);
            go.transform.SetParent(world, false);
            go.transform.position = pos;
            var lp = go.AddComponent<Light>();
            lp.type = LightType.Point;
            lp.range = range;
            lp.intensity = intensity;
            lp.color = col;
            lp.shadows = LightShadows.None;
        }

        static void BuildSkyRing()
        {
            var root = new GameObject("SkyRing");
            root.transform.SetParent(world, false);
            root.transform.SetPositionAndRotation(new Vector3(0f, 68f, -180f), Quaternion.Euler(50f, 10f, 0f));

            var spin = root.AddComponent<MainCourse.AmbientSpin>();
            spin.speed = 2.6f;
            spin.axis = Vector3.forward;

            int segs = 56;
            float R = 170f;
            for (int i = 0; i < segs; i++)
            {
                float a = Mathf.PI * 2f * i / segs;
                int m = i % 7;
                float len = m == 0 ? 6.2f : (m % 2 == 0 ? 4.4f : 2.8f);
                Pill("RingSeg_" + i, root.transform, new Vector3(Mathf.Cos(a) * R, Mathf.Sin(a) * R, 0f), 2.6f, len, neon);
            }

            Cube("RingCore", root.transform, Vector3.zero, Vector3.one * 5f, neon, false);

            var lightGo = new GameObject("RingLight");
            lightGo.transform.SetParent(root.transform, false);
            var rl = lightGo.AddComponent<Light>();
            rl.type = LightType.Point;
            rl.range = 110f;
            rl.intensity = 34f;
            rl.color = NeonC;

            var orbs = new GameObject("Orbiters");
            orbs.transform.SetParent(root.transform, false);
            var orbSpin = orbs.AddComponent<MainCourse.AmbientSpin>();
            orbSpin.speed = 17f;
            orbSpin.axis = -Vector3.forward;
            float orbR = R + 26f;
            for (int i = 0; i < 4; i++)
            {
                float a = Mathf.PI * 2f * i / 4f;
                Cube("Orb_" + i, orbs.transform, new Vector3(Mathf.Cos(a) * orbR, Mathf.Sin(a) * orbR, 10f), Vector3.one * 3f, neon, false);
            }
        }

        static void Cloud(float x, float y, float z, float dir)
        {
            float s = 46f;
            Cube("Cloud_" + Mathf.RoundToInt(x) + "_" + Mathf.RoundToInt(z), world, new Vector3(x, y, z), new Vector3(s, 7f, 30f), cloud, false);
            Cube("Cloud_" + Mathf.RoundToInt(x) + "_" + Mathf.RoundToInt(z) + "b", world, new Vector3(x + dir * 16f, y + 1.5f, z + 4f), new Vector3(s * 0.6f, 6f, 24f), cloud, false);
        }

        static void BuildOcean()
        {
            Cube("OCEAN", world, new Vector3(0f, -9.6f, 200f), new Vector3(1600f, 0.5f, 2400f), ocean, false);

            var trig = new GameObject("OCEAN_KILL");
            trig.transform.SetParent(world, false);
            trig.transform.position = new Vector3(0f, -9.2f, 200f);
            var c = trig.AddComponent<BoxCollider>();
            c.isTrigger = true;
            c.size = new Vector3(1600f, 0.9f, 2400f);
            trig.AddComponent<MainCourse.KillTrigger>();
        }

        /* ---------------- player / gm / volume ---------------- */

        static void SpawnPlayer()
        {
            var playerGo = new GameObject("Player");
            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.75f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.875f, 0f);

            var pc = playerGo.AddComponent<PlayerController>();
            playerGo.transform.SetPositionAndRotation(startMarker.position, startMarker.rotation);

            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(playerGo.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 450f;
            cam.fieldOfView = 72f;
            cam.allowHDR = true;
            cam.useOcclusionCulling = false;

            var data = camGo.AddComponent<UniversalAdditionalCameraData>();
            if (data != null)
                data.renderPostProcessing = true;

            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<MainCourse.SpeedFX>();

            BuildArms(camGo.transform);

            pc.cameraPivot = camGo.transform;

            var trailGo = new GameObject("DashTrail");
            trailGo.transform.SetParent(playerGo.transform, false);
            var trail = trailGo.AddComponent<TrailRenderer>();
            trail.material = trailMat;
            trail.time = 0.34f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0.02f;
            trail.minVertexDistance = 0.12f;
            trail.startColor = new Color(NeonC.r, NeonC.g, NeonC.b, 0.95f);
            trail.endColor = new Color(NeonC.r, NeonC.g, NeonC.b, 0f);
            trail.emitting = false;
            pc.dashTrail = trail;
        }

        static void BuildArms(Transform cam)
        {
            var root = new GameObject("Arms");
            root.transform.SetParent(cam, false);
            root.transform.localPosition = new Vector3(0f, -0.27f, 0.36f);
            root.transform.localRotation = Quaternion.identity;

            var anim = root.AddComponent<ArmsAnimator>();

            var armL = ArmGroup(root.transform, "Arm_L", -1f);
            var armR = ArmGroup(root.transform, "Arm_R", 1f);
            anim.armL = armL;
            anim.armR = armR;
        }

        static Transform ArmGroup(Transform parent, string name, float side)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = new Vector3(0.36f * side, 0.0f, 0.02f);
            pivot.localRotation = Quaternion.Euler(0f, 12f * side, -8f * side);

            Cube(name + "_Sleeve", pivot, new Vector3(0f, -0.12f, -0.04f), new Vector3(0.12f, 0.24f, 0.13f), shirt);
            Cube(name + "_Hand", pivot, new Vector3(0f, -0.32f, -0.04f), new Vector3(0.11f, 0.16f, 0.12f), skin);
            return pivot;
        }

        static void CreateGameManager()
        {
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<MainCourse.GameManager>();
            gm.startPoint = startMarker;
            gm.CheckpointTotal = 11;

            var cfxGo = new GameObject("CaptureFX");
            cfxGo.transform.SetParent(gmGo.transform, false);
            var cfx = cfxGo.AddComponent<MainCourse.CaptureFX>();
            cfx.burstMat = neon;
            cfx.worldRoot = world;
            gm.captureFX = cfx;
        }

        static void BuildVolume()
        {
            var go = new GameObject("Global Volume");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 1f;
            vol.sharedProfile = URPSetup.GetOrCreateVolumeProfile();
        }

        /* ---------------- materials & lighting ---------------- */

        static void CreateMaterials()
        {
            URPSetup.ConfigureTextureImporters();

            deckA = URPSetup.MakeTexturedMaterial("DeckYellow", "concrete_tiles_02_diff_1k.jpg", "concrete_tiles_02_nor_gl_1k.jpg", new Color(0.93f, 0.70f, 0.28f), 0.28f, tiling: new Vector2(2f, 2f));
            deckB = URPSetup.MakeTexturedMaterial("DeckPink", "concrete_tiles_02_diff_1k.jpg", "concrete_tiles_02_nor_gl_1k.jpg", new Color(0.96f, 0.52f, 0.64f), 0.28f, tiling: new Vector2(2f, 2f));
            deckC = URPSetup.MakeTexturedMaterial("DeckTeal", "concrete_tiles_02_diff_1k.jpg", "concrete_tiles_02_nor_gl_1k.jpg", new Color(0.30f, 0.78f, 0.72f), 0.28f, tiling: new Vector2(2f, 2f));
            deckD = URPSetup.MakeTexturedMaterial("DeckPurple", "concrete_tiles_02_diff_1k.jpg", "concrete_tiles_02_nor_gl_1k.jpg", new Color(0.58f, 0.44f, 0.86f), 0.28f, tiling: new Vector2(2f, 2f));
            wallBrick = URPSetup.MakeTexturedMaterial("WallBrick", "brick_wall_001_diffuse_1k.jpg", "brick_wall_001_nor_gl_1k.jpg", new Color(0.88f, 0.82f, 0.78f), 0.2f, tiling: new Vector2(2f, 2f));
            wood = URPSetup.MakeTexturedMaterial("WoodPlanks", "brown_planks_05_diff_1k.jpg", "brown_planks_05_nor_gl_1k.jpg", Color.white, 0.3f, tiling: new Vector2(2f, 2f));
            cliff = URPSetup.CreateShaderMaterial("IslandRockMat", "MainCourse/IslandRock");
            if (cliff != null)
            {
                cliff.SetColor("_Base", new Color(0.42f, 0.26f, 0.58f, 1f));
                cliff.SetColor("_Band", new Color(0.85f, 0.34f, 0.62f, 1f));
                cliff.SetColor("_Rim", new Color(1f, 0.38f, 0.72f, 1f));
                cliff.SetFloat("_RimPower", 3.2f);
                cliff.SetFloat("_RimStrength", 1.6f);
                cliff.SetFloat("_BandScale", 1.1f);
            }

            hazard = URPSetup.MakeMaterial("HazardRed", Red, 0f, Red * 2.6f);
            accent = URPSetup.MakeMaterial("AccentGreen", Green, 0.1f, Green * 2.6f);
            mint = URPSetup.MakeMaterial("CheckpointMint", Mint, 0.05f, Mint * 2.2f);
            lava = URPSetup.MakeMaterial("LavaOrange", Orange, 0f, Orange * 2.8f);
            ocean = URPSetup.CreateShaderMaterial("LavaSea2", "MainCourse/Lava");
            if (ocean != null)
            {
                ocean.SetColor("_Crust", new Color(0.03f, 0.01f, 0.006f, 1f));
                ocean.SetColor("_Deep", new Color(0.60f, 0.06f, 0.012f, 1f));
                ocean.SetColor("_Hot", new Color(0.98f, 0.34f, 0.04f, 1f));
                ocean.SetColor("_White", new Color(1f, 0.84f, 0.5f, 1f));
                ocean.SetFloat("_Flow", 0.3f);
                ocean.SetFloat("_Ripple", 0.55f);
                ocean.SetFloat("_Emissive", 3.8f);
                ocean.SetFloat("_Churn", 1.0f);
                ocean.SetFloat("_Cell", 1.3f);
                ocean.SetFloat("_Edge", 0.6f);
                ocean.SetVector("_FlowDir", new Vector4(0.35f, 0f, 0.94f, 0f));
            }
            water = null;
            neon = URPSetup.MakeMaterial("NeonRing", NeonC, 0f, NeonC * 2.9f);
            gold = URPSetup.MakeMaterial("GemGold", Warm, 0.15f, Warm * 2.5f);
            pink = URPSetup.MakeMaterial("NeonPink", new Color(1f, 0.28f, 0.68f, 1f), 0f, new Color(1f, 0.28f, 0.68f, 1f) * 4f);
            skyDome = URPSetup.CreateShaderMaterial("SkyDomeMat", "MainCourse/SkyDome");
            if (skyDome != null)
            {
                skyDome.SetColor("_Zenith", new Color(0.032f, 0.052f, 0.17f, 1f));
                skyDome.SetColor("_Mid", new Color(0.065f, 0.11f, 0.29f, 1f));
                skyDome.SetColor("_Horizon", new Color(0.16f, 0.11f, 0.32f, 1f));
                skyDome.SetColor("_Sun", new Color(0.60f, 0.76f, 1f, 1f));
                skyDome.SetVector("_SunDir", new Vector4(0.35f, 0.62f, 0.35f, 0f));
                skyDome.SetFloat("_SunSpread", 11f);
                skyDome.SetFloat("_MoonSize", 0.992f);
                skyDome.SetFloat("_Glow", 0.75f);
                skyDome.SetFloat("_Stars", 1.6f);
                skyDome.SetColor("_StarTint", new Color(0.88f, 0.94f, 1f, 1f));
                skyDome.SetFloat("_StarDensity", 70f);
                skyDome.SetFloat("_Twinkle", 1f);
                skyDome.SetVector("_MWDir", new Vector4(0.25f, 0.20f, 0.95f, 0f));
                skyDome.SetFloat("_MWStrength", 1.1f);
                skyDome.SetFloat("_Nebula", 0.5f);
            }

            var sprite = Shader.Find("Sprites/Default");
            trailMat = new Material(sprite);
            trailMat.SetColor("_Color", NeonC);
            trailMat = URPSetup.SaveMaterialAsset(trailMat, "TrailSprite");

            skin = URPSetup.MakeMaterial("PlayerSkin", Skin, 0.35f);
            shirt = URPSetup.MakeMaterial("PlayerShirt", Shirt, 0.2f, Shirt * 1.3f);
            cloud = URPSetup.MakeMaterial("CloudDay", new Color(0.96f, 0.96f, 1f, 1f), 0f, new Color(0.35f, 0.38f, 0.5f, 1f));
            distant = URPSetup.MakeMaterial("IslandDark", Distant, 0f);

            skybox = URPSetup.CreateSkyMaterial();
        }

        static void SetupLighting()
        {
            var sun = new GameObject("Moon");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.94f, 0.86f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(52f, -38f, 0f);

            var fill = new GameObject("Fill");
            var l2 = fill.AddComponent<Light>();
            l2.type = LightType.Directional;
            l2.intensity = 0.28f;
            l2.color = new Color(0.88f, 0.90f, 1f);
            fill.transform.rotation = Quaternion.Euler(30f, 140f, 0f);

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.64f);

            var skyShader = Shader.Find("Skybox/Panoramic");
            if (skyShader != null)
                skybox = URPSetup.CreateSkyMaterial();
            else if (Shader.Find("Skybox/Procedural") != null)
            {
                skybox = URPSetup.CreateSkyMaterial();
            }
            if (skybox != null)
                RenderSettings.skybox = skybox;

            RenderSettings.sun = light;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0025f;
            RenderSettings.fogColor = new Color(0.68f, 0.74f, 0.82f);
        }

        static void RegisterScene()
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int existing = list.FindIndex(s => s.path == ScenePath);
            if (existing >= 0) list[existing].enabled = true;
            else list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        static void EnsureFolder(string parent, string name)
        {
            URPSetup.EnsureFolder(parent, name);
        }
    }
}