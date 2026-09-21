using MainCourse;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MainCourseEditor
{
    public static class HUDBuilder
    {
        const string PillTexPath = "Assets/Materials/MainCourse/UI_PillTex.asset";
        static Sprite pillSprite;

        [MenuItem("Speedrun/Generate HUD")]
        public static void GenerateHud()
        {
            Build();
        }

        public static void Build()
        {
            var font = GetFont();

            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var hud = canvasGo.AddComponent<HUDController>();
            var root = canvasGo.transform;

            // ---------- Top-left: run stats ----------
            var pillL = MakePill(root, "Panel_Left", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(254f, -130f), new Vector2(460f, 212f), new Color(0.035f, 0.045f, 0.10f, 0.68f));
            MakeLabel(pillL, "Label_Deaths", font, "DEATHS", 20, new Vector2(0f, 62f));
            hud.deathsText = MakeText(pillL, "Deaths", font, 34, new Vector2(0f, 26f), new Vector2(430f, 52f), new Color(1f, 0.48f, 0.46f, 1f));
            hud.deathsText.text = "0";
            MakeLabel(pillL, "Label_Gems", font, "GEMS", 20, new Vector2(0f, -34f));
            hud.gemText = MakeText(pillL, "Gems", font, 34, new Vector2(0f, -70f), new Vector2(430f, 52f), new Color(1f, 0.86f, 0.45f, 1f));
            hud.gemText.text = "0 / 13";

            // ---------- Top-center: timer ----------
            var pillT = MakePill(root, "Panel_Timer", new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -124f), new Vector2(680f, 200f), new Color(0.035f, 0.045f, 0.10f, 0.6f));
            hud.timerText = MakeText(pillT, "Timer", font, 92, new Vector2(0f, 16f), new Vector2(660f, 110f), new Color(1f, 0.85f, 0.45f, 1f));
            hud.timerText.text = "00:00.00";
            hud.bestText = MakeText(pillT, "Best", font, 26, new Vector2(0f, -56f), new Vector2(660f, 36f), new Color(0.75f, 0.82f, 1f, 0.95f));
            hud.bestText.text = "BEST  --:--.----";

            // ---------- Top-right: checkpoint ----------
            var pillR = MakePill(root, "Panel_Checkpoint", new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-254f, -124f), new Vector2(460f, 200f), new Color(0.035f, 0.045f, 0.10f, 0.6f));
            MakeLabel(pillR, "Label_Checkpoint", font, "CHECKPOINT", 22, new Vector2(0f, 44f));
            hud.checkpointText = MakeText(pillR, "Checkpoint", font, 48, new Vector2(0f, -6f), new Vector2(440f, 80f), new Color(0.55f, 1f, 0.88f, 1f));
            hud.checkpointText.text = "CP  0 / 11";

            // ---------- Bottom-left: dash ----------
            var pillD = MakePill(root, "Panel_Dash", new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(252f, 112f), new Vector2(416f, 60f), new Color(0.035f, 0.045f, 0.10f, 0.68f));
            hud.dashText = MakeText(pillD, "Dash", font, 28, new Vector2(0f, 0f), new Vector2(396f, 46f), new Color(0.45f, 1f, 0.92f, 1f));
            hud.dashText.text = "DASH  READY";

            // ---------- Bottom-center: hint ----------
            hud.hintText = MakeText(root, "Hint", font, 24, new Vector2(0f, 20f), new Vector2(1500f, 40f), new Color(1f, 1f, 1f, 0.85f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            hud.hintText.text = "WASD MOVE   SPACE JUMP   SHIFT TAP = DASH   SHIFT HOLD = SPRINT   CTRL = SLIDE   R RESTART";

            BuildVictoryPanel(root, font, hud);

            BuildPausePanel(root, font, hud);

            if (GameManager.Instance != null)
                Debug.Log("[MainCourse] HUD_BUILT wired=" + GameManager.Instance.name);
            else
                Debug.Log("[MainCourse] HUD_BUILT (GameManager not in scene)");
        }

        static void BuildVictoryPanel(Transform root, Font font, HUDController hud)
        {
            var panel = new GameObject("VictoryPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = panel.GetComponent<Image>();
            bg.color = new Color(0.035f, 0.045f, 0.10f, 0.88f);
            bg.raycastTarget = false;

            hud.victoryPanel = panel;

            var pill = MakePill(panel.transform, "V_Pill", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(900f, 600f), new Color(0.02f, 0.03f, 0.08f, 0.6f));

            hud.victoryTitleText = MakeText(pill, "VictoryTitle", font, 96, new Vector2(0f, 200f), new Vector2(860f, 140f), new Color(1f, 0.85f, 0.35f, 1f));
            hud.victoryTitleText.text = "COURSE CLEARED";

            hud.victoryTimeText = MakeText(pill, "VictoryTime", font, 62, new Vector2(0f, 72f), new Vector2(860f, 110f), new Color(0.55f, 1f, 0.88f, 1f));
            hud.victoryTimeText.text = "TIME  00:00.00";

            hud.victoryDeathsText = MakeText(pill, "VictoryStats", font, 40, new Vector2(0f, -78f), new Vector2(820f, 190f), new Color(0.80f, 0.86f, 1f, 1f));
            hud.victoryDeathsText.text = "DEATHS  0\nGEMS  0 / 13\nBEST  --:--.----";

            var again = MakeText(pill, "VictoryHint", font, 34, new Vector2(0f, -230f), new Vector2(860f, 60f), new Color(1f, 1f, 1f, 0.7f));
            again.text = "PRESS  R  TO  RUN  AGAIN";

            panel.SetActive(false);
        }

        static void BuildPausePanel(Transform root, Font font, HUDController hud)
        {
            var panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var dim = panel.GetComponent<Image>();
            dim.color = new Color(0.02f, 0.03f, 0.06f, 0.78f);
            dim.raycastTarget = false;

            hud.pausePanel = panel;

            var pill = MakePill(panel.transform, "P_Pill", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(860f, 480f), new Color(0.02f, 0.03f, 0.08f, 0.85f));

            var title = MakeText(pill, "P_Title", font, 110, new Vector2(0f, 150f), new Vector2(820f, 140f), new Color(1f, 0.85f, 0.35f, 1f));
            title.text = "PAUSED";

            MakeText(pill, "P_Resume", font, 40, new Vector2(0f, 10f), new Vector2(820f, 60f), new Color(0.55f, 1f, 0.88f, 1f)).text = "ESC  -  RESUME";
            MakeText(pill, "P_Restart", font, 40, new Vector2(0f, -70f), new Vector2(820f, 60f), new Color(0.80f, 0.86f, 1f, 1f)).text = "R  -  RESTART";
            MakeText(pill, "P_Quit", font, 40, new Vector2(0f, -150f), new Vector2(820f, 60f), new Color(1f, 0.6f, 0.6f, 1f)).text = "Q  -  QUIT";

            panel.SetActive(false);
        }

        static void MakeLabel(Transform parent, string name, Font font, string text, float size, Vector2 pos)
        {
            var t = MakeText(parent, name, font, size, pos, new Vector2(380f, 30f), new Color(0.72f, 0.76f, 0.92f, 0.9f));
            t.text = text;
        }

        static RectTransform MakePill(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            var spr = GetPillSprite();
            if (spr != null)
            {
                img.sprite = spr;
                img.type = Image.Type.Sliced;
                img.fillCenter = true;
            }
            img.color = color;

            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor;
            r.anchorMax = anchor;
            r.pivot = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            return r;
        }

        static Text MakeText(Transform parent, string name, Font font, float size,
            Vector2 pos, Vector2 sizeDelta, Color color,
            Vector2? anchor = null, Vector2? pivot = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = Mathf.RoundToInt(size);
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = false;
            t.raycastTarget = false;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            var r = t.rectTransform;
            r.anchorMin = anchor ?? new Vector2(0.5f, 0.5f);
            r.anchorMax = anchor ?? new Vector2(0.5f, 0.5f);
            r.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            r.anchoredPosition = pos;
            r.sizeDelta = sizeDelta;

            return t;
        }

        static Font GetFont()
        {
            var font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont("Segoe UI", 32);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        static Sprite GetPillSprite()
        {
            if (pillSprite != null) return pillSprite;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(PillTexPath);
            if (tex == null)
            {
                const int S = 128;
                const int r = 18;
                tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                float half = (S - 1) * 0.5f;
                for (int y = 0; y < S; y++)
                {
                    for (int x = 0; x < S; x++)
                    {
                        float px = Mathf.Abs(x - half);
                        float py = Mathf.Abs(y - half);
                        float b = S * 0.5f;
                        float qx = Mathf.Max(px - (b - r), 0f);
                        float qy = Mathf.Max(py - (b - r), 0f);
                        float sd = Mathf.Sqrt(qx * qx + qy * qy) - r;
                        float a = Mathf.Clamp01(1f - Mathf.Max(sd, 0f) / 12f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                }
                tex.Apply();
                AssetDatabase.CreateAsset(tex, PillTexPath);
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PillTexPath);
            if (sprite == null)
            {
                sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(30f, 30f, 30f, 30f));
                AssetDatabase.AddObjectToAsset(sprite, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(PillTexPath));
                AssetDatabase.SaveAssets();
            }

            pillSprite = sprite;
            return sprite;
        }
    }
}