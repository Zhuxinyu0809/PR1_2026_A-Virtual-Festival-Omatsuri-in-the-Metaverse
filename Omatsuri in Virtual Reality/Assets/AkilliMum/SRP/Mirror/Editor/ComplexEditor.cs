using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal.ShaderGUI;
//using UnityEditor.Rendering.Universal.ShaderGUI;

namespace AkilliMum.SRP.Mirror
{
    internal class ComplexEditor : PipelineGUIBase
    {
        private bool MenuReflection = true;

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            Material targetMat = materialEditorIn.target as Material;

            #region Reflection
            {
                MenuReflection = EditorGUILayout.BeginFoldoutHeaderGroup(MenuReflection,
                    new GUIContent { text = "Reflection" });

                EditorGUILayout.HelpBox("Any reflection related options are here. Modify them according to your needs", MessageType.Info);

                MaterialProperty _FULLMIRROR =
                    ShaderGUI.FindProperty("_FULLMIRROR", properties);
                bool fullMirror = _FULLMIRROR.floatValue > 0.5f;

                MaterialProperty _UseFresnel =
                    ShaderGUI.FindProperty("_UseFresnel", properties);
                bool useFresnel = _UseFresnel.floatValue > 0.5f;

                MaterialProperty _EnableSimpleDepth =
                    ShaderGUI.FindProperty("_EnableSimpleDepth", properties);
                bool useDepth = _EnableSimpleDepth.floatValue > 0.5f;

                MaterialProperty _EnableDepthBlur =
                    ShaderGUI.FindProperty("_EnableDepthBlur", properties);
                MaterialProperty _MixBlackColor =
                    ShaderGUI.FindProperty("_MixBlackColor", properties);
                bool useDepthBlur = _EnableDepthBlur.floatValue > 0.5f;

                if (MenuReflection)
                {
                    //intensity
                    MaterialProperty _ReflectionIntensity = ShaderGUI.FindProperty("_ReflectionIntensity", properties);
                    materialEditorIn.ShaderProperty(_ReflectionIntensity, new GUIContent
                    {
                        text = "Intensity",
                        tooltip = "Intensity of reflection, like '0' = no reflection, '1' = full mirror"
                    });

                    //GI
                    fullMirror = EditorGUILayout.Toggle(new GUIContent
                    {
                        text = "Disable GI",
                        tooltip =
                            "Disables GI to create full mirror, so it does not affected by GI and creates perfect reflection"
                    }, fullMirror);
                    _FULLMIRROR.floatValue = fullMirror ? 1.0f : -1.0f;

                    //Fresnel Like Reflection
                    useFresnel = EditorGUILayout.Toggle(new GUIContent
                    {
                        text = "Fresnel Like Reflection",
                        tooltip =
                            "Uses fresnel like reflection, so far away objects (pixels) creates more reflectiveness"
                    }, useFresnel);
                    _UseFresnel.floatValue = useFresnel ? 1.0f : -1.0f;
                    //if (useFresnel)
                    //{
                    //    MaterialProperty _UseFresnelPower =
                    //        ShaderGUI.FindProperty("_UseFresnelPower", properties);
                    //    materialEditorIn.ShaderProperty(_UseFresnelPower, new GUIContent
                    //    {
                    //        text = "\tPower",
                    //        tooltip = "Power of the fresnel-ity"
                    //    });
                    //}

                    //mip level
                    MaterialProperty _LODLevel = ShaderGUI.FindProperty("_LODLevel", properties);
                    materialEditorIn.ShaderProperty(_LODLevel, new GUIContent
                    {
                        text = "Mip Level",
                        tooltip = "Mip level of the texture to be used. Warning: Mip Mapping must be enabled on MirrorManager script!"
                    });

                    //refraction
                    MaterialProperty _RefractionTex = ShaderGUI.FindProperty("_RefractionTex", properties);
                    materialEditorIn.TexturePropertySingleLine(
                        new GUIContent
                        {
                            text = "Refraction Map",
                            tooltip = "Refraction normal map to mimic refraction on reflection"
                        }, _RefractionTex);
                    //MaterialProperty _AKMU_MIRROR_REFRACTION = ShaderGUI.FindProperty("_AKMU_MIRROR_REFRACTION", properties);
                    if (_RefractionTex.textureValue != null)
                    {
                        //_AKMU_MIRROR_REFRACTION.floatValue = 1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_REFRACTION", true);

                        MaterialProperty _ReflectionRefraction =
                            ShaderGUI.FindProperty("_ReflectionRefraction", properties);
                        materialEditorIn.ShaderProperty(_ReflectionRefraction, new GUIContent
                        {
                            text = "\tIntensity",
                            tooltip = "Refraction intensity to refract more or less"
                        });
                    }
                    else
                    {
                        //_AKMU_MIRROR_REFRACTION.floatValue = 0;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_REFRACTION", false);
                    }

                    //wave
                    MaterialProperty _WaveNoiseTex = ShaderGUI.FindProperty("_WaveNoiseTex", properties);
                    materialEditorIn.TexturePropertySingleLine(
                        new GUIContent
                        {
                            text = "Wave Map",
                            tooltip = "Wave normal map to mimic waves on reflection"
                        }, _WaveNoiseTex);
                    //MaterialProperty _AKMU_MIRROR_WAVE = ShaderGUI.FindProperty("_AKMU_MIRROR_WAVE", properties);
                    if (_WaveNoiseTex.textureValue != null)
                    {
                        //_AKMU_MIRROR_WAVE.floatValue = 1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_WAVE", true);

                        MaterialProperty _WaveSize =
                            ShaderGUI.FindProperty("_WaveSize", properties);
                        materialEditorIn.ShaderProperty(_WaveSize, new GUIContent
                        {
                            text = "\tSize",
                            tooltip = "Size of the waves"
                        });

                        MaterialProperty _WaveDistortion =
                            ShaderGUI.FindProperty("_WaveDistortion", properties);
                        materialEditorIn.ShaderProperty(_WaveDistortion, new GUIContent
                        {
                            text = "\tDistortion",
                            tooltip = "Distortion amount of the waves"
                        });

                        MaterialProperty _WaveSpeed =
                            ShaderGUI.FindProperty("_WaveSpeed", properties);
                        materialEditorIn.ShaderProperty(_WaveSpeed, new GUIContent
                        {
                            text = "\tSpeed",
                            tooltip = "Speed of the waves according to the time"
                        });
                    }
                    else
                    {
                        //_AKMU_MIRROR_WAVE.floatValue = 0;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_WAVE", false);
                    }

                    //[HideInInspector]_EnableRipple("Enable Ripples", Float) = -1.0
                    //    [HideInInspector]_RippleTex("Ripple", 2D) = "bump" { }
                    //[HideInInspector]_RippleSize("Ripple Size", Float) = 2.0
                    //    [HideInInspector]_RippleRefraction("Ripple Refraction", Float) = 0.02
                    //    [HideInInspector]_RippleDensity("Ripple Density", Float) = 1.0
                    //    [HideInInspector]_RippleSpeed("Ripple Speed", Float) = 0.3
                    //ripple
                    MaterialProperty _RippleTex = ShaderGUI.FindProperty("_RippleTex", properties);
                    materialEditorIn.TexturePropertySingleLine(
                        new GUIContent
                        {
                            text = "Ripple Map",
                            tooltip = "Ripple normal map to mimic ripples on reflection"
                        }, _RippleTex);
                    //MaterialProperty _AKMU_MIRROR_RIPPLE = ShaderGUI.FindProperty("_AKMU_MIRROR_RIPPLE", properties);
                    if (_RippleTex.textureValue != null)
                    {
                        //_AKMU_MIRROR_RIPPLE.floatValue = 1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_RIPPLE", true);

                        MaterialProperty _RippleSize =
                            ShaderGUI.FindProperty("_RippleSize", properties);
                        materialEditorIn.ShaderProperty(_RippleSize, new GUIContent
                        {
                            text = "\tSize",
                            tooltip = "Size of the ripples"
                        });

                        MaterialProperty _RippleRefraction =
                            ShaderGUI.FindProperty("_RippleRefraction", properties);
                        materialEditorIn.ShaderProperty(_RippleRefraction, new GUIContent
                        {
                            text = "\tDistortion",
                            tooltip = "Distortion amount of the ripples"
                        });

                        MaterialProperty _RippleSpeed =
                            ShaderGUI.FindProperty("_RippleSpeed", properties);
                        materialEditorIn.ShaderProperty(_RippleSpeed, new GUIContent
                        {
                            text = "\tSpeed",
                            tooltip = "Speed of the ripples according to the time"
                        });

                        MaterialProperty _RippleDensity =
                            ShaderGUI.FindProperty("_RippleDensity", properties);
                        materialEditorIn.ShaderProperty(_RippleDensity, new GUIContent
                        {
                            text = "\tDensity",
                            tooltip = "Density of the ripples (hard-soft etc.)"
                        });
                    }
                    else
                    {
                        //_AKMU_MIRROR_RIPPLE.floatValue = 0;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_RIPPLE", false);
                    }

                    //depth
                    useDepth = EditorGUILayout.Toggle(new GUIContent
                    {
                        text = "Depth",
                        tooltip =
                            "Use reflection depth to mimic some fade-off on reflection. Warning: Depth must be enabled on MirrorManager script too!"
                    }, useDepth);
                    _EnableSimpleDepth.floatValue = useDepth ? 1.0f : -1.0f;
                    if (useDepth)
                    {
                        MaterialProperty _SimpleDepthCutoff =
                            ShaderGUI.FindProperty("_SimpleDepthCutoff", properties);
                        materialEditorIn.ShaderProperty(_SimpleDepthCutoff, new GUIContent
                        {
                            text = "\tCut-Off",
                            tooltip = "Depth cut-off value to set start-end reflection fade-off"
                        });
                    }

                    //depth blur
                    useDepthBlur = EditorGUILayout.Toggle(new GUIContent
                    {
                        text = "Depth Blur",
                        tooltip =
                            "Use advanced depth calculations to mimic some fade-off and blur on reflection. Warning: Depth Blur must be enabled on MirrorManager script too!"
                    }, useDepthBlur);
                    //_EnableDepthBlur.floatValue = _MixBlackColor.floatValue = useDepthBlur ? 1.0f : -1.0f;
                    _EnableDepthBlur.floatValue = useDepthBlur ? 1.0f : -1.0f;

                    //mask
                    MaterialProperty _MaskTex = ShaderGUI.FindProperty("_MaskTex", properties);
                    materialEditorIn.TexturePropertySingleLine(
                        new GUIContent
                        {
                            text = "Alpha Mask Map",
                            tooltip = "Alpha mask texture to create some transparent areas on reflection (like puddles)"
                        }, _MaskTex);
                    //MaterialProperty _AKMU_MIRROR_MASK = ShaderGUI.FindProperty("_AKMU_MIRROR_MASK", properties);
                    if (_MaskTex.textureValue != null)
                    {
                        //_AKMU_MIRROR_MASK.floatValue = 1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_MASK", true);

                        MaterialProperty _MaskCutoff =
                            ShaderGUI.FindProperty("_MaskCutoff", properties);
                        materialEditorIn.ShaderProperty(_MaskCutoff, new GUIContent
                        {
                            text = "\tCut-Off",
                            tooltip = "Cut-Off value to set start-end alpha fade-off"
                        });

                        MaterialProperty _MaskEdgeDarkness =
                            ShaderGUI.FindProperty("_MaskEdgeDarkness", properties);
                        materialEditorIn.ShaderProperty(_MaskEdgeDarkness, new GUIContent
                        {
                            text = "\tEdge Darkness",
                            tooltip = "To create darkness on edges of the mask to mimic intensity (like mud)"
                        });

                        MaterialProperty _MaskTiling =
                            ShaderGUI.FindProperty("_MaskTiling", properties);
                        materialEditorIn.ShaderProperty(_MaskTiling, new GUIContent
                        {
                            text = "\tTiling",
                            tooltip = "Tiling of the mask texture to fit on object as you want"
                        });
                    }
                    else
                    {
                        //_AKMU_MIRROR_MASK.floatValue = 0;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_MASK", false);
                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                //enable disable GI with keyword
                if (fullMirror)
                {
                    targetMat.EnableKeyword("_AKMU_MIRROR_FULL");
                }
                else
                {
                    targetMat.DisableKeyword("_AKMU_MIRROR_FULL");
                }
            }

            #endregion

            //call base!
            base.OnGUI(materialEditorIn, properties);
        }
    }
}

//#region Locally Correction

//{
//    MaterialProperty _EnableLocallyCorrection =
//        ShaderGUI.FindProperty("_EnableLocallyCorrection", properties);

//    MenuLocallyCorrection = EditorGUILayout.BeginFoldoutHeaderGroup(MenuLocallyCorrection,
//        new GUIContent { text = "Locally Correction" });

//    bool enableLCRS = false;
//    if (MenuLocallyCorrection)
//    {
//        enableLCRS = _EnableLocallyCorrection.floatValue > 0.5f;
//        enableLCRS = EditorGUILayout.Toggle("Enable", enableLCRS);
//        _EnableLocallyCorrection.floatValue = enableLCRS ? 1.0f : -1.0f;

//        //if (realTimeRef)
//        //{
//        //    MaterialProperty _EnviCubeIntensity = ShaderGUI.FindProperty("_EnviCubeIntensity", properties);
//        //    materialEditorIn.ShaderProperty(_EnviCubeIntensity, "Intensity");

//        //    MaterialProperty _EnviCubeSmoothness = ShaderGUI.FindProperty("_EnviCubeSmoothness", properties);
//        //    materialEditorIn.ShaderProperty(_EnviCubeSmoothness, "Smoothness");
//        //}
//    }

//    EditorGUILayout.EndFoldoutHeaderGroup();

//    if (enableLCRS)
//    {
//        targetMat.EnableKeyword("_LOCALLYCORRECTION");
//        //Debug.Log("Enabled LCRS");
//    }
//    else
//    {
//        targetMat.DisableKeyword("_LOCALLYCORRECTION");
//        //Debug.Log("Disabled LCRS");
//    }
//}

//#endregion