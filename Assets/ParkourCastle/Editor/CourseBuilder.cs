using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ParkourCastle.EditorTools
{
    public static class CourseBuilder
    {
        const string RootFolder = "Assets/ParkourCastle";
        const string ScenePath = RootFolder + "/Scenes/TakeshiCastle.unity";
        const string MatFolder = RootFolder + "/Materials";
        const string EnvPrefabs = "Assets/Dynamic Parkour System/Prefabs/Environment";
        const string PlayerPrefab = "Assets/Dynamic Parkour System/Prefabs/Player.prefab";

        static readonly Color Sky = new Color(0.49f, 0.78f, 0.99f);
        static readonly Color Yellow = new Color(1f, 0.82f, 0.24f);
        static readonly Color Pink = new Color(1f, 0.36f, 0.54f);
        static readonly Color Teal = new Color(0.26f, 0.86f, 0.80f);
        static readonly Color Purple = new Color(0.60f, 0.36f, 0.90f);
        static readonly Color Red = new Color(0.98f, 0.26f, 0.28f);
        static readonly Color Green = new Color(0.30f, 0.85f, 0.42f);
        static readonly Color WaterDeep = new Color(0.10f, 0.35f, 0.78f);

        const float Thickness = 0.6f;
        const float Gap = 2.5f;

        static Material deckA, deckB, deckC, deckD, hazard, accent, water, checkpointGlow;
        static Transform world;
        static Transform startMarker;
        static float cursor;

        [MenuItem("Parkour Castle/Rebuild Course Scene")]
        public static void Build()
        {
            EnsureFolder("Assets", "ParkourCastle");
            EnsureFolder(RootFolder, "Scenes");
            EnsureFolder(RootFolder, "Materials");

            CreateMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupLighting();
            world = new GameObject("WORLD").transform;

            BuildWater();
            BuildCourse();

            GameObject player = SpawnPlayer();
            BuildHud(player);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterScene();

            Debug.Log("[ParkourCastle] Course built: " + ScenePath);
        }

        /* ---------- environment helpers ---------- */

        static GameObject Cube(string name, Transform parent, Vector3 localPos, Vector3 size, Material mat, bool solid = true, string tag = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            if (!solid)
                Object.DestroyImmediate(go.GetComponent<Collider>());
            if (!string.IsNullOrEmpty(tag))
                go.tag = tag;

            return go;
        }

        static GameObject Sphere2(string name, Transform parent, Vector3 localPos, float diameter, Material mat, bool solid = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * diameter;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            if (!solid)
                Object.DestroyImmediate(go.GetComponent<Collider>());
            else if (parent.GetComponent<Rigidbody>() == null)
            {
                var rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            return go;
        }

        static GameObject Cylinder(string name, Transform parent, Vector3 localPos, Vector3 size, Material mat, bool solid = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            if (!solid)
                Object.DestroyImmediate(go.GetComponent<Collider>());

            return go;
        }

        static GameObject Deck(string name, float width, float length, Material mat, float topY = 0f, float gap = -1f)
        {
            float g = gap < 0f ? Gap : gap;
            float startZ = cursor + g;
            float centerZ = startZ + length * 0.5f;
            var go = Cube(name, world, new Vector3(0f, topY - Thickness * 0.5f, centerZ), new Vector3(width, Thickness, length), mat);
            cursor = startZ + length;
            return go;
        }

        static GameObject Crumble(string name, float width, float length, float gap)
        {
            float startZ = cursor + gap;
            float centerZ = startZ + length * 0.5f;
            var go = Cube(name, world, new Vector3(0f, -Thickness * 0.5f, centerZ), new Vector3(width, Thickness, length), deckC);

            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            go.AddComponent<FallingPlatform>();

            var touch = new GameObject("Touch");
            touch.transform.SetParent(go.transform, false);
            touch.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            touch.transform.localScale = new Vector3(1f, 1.3f, 1f);
            touch.AddComponent<BoxCollider>().isTrigger = true;
            touch.AddComponent<FallTrigger>();

            cursor = startZ + length;
            return go;
        }

        static GameObject PluginPrefab(string path, Transform parent, Vector3 localPos, Vector3 euler, Vector3 scale)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogError("[ParkourCastle] Missing prefab: " + path);
                return null;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            go.transform.localPosition = localPos;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = scale;
            return go;
        }

        static void FenceRow(float z, params float[] xs)
        {
            foreach (float x in xs)
                PluginPrefab(EnvPrefabs + "/Vault/Obstacle.prefab", world, new Vector3(x, 0.4f, z), Vector3.zero, new Vector3(3.19f, 0.82f, 0.23f));
        }

        static void SlideGate(float z, float width)
        {
            PluginPrefab(EnvPrefabs + "/Vault/Slide.prefab", world, new Vector3(0f, 1.45f, z), Vector3.zero, new Vector3(width, 0.9f, 1.5f));
        }

        static void DeepJumpBox(float z, float x = 0f)
        {
            PluginPrefab(EnvPrefabs + "/Vault/Box.prefab", world, new Vector3(x, 0.456f, z), Vector3.zero, new Vector3(1.96f, 0.99f, 0.66f));
        }

        static GameObject AddRigid(GameObject go)
        {
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            return go;
        }

        /* ---------- course layout ---------- */

        static void BuildCourse()
        {
            cursor = 0f;

            Deck("START", 14f, 14f, deckA, 0f, 0f);
            startMarker = new GameObject("START_POINT").transform;
            startMarker.SetParent(world, false);
            startMarker.position = new Vector3(0f, 0.15f, 2.5f);
            startMarker.rotation = Quaternion.identity;

            BuildStartGate();

            float v = cursor;
            Deck("STAGE_01_VAULT", 12f, 20f, deckB);
            FenceRow(v + 3.5f, -3.2f, 0f, 3.2f);
            FenceRow(v + 8.0f, -4.0f, 4.0f);
            FenceRow(v + 12.5f, -3.2f, 0f, 3.2f);
            AddCheckpoint(1, cursor, 11f);

            float s = cursor;
            Deck("STAGE_02_SLIDE", 12f, 20f, deckC);
            SlideGate(s + 4.5f, 6f);
            SlideGate(s + 9.5f, 8f);
            SlideGate(s + 14.5f, 5f);
            AddCheckpoint(2, cursor, 11f);

            float b = cursor;
            Deck("STAGE_03_BOULDERS", 12f, 20f, deckD);
            Boulder(b + 5f, 1.4f, 5f, 1.5f, 0f);
            Boulder(b + 10f, 1.4f, 6f, 1.65f, 1.7f);
            Boulder(b + 15f, 1.5f, 7f, 1.85f, 3.4f);

            float p = cursor;
            Deck("STAGE_04_PENDULUM", 14f, 20f, deckA);
            WreckingBall(p + 5f, 55f, 1.5f, 0f);
            WreckingBall(p + 10f, 60f, 1.65f, 1.0f);
            WreckingBall(p + 15f, 50f, 1.8f, 2.0f);
            AddCheckpoint(3, cursor, 13f);

            Crumble("CRUMBLE_1", 5f, 4f, Gap);
            Crumble("CRUMBLE_2", 5f, 4f, Gap);
            Crumble("CRUMBLE_3", 5f, 4f, Gap);
            Crumble("CRUMBLE_4", 5f, 4f, Gap);
            Deck("STAGE_05_LANDING", 12f, 10f, deckB);
            AddCheckpoint(4, cursor, 11f);

            float sp = cursor;
            Deck("STAGE_06_SPINNERS", 16f, 20f, deckC);
            Spinner(sp + 6f, 95f, -1f);
            Spinner(sp + 14f, 160f, 1f);

            float l = cursor;
            Deck("LEAP_1", 6f, 6f, deckD, 0f, 3f);
            DeepJumpBox(l + 2.5f);
            Deck("LEAP_2", 6f, 6f, deckA, 0f, 3f);
            Deck("LEAP_3", 6f, 6f, deckB, 0f, 3f);

            Deck("STAGE_07_FINISH", 16f, 16f, deckD);
            BuildFinish();
        }

        static void BuildStartGate()
        {
            Cube("StartPostL", world, new Vector3(-4f, 2.5f, 1f), new Vector3(0.4f, 5f, 0.4f), accent);
            Cube("StartPostR", world, new Vector3(4f, 2.5f, 1f), new Vector3(0.4f, 5f, 0.4f), accent);
            Cube("StartBeam", world, new Vector3(0f, 4.9f, 1f), new Vector3(8.4f, 0.4f, 0.4f), hazard);
            Cube("StartArrow", world, new Vector3(0f, 0.6f, 1f), new Vector3(2f, 0.15f, 2.5f), accent);
        }

        static void AddCheckpoint(int index, float deckEnd, float width)
        {
            float triggerZ = deckEnd - 1.6f;

            var root = new GameObject("CHECKPOINT_" + index);
            root.transform.SetParent(world, false);
            root.transform.position = new Vector3(0f, 0f, triggerZ);

            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(width, 4f, 1.4f);
            col.center = new Vector3(0f, 2f, 0f);

            var cp = root.AddComponent<Checkpoint>();
            cp.index = index;

            var spawn = new GameObject("Spawn").transform;
            spawn.SetParent(root.transform, false);
            spawn.localPosition = new Vector3(0f, 0.3f, -2.8f);
            spawn.localRotation = Quaternion.identity;
            cp.spawn = spawn;

            Cube("PostL", root.transform, new Vector3(-width * 0.5f + 0.3f, 2f, 0f), new Vector3(0.35f, 4f, 0.35f), checkpointGlow);
            Cube("PostR", root.transform, new Vector3(width * 0.5f - 0.3f, 2f, 0f), new Vector3(0.35f, 4f, 0.35f), checkpointGlow);
        }

        static void Boulder(float z, float diameter, float distance, float speed, float phase)
        {
            var go = Sphere2("Boulder", world, new Vector3(0f, diameter, z), diameter, hazard);
            var trap = go.AddComponent<RollingTrap>();
            trap.direction = Vector3.right;
            trap.distance = distance;
            trap.speed = speed;
            trap.phase = phase;
            trap.spinSpeed = 220f;
            go.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        static void WreckingBall(float z, float maxAngle, float speed, float phase)
        {
            var pivot = new GameObject("Pendulum" + z);
            pivot.transform.SetParent(world, false);
            pivot.transform.position = new Vector3(0f, 6f, z);
            AddRigid(pivot);

            Cylinder("Rope", pivot.transform, new Vector3(0f, -2.15f, 0f), new Vector3(0.12f, 4.3f, 0.12f), deckA);
            Sphere2("Ball", pivot.transform, new Vector3(0f, -4.3f, 0f), 2.6f, hazard);

            var trap = pivot.AddComponent<SwingingTrap>();
            trap.axis = Vector3.forward;
            trap.maxAngle = maxAngle;
            trap.speed = speed;
            trap.phase = phase;
        }

        static void Spinner(float z, float speed, float phase)
        {
            var pivot = new GameObject("Spinner" + z);
            pivot.transform.SetParent(world, false);
            pivot.transform.position = new Vector3(0f, 0.95f, z);
            pivot.transform.localScale = new Vector3(1f, 1f, 1f);
            AddRigid(pivot);

            Cylinder("Post", pivot.transform, new Vector3(0f, -0.7f, 0f), new Vector3(0.7f, 1.4f, 0.7f), deckB);
            Cylinder("Bar", pivot.transform, new Vector3(0f, 0.3f, 0f), new Vector3(15f, 0.5f, 0.5f), hazard);

            var trap = pivot.AddComponent<SpinBar>();
            trap.axis = Vector3.up;
            trap.speed = speed;
            pivot.transform.Rotate(0f, phase * 40f, 0f, Space.World);
        }

        static void BuildFinish()
        {
            float fz = cursor - 2.5f;

            Cube("FinishPad", world, new Vector3(0f, 0.1f, fz + 2f), new Vector3(12f, 0.2f, 12f), accent);

            var trigger = new GameObject("FINISH");
            trigger.transform.SetParent(world, false);
            trigger.transform.position = new Vector3(0f, 2f, fz);
            var col = trigger.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(12f, 4f, 1.6f);
            trigger.AddComponent<FinishLine>();

            Cube("FPostL", world, new Vector3(-6f, 3f, fz), new Vector3(0.5f, 6f, 0.5f), deckB);
            Cube("FPostR", world, new Vector3(6f, 3f, fz), new Vector3(0.5f, 6f, 0.5f), deckB);
            Cube("FBeam", world, new Vector3(0f, 5.8f, fz), new Vector3(12.6f, 0.6f, 0.6f), hazard);
            for (int i = 0; i < 11; i++)
                Cube("Check_" + i, world, new Vector3(-5.2f + i * 1.05f, 5.8f, fz), new Vector3(1.0f, 0.66f, 0.66f),
                    i % 2 == 0 ? hazard : new Material(hazard) { color = Color.white }, solid: false);
        }

        /* ---------- environment ---------- */

        static void BuildWater()
        {
            var waterGo = Cube("WATER", world, new Vector3(0f, -3.5f, 100f), new Vector3(400f, 0.5f, 500f), water);
            Object.DestroyImmediate(waterGo.GetComponent<Collider>());

            var kill = new GameObject("KILL_ZONE");
            kill.transform.SetParent(world, false);
            kill.transform.position = new Vector3(0f, -6f, 100f);
            var col = kill.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(220f, 4f, 520f);
            kill.AddComponent<KillZone>();
        }

        /* ---------- player, hud ---------- */

        static GameObject SpawnPlayer()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab);
            if (asset == null)
            {
                Debug.LogError("[ParkourCastle] Missing player prefab: " + PlayerPrefab);
                return null;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            go.name = "Player";
            go.transform.SetPositionAndRotation(startMarker.position, startMarker.rotation);
            go.transform.localScale = Vector3.one;
            go.SetActive(true);

            var cam = go.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Sky;
                cam.farClipPlane = 900f;
            }

            return go;
        }

        static void BuildHud(GameObject player)
        {
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            gm.startPoint = startMarker;

            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var font = GetFont();

            gm.timerText = MakeText(canvasGo.transform, "Timer", font, 92, new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(900f, 130f), TextAnchor.MiddleCenter, Color.white);
            gm.bestText = MakeText(canvasGo.transform, "Best", font, 42, new Vector2(0f, 1f), new Vector2(180f, -75f), new Vector2(360f, 80f), TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.55f, 1f));
            gm.checkpointText = MakeText(canvasGo.transform, "Checkpoint", font, 42, new Vector2(1f, 1f), new Vector2(-180f, -75f), new Vector2(360f, 80f), TextAnchor.MiddleCenter, new Color(0.55f, 1f, 0.85f, 1f));
            gm.messageText = MakeText(canvasGo.transform, "Message", font, 78, new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(1600f, 340f), TextAnchor.MiddleCenter, Color.white);
            gm.messageText.alignment = TextAnchor.MiddleCenter;
            gm.messageText.enabled = false;

            var hint = MakeText(canvasGo.transform, "Hint", font, 34, new Vector2(0f, 0f), new Vector2(40f, 28f), new Vector2(1400f, 62f), TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.92f));
            hint.text = "WASD MOVE   SHIFT RUN   SPACE JUMP-VAULT   C SLIDE   R RESTART";
        }

        static Text MakeText(Transform parent, string name, Font font, int size, Vector2 anchor, Vector2 pos, Vector2 sizeDelta, TextAnchor align, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var rt = text.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(4f, -4f);

            return text;
        }

        static Font GetFont()
        {
            try
            {
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                try { return Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch { return Font.CreateDynamicFontFromOSFont("Arial", 16); }
            }
        }

        /* ---------- materials & lighting ---------- */

        static void CreateMaterials()
        {
            deckA = MakeMaterial("Deck_Yellow", Yellow, 0.1f);
            deckB = MakeMaterial("Deck_Pink", Pink, 0.1f);
            deckC = MakeMaterial("Deck_Teal", Teal, 0.1f);
            deckD = MakeMaterial("Deck_Purple", Purple, 0.1f);
            hazard = MakeMaterial("Hazard_Red", Red, 0.35f);
            accent = MakeMaterial("Accent_Green", Green, 0.2f);
            water = MakeMaterial("Water_Blue", WaterDeep, 0.85f);
            checkpointGlow = MakeMaterial("Checkpoint_Mint", new Color(0.55f, 1f, 0.85f), 0.3f);
        }

        static Material MakeMaterial(string name, Color color, float smoothness)
        {
            string path = MatFolder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Diffuse");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = color;
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void SetupLighting()
        {
            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.97f, 0.9f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.52f, 0.6f, 0.72f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Sky;
            RenderSettings.fogStartDistance = 130f;
            RenderSettings.fogEndDistance = 360f;
        }

        static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
                AssetDatabase.CreateFolder(parent, name);
        }

        static void RegisterScene()
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int existing = list.FindIndex(s => s.path == ScenePath);
            if (existing >= 0) list[existing].enabled = true;
            else list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}