using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MainCourseEditor
{
    public static class CaptureShots
    {
        const string ShotDir = "Assets/ReportShots";

        [MenuItem("Speedrun/Capture Screenshots")]
        public static void Capture()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainCourse.unity", OpenSceneMode.Single);

            if (!Directory.Exists(ShotDir))
                Directory.CreateDirectory(ShotDir);

            var world = GameObject.Find("WORLD");

            TryShot("01_hero_wide", world, new Vector3(70f, 52f, 30f), new Vector3(0f, 6f, 200f), 55f);
            TryShot("02_start_gate", world, new Vector3(16f, 3.2f, -6f), new Vector3(0f, 2.6f, 24f), 55f);
            TryShot("03_stage1_jump", world, new Vector3(22f, 4.5f, 55f), new Vector3(0f, 3.4f, 80f), 50f);
            TryShot("04_beam_bridge", world, new Vector3(16f, 9f, 118f), new Vector3(0f, 6.5f, 150f), 50f);
            TryShot("05_lava_sea", world, new Vector3(34f, 5f, 300f), new Vector3(0f, -8f, 380f), 55f);
            TryShot("06_endgate", world, new Vector3(24f, 11f, 430f), new Vector3(0f, 3f, 470f), 50f);

            AssetDatabase.Refresh();
            Debug.Log("[CaptureShots] DONE -> " + ShotDir);
            EditorApplication.Exit(0);
        }

        static void TryShot(string name, GameObject world, Vector3 pos, Vector3 target, float fov)
        {
            var go = new GameObject("ShotCam");
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 900f;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.useOcclusionCulling = false;
            cam.renderingPath = RenderingPath.UsePlayerSettings;

            go.transform.SetPositionAndRotation(pos, Quaternion.LookRotation((target - pos).normalized));

            var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            rt.name = "ShotRT";
            var old = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = old;

            RenderTexture.active = rt;
            var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0f, 0f, 1920, 1080), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            File.WriteAllBytes(Path.Combine(ShotDir, name + ".png"), tex.EncodeToPNG());

            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(go);
            rt.Release();
        }
    }
}