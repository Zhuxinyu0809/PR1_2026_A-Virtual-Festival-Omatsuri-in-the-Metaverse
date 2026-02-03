using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal.ShaderGUI;

// ReSharper disable CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace AkilliMum.SRP
{
    public class PipelineGUIBase : BaseShaderGUI
    {
        static readonly string[] workflowModeNames = Enum.GetNames(typeof(LitGUI.WorkflowMode));

        private LitGUI.LitProperties litProperties;
        private LitDetailGUI.LitProperties litDetailProperties;

        private bool MenuBlendingZStates = true;
        private bool MenuUV = true;

        private bool UnityInternalOptions = true;

        public int DefaultSpace = 15;

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            Material targetMat = materialEditorIn.target as Material;

            EditorGUILayout.Space(DefaultSpace);

            #region BlendingZStates
            MenuBlendingZStates = EditorGUILayout.BeginFoldoutHeaderGroup(MenuBlendingZStates, new GUIContent { text = "Blend etc. Options" });

            if (MenuBlendingZStates)
            {
                EditorGUILayout.HelpBox("Contains the stencil, blending states, culling and Z options. Default values works fine but you can change them according to your needs!", MessageType.Info);

                try
                {
                    MaterialProperty _StencilRef = ShaderGUI.FindProperty("_StencilRef", properties);
                    materialEditorIn.ShaderProperty(_StencilRef, "Stencil Reference");

                }
                catch
                {
                    //ignore
                }

                try
                {
                    MaterialProperty _StencilComp = ShaderGUI.FindProperty("_StencilComp", properties);
                    materialEditorIn.ShaderProperty(_StencilComp, "Stencil Compare");

                }
                catch
                {
                    //ignore
                }

                MaterialProperty _SrcBlendEx = ShaderGUI.FindProperty("_SrcBlendEx", properties);
                materialEditorIn.ShaderProperty(_SrcBlendEx, "Source Blend");

                MaterialProperty _DstBlendEx = ShaderGUI.FindProperty("_DstBlendEx", properties);
                materialEditorIn.ShaderProperty(_DstBlendEx, "Destination Blend");

                EditorGUILayout.Space();

                MaterialProperty _ZTestEx = ShaderGUI.FindProperty("_ZTestEx", properties);
                materialEditorIn.ShaderProperty(_ZTestEx, "Z Test");

                MaterialProperty _CullEx = ShaderGUI.FindProperty("_CullEx", properties);
                materialEditorIn.ShaderProperty(_CullEx, "Cull");

                MaterialProperty _ZWriteEx = ShaderGUI.FindProperty("_ZWriteEx", properties);
                materialEditorIn.ShaderProperty(_ZWriteEx, "Z Write");
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            EditorGUILayout.Space(DefaultSpace);

            #region UV
            MenuUV = EditorGUILayout.BeginFoldoutHeaderGroup(MenuUV, new GUIContent { text = "UVs and Tiling-Offset Setups" });

            if (MenuUV)
            {
                EditorGUILayout.HelpBox("You can select which map should use which UV here. By default all maps uses UV0, but you can change them according to your needs", MessageType.Info);

                MaterialProperty _BaseMapUV = ShaderGUI.FindProperty("_BaseMapUV", properties);
                materialEditorIn.ShaderProperty(_BaseMapUV, "Base Map UV");

                MaterialProperty _SpecularUV = ShaderGUI.FindProperty("_SpecularUV", properties);
                materialEditorIn.ShaderProperty(_SpecularUV, "Specular Map UV");

                MaterialProperty _MetallicUV = ShaderGUI.FindProperty("_MetallicUV", properties);
                materialEditorIn.ShaderProperty(_MetallicUV, "Metallic Map UV");

                MaterialProperty _NormalUV = ShaderGUI.FindProperty("_NormalUV", properties);
                materialEditorIn.ShaderProperty(_NormalUV, "Normal Map UV");

                MaterialProperty _ParallaxUV = ShaderGUI.FindProperty("_ParallaxUV", properties);
                materialEditorIn.ShaderProperty(_ParallaxUV, "Height Map UV");

                MaterialProperty _OcclusionUV = ShaderGUI.FindProperty("_OcclusionUV", properties);
                materialEditorIn.ShaderProperty(_OcclusionUV, "Occlusion Map UV");

                MaterialProperty _ClearCoatUV = ShaderGUI.FindProperty("_ClearCoatUV", properties);
                materialEditorIn.ShaderProperty(_ClearCoatUV, "Clear Coat Map UV");

                MaterialProperty _EmissionUV = ShaderGUI.FindProperty("_EmissionUV", properties);
                materialEditorIn.ShaderProperty(_EmissionUV, "Emission Map UV");

                EditorGUILayout.HelpBox("You can select UV tilings-offsets here. First 2 are X,Y tilings and other 2 are X,Y offsets. By default all UVs share 1,1 tiling and 0,0 offset but you can change them according to your needs", MessageType.Info);

                MaterialProperty _UV0TileOffset = ShaderGUI.FindProperty("_UV0TileOffset", properties);
                materialEditorIn.ShaderProperty(_UV0TileOffset, "UV0 Tiling and Offset");

                MaterialProperty _UV1TileOffset = ShaderGUI.FindProperty("_UV1TileOffset", properties);
                materialEditorIn.ShaderProperty(_UV1TileOffset, "UV1 Tiling and Offset");

                MaterialProperty _UV2TileOffset = ShaderGUI.FindProperty("_UV2TileOffset", properties);
                materialEditorIn.ShaderProperty(_UV2TileOffset, "UV2 Tiling and Offset");

                MaterialProperty _UV3TileOffset = ShaderGUI.FindProperty("_UV3TileOffset", properties);
                materialEditorIn.ShaderProperty(_UV3TileOffset, "UV3 Tiling and Offset");
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            EditorGUILayout.Space(DefaultSpace);

            //#region UnityInternalOptions
            //UnityInternalOptions = EditorGUILayout.BeginFoldoutHeaderGroup(UnityInternalOptions, new GUIContent { text = "Unity's Internal Options" });

            //if (UnityInternalOptions)
            //{
            //    EditorGUILayout.HelpBox("We removed some Unity shader options (internal) to speed up the compile process. If you want you can open them here", MessageType.Info);

            //    {
            //        var value = ShaderGUI.FindProperty("O_REFLECTION_PROBE_BLENDING", properties);
            //        var boolean = value.floatValue > 0.5;
            //        boolean = EditorGUILayout.Toggle(new GUIContent { text = "Reflection Probe Blending" }, boolean);
            //        value.floatValue = boolean ? 1 : 0;
            //        if (boolean)
            //            targetMat?.EnableKeyword("_REFLECTION_PROBE_BLENDING");
            //        else
            //            targetMat?.DisableKeyword("_REFLECTION_PROBE_BLENDING");
            //    }

            //    {
            //        var value = ShaderGUI.FindProperty("O_REFLECTION_PROBE_BOX_PROJECTION", properties);
            //        var boolean = value.floatValue > 0.5;
            //        boolean = EditorGUILayout.Toggle(new GUIContent { text = "Reflection Probe Box Projection" }, boolean);
            //        value.floatValue = boolean ? 1 : 0;
            //        if (boolean)
            //            targetMat?.EnableKeyword("_REFLECTION_PROBE_BOX_PROJECTION");
            //        else
            //            targetMat?.DisableKeyword("_REFLECTION_PROBE_BOX_PROJECTION");
            //    }
                
            //    {
            //        var value = ShaderGUI.FindProperty("O_LIGHT_COOKIES", properties);
            //        var boolean = value.floatValue > 0.5;
            //        boolean = EditorGUILayout.Toggle(new GUIContent { text = "Light Cookies" }, boolean);
            //        value.floatValue = boolean ? 1 : 0;
            //        if (boolean)
            //            targetMat?.EnableKeyword("_LIGHT_COOKIES");
            //        else
            //            targetMat?.DisableKeyword("_LIGHT_COOKIES");
            //    }

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_WRITE_RENDERING_LAYERS", properties);
            //    //    var boolean = value.floatValue > 0.5;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Write Rendering Layers" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat?.EnableKeyword("_WRITE_RENDERING_LAYERS");
            //    //    else
            //    //        targetMat?.DisableKeyword("_WRITE_RENDERING_LAYERS");
            //    //}

            //    {
            //        var value = ShaderGUI.FindProperty("O_LOD_FADE_CROSSFADE", properties);
            //        var boolean = value.floatValue > 0.5;
            //        boolean = EditorGUILayout.Toggle(new GUIContent { text = "LOD Cross Fade" }, boolean);
            //        value.floatValue = boolean ? 1 : 0;
            //        if (boolean)
            //            targetMat?.EnableKeyword("LOD_FADE_CROSSFADE");
            //        else
            //            targetMat?.DisableKeyword("LOD_FADE_CROSSFADE");
            //    }

            //    {
            //        var value = ShaderGUI.FindProperty("O_DEBUG_DISPLAY", properties);
            //        var boolean = value.floatValue > 0.5;
            //        boolean = EditorGUILayout.Toggle(new GUIContent { text = "Debug Display" }, boolean);
            //        value.floatValue = boolean ? 1 : 0;
            //        if (boolean)
            //            targetMat?.EnableKeyword("DEBUG_DISPLAY");
            //        else
            //            targetMat?.DisableKeyword("DEBUG_DISPLAY");
            //    }

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_LIGHTMAP_ON", properties);
            //    //    var boolean = value.floatValue > 0.5 ? true : false;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Lightmaps" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat.EnableKeyword("LIGHTMAP_ON");
            //    //    else
            //    //        targetMat.DisableKeyword("LIGHTMAP_ON");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_DYNAMICLIGHTMAP_ON", properties);
            //    //    var boolean = value.floatValue > 0.5;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Dynamic Lightmaps" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat?.EnableKeyword("DYNAMICLIGHTMAP_ON");
            //    //    else
            //    //        targetMat?.DisableKeyword("DYNAMICLIGHTMAP_ON");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_LIGHT_LAYERS", properties);
            //    //    var boolean = value.floatValue > 0.5;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Light Layers" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat?.EnableKeyword("_LIGHT_LAYERS");
            //    //    else
            //    //        targetMat?.DisableKeyword("_LIGHT_LAYERS");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_LIGHTMAP_SHADOW_MIXING", properties);
            //    //    var boolean = value.floatValue > 0.5;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Mix Shadows and Lightmaps" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat?.EnableKeyword("LIGHTMAP_SHADOW_MIXING");
            //    //    else
            //    //        targetMat?.DisableKeyword("LIGHTMAP_SHADOW_MIXING");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_SHADOWS_SHADOWMASK", properties);
            //    //    var boolean = value.floatValue > 0.5;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Mix Shadows and Shadow Masks" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat?.EnableKeyword("SHADOWS_SHADOWMASK");
            //    //    else
            //    //        targetMat?.DisableKeyword("SHADOWS_SHADOWMASK");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_DIRLIGHTMAP_COMBINED", properties);
            //    //    var boolean = value.floatValue > 0.5;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Combine Directional Lightmaps" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat?.EnableKeyword("DIRLIGHTMAP_COMBINED");
            //    //    else
            //    //        targetMat?.DisableKeyword("DIRLIGHTMAP_COMBINED");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_DOTS_INSTANCING_ON", properties);
            //    //    var boolean = value.floatValue > 0.5 ? true : false;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "DOTS Instancing" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //    {
            //    //        targetMat.EnableKeyword("DOTS_INSTANCING_ON");
            //    //        //targetMat.EnableKeyword("UNITY_SUPPORT_INSTANCING"); //!!
            //    //    }
            //    //    else
            //    //        targetMat.DisableKeyword("DOTS_INSTANCING_ON");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_ADDITIONAL_LIGHT_SHADOWS", properties);
            //    //    var boolean = value.floatValue > 0.5 ? true : false;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Additional Light Shadows" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat.EnableKeyword("_ADDITIONAL_LIGHT_SHADOWS");
            //    //    else
            //    //        targetMat.DisableKeyword("_ADDITIONAL_LIGHT_SHADOWS");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_SHADOWS_SOFT", properties);
            //    //    var boolean = value.floatValue > 0.5;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Soft Shadows" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat?.EnableKeyword("_SHADOWS_SOFT");
            //    //    else
            //    //        targetMat?.DisableKeyword("_SHADOWS_SOFT");
            //    //}

            //    //{
            //    //    var value = ShaderGUI.FindProperty("O_EMISSION", properties);
            //    //    var boolean = value.floatValue > 0.5 ? true : false;
            //    //    boolean = EditorGUILayout.Toggle(new GUIContent { text = "Emission" }, boolean);
            //    //    value.floatValue = boolean ? 1 : 0;
            //    //    if (boolean)
            //    //        targetMat.EnableKeyword("_EMISSION");
            //    //    else
            //    //        targetMat.DisableKeyword("_EMISSION");
            //    //}
            //}

            //EditorGUILayout.EndFoldoutHeaderGroup();
            //#endregion

            //EditorGUILayout.Space(DefaultSpace);

            //call base!
            base.OnGUI(materialEditorIn, properties);
        }

#if UNITY_2022_1_OR_NEWER
        public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
        {
            materialScopesList.RegisterHeaderScope(LitDetailGUI.Styles.detailInputs, Expandable.Details, _ => LitDetailGUI.DoDetailArea(litDetailProperties, materialEditor));
        }
#else

        // collect properties from the material properties
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SetMaterialKeywords(material, LitGUI.SetMaterialKeywords);
        }
#endif
        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new LitGUI.LitProperties(properties);
            litDetailProperties = new LitDetailGUI.LitProperties(properties);
        }

#if UNITY_2022_1_OR_NEWER
        // material changed check
        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material, LitGUI.SetMaterialKeywords, LitDetailGUI.SetMaterialKeywords);
        }
#endif

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            if (litProperties.workflowMode != null)
                DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, workflowModeNames);

            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            LitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
            }

            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Blend", (float)blendMode);

            material.SetFloat("_Surface", (float)surfaceType);
            if (surfaceType == SurfaceType.Opaque)
            {
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
        }
    }

    internal class LitDetailGUI
    {
        public static class Styles
        {
            public static readonly GUIContent detailInputs = EditorGUIUtility.TrTextContent("Detail Inputs",
                "These settings define the surface details by tiling and overlaying additional maps on the surface.");

            public static readonly GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Mask",
                "Select a mask for the Detail map. The mask uses the alpha channel of the selected texture. The Tiling and Offset settings have no effect on the mask.");

            public static readonly GUIContent detailAlbedoMapText = EditorGUIUtility.TrTextContent("Base Map",
                "Select the surface detail texture.The alpha of your texture determines surface hue and intensity.");

            public static readonly GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Normal Map",
                "Designates a Normal Map to create the illusion of bumps and dents in the details of this Material's surface.");

            public static readonly GUIContent detailAlbedoMapScaleInfo = EditorGUIUtility.TrTextContent("Setting the scaling factor to a value other than 1 results in a less performant shader variant.");
        }

        public struct LitProperties
        {
            public MaterialProperty detailMask;
            public MaterialProperty detailAlbedoMapScale;
            public MaterialProperty detailAlbedoMap;
            public MaterialProperty detailNormalMapScale;
            public MaterialProperty detailNormalMap;

            public LitProperties(MaterialProperty[] properties)
            {
                detailMask = BaseShaderGUI.FindProperty("_DetailMask", properties, false);
                detailAlbedoMapScale = BaseShaderGUI.FindProperty("_DetailAlbedoMapScale", properties, false);
                detailAlbedoMap = BaseShaderGUI.FindProperty("_DetailAlbedoMap", properties, false);
                detailNormalMapScale = BaseShaderGUI.FindProperty("_DetailNormalMapScale", properties, false);
                detailNormalMap = BaseShaderGUI.FindProperty("_DetailNormalMap", properties, false);
            }
        }

        public static void DoDetailArea(LitProperties properties, MaterialEditor materialEditor)
        {
            try
            {
                materialEditor.TexturePropertySingleLine(Styles.detailMaskText, properties.detailMask);
                materialEditor.TexturePropertySingleLine(Styles.detailAlbedoMapText, properties.detailAlbedoMap,
                    properties.detailAlbedoMap.textureValue != null ? properties.detailAlbedoMapScale : null);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (properties.detailAlbedoMapScale.floatValue != 1.0f)
                {
                    EditorGUILayout.HelpBox(Styles.detailAlbedoMapScaleInfo.text, MessageType.Info, true);
                }

                materialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, properties.detailNormalMap,
                    properties.detailNormalMap.textureValue != null ? properties.detailNormalMapScale : null);
                materialEditor.TextureScaleOffsetProperty(properties.detailAlbedoMap);
            }
            catch
            {
                //ignore
            }
        }

        public static void SetMaterialKeywords(Material material)
        {
            if (material.HasProperty("_DetailAlbedoMap") && material.HasProperty("_DetailNormalMap") && material.HasProperty("_DetailAlbedoMapScale"))
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                bool isScaled = material.GetFloat("_DetailAlbedoMapScale") != 1.0f;
                bool hasDetailMap = material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap");
                CoreUtils.SetKeyword(material, "_DETAIL_MULX2", !isScaled && hasDetailMap);
                CoreUtils.SetKeyword(material, "_DETAIL_SCALED", isScaled && hasDetailMap);
            }
        }
    }

    //internal class SavedBool
    //{
    //    private bool m_Value;
    //    private string m_Name;

    //    public bool value
    //    {
    //        get
    //        {
    //            return this.m_Value;
    //        }
    //        set
    //        {
    //            if (this.m_Value == value)
    //                return;
    //            this.m_Value = value;
    //            EditorPrefs.SetBool(this.m_Name, value);
    //        }
    //    }

    //    public SavedBool(string name, bool value)
    //    {
    //        this.m_Name = name;
    //        this.m_Value = EditorPrefs.GetBool(name, value);
    //    }

    //    public static implicit operator bool(SavedBool s)
    //    {
    //        return s.value;
    //    }
    //}
}
