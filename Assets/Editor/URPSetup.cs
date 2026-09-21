using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MainCourseEditor
{
    public static class URPSetup
    {
        const string PipelinePath = "Assets/Settings/URP_Pipeline.asset";
        const string RendererPath = "Assets/Settings/URP_Renderer.asset";
        const string ProfilePath = "Assets/Settings/MainCourse_Volume.asset";

        public const string MatFolder = "Assets/Materials/MainCourse";
        public const string MatPrefix = "MC_";
        public const string TexFolder = "Assets/Textures/MainCourse";

        public static UniversalRenderPipelineAsset EnsureUrp()
        {
            EnsureFolder("Assets", "Settings");

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            // Persist + import both assets first so the renderer has a real fileID before we write the reference.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (rendererData == null || pipeline == null)
                throw new System.Exception("[MainCourse] Failed to create URP assets under Assets/Settings");

            var so = new SerializedObject(pipeline);
            var list = so.FindProperty("m_RendererDataList");
            if (list == null)
                throw new System.Exception("[MainCourse] URP asset missing m_RendererDataList; wrong URP version");
            if (list.arraySize == 0) list.arraySize = 1;
            list.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
            var def = so.FindProperty("m_DefaultRendererIndex");
            if (def != null) def.intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var verify = new SerializedObject(AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath));
            var refProp = verify.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0);
            if (refProp.objectReferenceValue == null)
                throw new System.Exception("[MainCourse] URP renderer reference did not persist into " + PipelinePath);

            if (GraphicsSettings.defaultRenderPipeline != pipeline)
                GraphicsSettings.defaultRenderPipeline = pipeline;

            if (QualitySettings.renderPipeline != pipeline)
                QualitySettings.renderPipeline = pipeline;

            try
            {
                var so2 = new SerializedObject(pipeline);
                var msaa = so2.FindProperty("m_MSAA");
                if (msaa != null) msaa.intValue = 8;
                so2.ApplyModifiedPropertiesWithoutUndo();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[MainCourse] MSAA set failed: " + ex.Message);
            }

            if (PlayerSettings.colorSpace != ColorSpace.Linear)
                PlayerSettings.colorSpace = ColorSpace.Linear;

            PlayerSettings.productName = "Parkour Game";

            AssetDatabase.SaveAssets();
            return pipeline;
        }

        public static VolumeProfile GetOrCreateVolumeProfile()
        {
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (existing != null) return existing;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.55f);
            bloom.scatter.Override(0.65f);
            bloom.tint.Override(Color.white);

            var colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.postExposure.Override(0.12f);
            colorAdj.contrast.Override(6f);
            colorAdj.saturation.Override(8f);

            var tonemap = profile.Add<Tonemapping>(true);
            tonemap.mode.Override(TonemappingMode.ACES);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.26f);
            vignette.smoothness.Override(0.4f);

            var ca = profile.Add<ChromaticAberration>(true);
            ca.intensity.Override(0f);

            var ld = profile.Add<LensDistortion>(true);
            ld.intensity.Override(0f);

            AssetDatabase.CreateAsset(profile, ProfilePath);
            return profile;
        }

        public static Material MakeMaterial(string name, Color color, float smoothness, Color emission = default)
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "MainCourse");
            string path = MatFolder + "/" + MatPrefix + name + ".mat";

            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            if (emission.a != 0f && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
                if (mat.HasProperty("_EmissiveColor")) mat.SetColor("_EmissiveColor", emission);
                if (mat.HasProperty("_EmissiveColorLDR")) mat.SetColor("_EmissiveColorLDR", emission);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material SaveMaterialAsset(Material mat, string name)
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "MainCourse");
            string path = MatFolder + "/" + MatPrefix + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;
            AssetDatabase.CreateAsset(mat, path);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateShaderMaterial(string name, string shaderName)
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "MainCourse");
            string path = MatFolder + "/" + MatPrefix + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError("[URPSetup] Shader not found: " + shaderName);
                return null;
            }

            var mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static void ConfigureTextureImporters()
        {
            EnsureFolder("Assets", "Textures");
            EnsureFolder("Assets/Textures", "MainCourse");
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TexFolder });
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;
                bool isNormal = path.Contains("_nor_gl_");
                if (ti.textureType != (isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default) ||
                    ti.wrapMode != TextureWrapMode.Repeat ||
                    ti.sRGBTexture != !isNormal)
                {
                    ti.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                    ti.sRGBTexture = !isNormal;
                    ti.wrapMode = TextureWrapMode.Repeat;
                    ti.mipmapEnabled = true;
                    ti.isReadable = false;
                    EditorUtility.SetDirty(ti);
                    ti.SaveAndReimport();
                }
            }

            string hdrPath = TexFolder + "/kloofendal_48d_partly_cloudy_puresky_2k.hdr";
            var hdrTi = AssetImporter.GetAtPath(hdrPath) as TextureImporter;
            if (hdrTi != null)
            {
                hdrTi.textureType = TextureImporterType.Default;
                hdrTi.sRGBTexture = false;
                hdrTi.wrapMode = TextureWrapMode.Clamp;
                hdrTi.mipmapEnabled = true;
                EditorUtility.SetDirty(hdrTi);
                hdrTi.SaveAndReimport();
            }
        }

        public static Texture2D LoadTex(string file)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(TexFolder + "/" + file);
        }

        public static Material MakeTexturedMaterial(string name, string baseFile, string normalFile, Color tint, float smoothness, float metallic = 0.02f, Vector2 tiling = default)
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "MainCourse");
            string path = MatFolder + "/" + MatPrefix + name + ".mat";

            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            var baseTex = LoadTex(baseFile);
            var normTex = LoadTex(normalFile);
            if (baseTex != null)
            {
                mat.SetTexture("_BaseMap", baseTex);
                mat.SetTexture("_MainTex", baseTex);
                mat.EnableKeyword("_BASE_MAP");
            }
            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_Color", tint);
            if (normTex != null)
            {
                mat.SetTexture("_BumpMap", normTex);
                mat.EnableKeyword("_NORMALMAP");
                mat.SetFloat("_BumpScale", 1f);
            }
            if (tiling != default && baseTex != null)
            {
                mat.SetTextureScale("_BaseMap", tiling);
                mat.SetTextureScale("_MainTex", tiling);
                mat.SetTextureScale("_BumpMap", tiling);
            }
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_SmoothnessTextureChannel", 0f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateSkyMaterial()
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "MainCourse");
            string path = MatFolder + "/" + MatPrefix + "Skybox.mat";

            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Skybox/Panoramic");
            if (shader == null) shader = Shader.Find("Skybox/Procedural");
            bool created = false;
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
                created = true;
            }
            else if (mat.shader == null || mat.shader.name != "Skybox/Panoramic")
            {
                mat.shader = shader;
                EditorUtility.SetDirty(mat);
            }

            var tex = LoadTex("kloofendal_48d_partly_cloudy_puresky_2k.hdr");
            if (tex != null)
            {
                mat.SetTexture("_Tex", tex);
                mat.SetTexture("_MainTex", tex);
            }
            mat.SetFloat("_Mapping", 0f);
            mat.SetFloat("_ImageType", 0f);
            mat.SetFloat("_Layout", 0f);
            mat.SetFloat("_Exposure", 1.0f);
            mat.SetFloat("_Rotation", 0f);
            EditorUtility.SetDirty(mat);
            if (created) AssetDatabase.SaveAssets();
            return mat;
        }

        public static void EnsureFolder(string parent, string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            string full = parent + "/" + name;
            if (AssetDatabase.IsValidFolder(full)) return;
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent.Substring(0, parent.LastIndexOf('/')), parent.Substring(parent.LastIndexOf('/') + 1));
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}