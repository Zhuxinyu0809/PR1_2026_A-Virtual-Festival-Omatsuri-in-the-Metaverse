using UnityEditor;
using UnityEngine;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace AkilliMum.SRP.Mirror
{
    [CustomEditor(typeof(MirrorManager))]
    [CanEditMultipleObjects]
    public class MirrorManagerEditor : Editor
    {
        bool MenuCommon = true;
        bool MenuCamera = true;

        SerializedProperty IsDebug;
        
        SerializedProperty IsEnabled;
        SerializedProperty Name;
        SerializedProperty IsMirrorInMirror;
        SerializedProperty MirrorInMirrorId;
        SerializedProperty TurnOffOcclusion;

        SerializedProperty Platform;
        SerializedProperty ForceMultiPass;

        SerializedProperty WorkingType;
        SerializedProperty ClearBackGround;
        SerializedProperty ClearColor;
        SerializedProperty UpVector;
        SerializedProperty CameraLODLevel;
        //SerializedProperty TextureLODLevel;

        SerializedProperty UseClipping;
        SerializedProperty ClipUV;
        SerializedProperty ClipEye;
        //SerializedProperty ClipMultiplier;
        SerializedProperty ClippingPercentage;
        SerializedProperty HDR;
        SerializedProperty FOV;
        SerializedProperty MSAALevel;
        SerializedProperty AntialiasingMode;
        SerializedProperty AntialiasingQuality;
        SerializedProperty RenderPostProcessing;
        SerializedProperty RequiresOpaqueTexture;
        SerializedProperty RequiresDepthTexture;
        SerializedProperty FilterMode;
        SerializedProperty MipMapping;
        SerializedProperty DisablePixelLights;
        SerializedProperty Shadow;
        SerializedProperty Cull;
        SerializedProperty CullDistance;
        SerializedProperty RenderTextureSize;
        SerializedProperty ManualSize;
        SerializedProperty ClipPlaneOffset;
        SerializedProperty ReflectLayers;

        SerializedProperty _cameraAttached;
        SerializedProperty ReflectiveObjects;
        SerializedProperty DrawAlways;
        SerializedProperty FakeCamera;

        SerializedProperty EnableDepth;

        SerializedProperty EnableDepthBlur;
        SerializedProperty DepthBlurShader;
        SerializedProperty DepthBlurCutoff;
        SerializedProperty DepthBlurIterations;
        SerializedProperty DepthBlurSurfacePower;
        SerializedProperty DepthBlurHorizontalMultiplier;
        SerializedProperty DepthBlurVerticalMultiplier;

        SerializedProperty EnableSimpleBlur;
        SerializedProperty SimpleBlurShader;
        SerializedProperty SimpleBlurIterations;
        SerializedProperty SimpleBlurPixels;

        SerializedProperty CustomShaders;

        SerializedProperty UIImage;
        SerializedProperty UIImageDepth;

        private int DefaultSpace = 15;
        //string space = "          ";
        GUIStyle headerStyle;

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            headerStyle = new GUIStyle { fontStyle = FontStyle.Bold };

            IsDebug = serializedObject.FindProperty("IsDebug");
            UIImage = serializedObject.FindProperty("UIImage");
            UIImageDepth = serializedObject.FindProperty("UIImageDepth");

            IsEnabled = serializedObject.FindProperty("IsEnabled");
            Name = serializedObject.FindProperty("Name");
            IsMirrorInMirror = serializedObject.FindProperty("IsMirrorInMirror");
            MirrorInMirrorId = serializedObject.FindProperty("MirrorInMirrorId");
            TurnOffOcclusion = serializedObject.FindProperty("TurnOffOcclusion");

            Platform = serializedObject.FindProperty("Platform");
            ForceMultiPass = serializedObject.FindProperty("ForceMultiPass");

            WorkingType = serializedObject.FindProperty("WorkingType");
            ClearBackGround = serializedObject.FindProperty("ClearBackGround");
            ClearColor = serializedObject.FindProperty("ClearColor");
            UpVector = serializedObject.FindProperty("UpVector");
            CameraLODLevel = serializedObject.FindProperty("CameraLODLevel");
            //TextureLODLevel = serializedObject.FindProperty("TextureLODLevel");

            UseClipping = serializedObject.FindProperty("UseClipping");
            ClipUV = serializedObject.FindProperty("ClipUV");
            ClipEye = serializedObject.FindProperty("ClipEye");
            //ClipMultiplier = serializedObject.FindProperty("ClipMultiplier");
            ClippingPercentage = serializedObject.FindProperty("ClippingPercentage");
            HDR = serializedObject.FindProperty("HDR");
            FOV = serializedObject.FindProperty("FOV");
            MSAALevel = serializedObject.FindProperty("MSAALevel");
            AntialiasingMode = serializedObject.FindProperty("AntialiasingMode");
            AntialiasingQuality = serializedObject.FindProperty("AntialiasingQuality");
            RenderPostProcessing = serializedObject.FindProperty("RenderPostProcessing");
            RequiresOpaqueTexture = serializedObject.FindProperty("RequiresOpaqueTexture");
            RequiresDepthTexture = serializedObject.FindProperty("RequiresDepthTexture");
            FilterMode = serializedObject.FindProperty("FilterMode");
            MipMapping = serializedObject.FindProperty("MipMapping");
            DisablePixelLights = serializedObject.FindProperty("DisablePixelLights");
            Shadow = serializedObject.FindProperty("Shadow");
            Cull = serializedObject.FindProperty("Cull");
            CullDistance = serializedObject.FindProperty("CullDistance");
            RenderTextureSize = serializedObject.FindProperty("RenderTextureSize");
            ManualSize = serializedObject.FindProperty("ManualSize");
            ClipPlaneOffset = serializedObject.FindProperty("ClipPlaneOffset");
            ReflectLayers = serializedObject.FindProperty("ReflectLayers");

            _cameraAttached = serializedObject.FindProperty("_cameraAttached");
            ReflectiveObjects = serializedObject.FindProperty("ReflectiveObjects");
            DrawAlways = serializedObject.FindProperty("DrawAlways");
            FakeCamera = serializedObject.FindProperty("FakeCamera");

            EnableDepth = serializedObject.FindProperty("EnableDepth");

            EnableDepthBlur = serializedObject.FindProperty("EnableDepthBlur");
            DepthBlurShader = serializedObject.FindProperty("DepthBlurShader");
            DepthBlurCutoff = serializedObject.FindProperty("DepthBlurCutoff");
            DepthBlurIterations = serializedObject.FindProperty("DepthBlurIterations");
            DepthBlurSurfacePower = serializedObject.FindProperty("DepthBlurSurfacePower");
            DepthBlurHorizontalMultiplier = serializedObject.FindProperty("DepthBlurHorizontalMultiplier");
            DepthBlurVerticalMultiplier = serializedObject.FindProperty("DepthBlurVerticalMultiplier");

            EnableSimpleBlur = serializedObject.FindProperty("EnableSimpleBlur");
            SimpleBlurShader = serializedObject.FindProperty("SimpleBlurShader");
            SimpleBlurIterations = serializedObject.FindProperty("SimpleBlurIterations");
            SimpleBlurPixels = serializedObject.FindProperty("SimpleBlurPixels");

            CustomShaders = serializedObject.FindProperty("CustomShaders");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(IsDebug);
            
            EditorGUILayout.PropertyField(IsEnabled);
            EditorGUILayout.PropertyField(Name);
            EditorGUILayout.PropertyField(TurnOffOcclusion);
            EditorGUILayout.PropertyField(IsMirrorInMirror);
            if (IsMirrorInMirror.boolValue)
            {
                EditorGUILayout.PropertyField(MirrorInMirrorId, new GUIContent { text = "\tId" });
            }



            EditorGUILayout.Space(DefaultSpace);
            EditorGUILayout.LabelField(new GUIContent { text = "Platform Type (Stand-Alone, VR, AR)" }, headerStyle);
            EditorGUILayout.PropertyField(Platform);
            if (Platform.intValue == (int)Mirror.Platform.VR)
            {
                EditorGUILayout.PropertyField(ForceMultiPass, new GUIContent { text = "\tForce MultiPass" });
            }


            EditorGUILayout.Space(DefaultSpace);
            //EditorGUILayout.LabelField(new GUIContent { text = "Reflective Objects" }, headerStyle);
            EditorGUILayout.PropertyField(_cameraAttached, new GUIContent { text = "Main Camera" });

            EditorGUILayout.PropertyField(ReflectiveObjects);
            EditorGUILayout.PropertyField(DrawAlways, new GUIContent { text = "\tAlways Render Reflection" });
            EditorGUILayout.PropertyField(FakeCamera, new GUIContent { text = "\tFake Camera Object" });



            EditorGUILayout.Space(DefaultSpace);
            MenuCommon = EditorGUILayout.BeginFoldoutHeaderGroup(MenuCommon, new GUIContent { text = "Common" });
            if (MenuCommon)
            {
                EditorGUILayout.PropertyField(WorkingType);
                EditorGUILayout.PropertyField(ClearBackGround);
                if (ClearBackGround.intValue == (int)Mirror.ClearBackGround.Color)
                {
                    EditorGUILayout.PropertyField(ClearColor, new GUIContent { text = "\tClear Color" });
                }
                EditorGUILayout.PropertyField(UpVector);
                //EditorGUILayout.PropertyField(TextureLODLevel);
                EditorGUILayout.PropertyField(EnableDepth);

                EditorGUILayout.PropertyField(EnableDepthBlur, new GUIContent { text = "Enable Depth Blur" });
                if (EnableDepthBlur.boolValue)
                {
                    EditorGUILayout.PropertyField(DepthBlurShader, new GUIContent { text = "\tShader" });
                    EditorGUILayout.PropertyField(DepthBlurCutoff, new GUIContent { text = "\tCutoff" });
                    EditorGUILayout.PropertyField(DepthBlurIterations, new GUIContent { text = "\tIterations" });
                    EditorGUILayout.PropertyField(DepthBlurSurfacePower, new GUIContent { text = "\tSurface Power" });
                    EditorGUILayout.PropertyField(DepthBlurHorizontalMultiplier, new GUIContent { text = "\tHorizontal Multiplier" });
                    EditorGUILayout.PropertyField(DepthBlurVerticalMultiplier, new GUIContent { text = "\tVertical Multiplier" });
                    (target as MirrorManager)?.InitializeMirror(); //run always to see the effect just in time
                }

                EditorGUILayout.PropertyField(EnableSimpleBlur, new GUIContent { text = "Enable Simple Blur" });
                if (EnableSimpleBlur.boolValue)
                {
                    EditorGUILayout.PropertyField(SimpleBlurShader, new GUIContent { text = "\tShader" });
                    EditorGUILayout.PropertyField(SimpleBlurIterations, new GUIContent { text = "\tIterations" });
                    EditorGUILayout.PropertyField(SimpleBlurPixels, new GUIContent { text = "\tPixels" });
                    (target as MirrorManager)?.InitializeMirror(); //run always to see the effect just in time
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();



            EditorGUILayout.Space(DefaultSpace);
            MenuCamera = EditorGUILayout.BeginFoldoutHeaderGroup(MenuCamera, new GUIContent { text = "Camera" });
            if (MenuCamera)
            {
                EditorGUILayout.PropertyField(UseClipping);
                if (UseClipping.boolValue)
                {
                    EditorGUILayout.PropertyField(ClippingPercentage, new GUIContent { text = "\tPercentage" });
                    EditorGUILayout.PropertyField(ClipUV, new GUIContent { text = "\tUV" });
                    EditorGUILayout.PropertyField(ClipEye, new GUIContent { text = "\tVR eye to render" });
                    //EditorGUILayout.PropertyField(ClipMultiplier, new GUIContent { text = "\tVR Multiplier" });
                    //Cli
                }
                else
                {
                    ClipUV.intValue = (int)AkilliMum.SRP.Mirror.ClipUV.None;
                    ClipEye.intValue = (int)AkilliMum.SRP.Mirror.ClipEye.None;
                }
                EditorGUILayout.PropertyField(HDR);
                EditorGUILayout.PropertyField(FOV);
                EditorGUILayout.PropertyField(MSAALevel);
                EditorGUILayout.PropertyField(AntialiasingMode);
                EditorGUILayout.PropertyField(AntialiasingQuality);
                EditorGUILayout.PropertyField(RenderPostProcessing);
                EditorGUILayout.PropertyField(RequiresOpaqueTexture);
                EditorGUILayout.PropertyField(RequiresDepthTexture);
                EditorGUILayout.PropertyField(FilterMode);
                EditorGUILayout.PropertyField(MipMapping);
                EditorGUILayout.PropertyField(DisablePixelLights);
                EditorGUILayout.PropertyField(Shadow);
                EditorGUILayout.PropertyField(Cull);
                if (Cull.boolValue)
                {
                    EditorGUILayout.PropertyField(CullDistance, new GUIContent { text = "\tDistance" });
                }
                EditorGUILayout.PropertyField(RenderTextureSize);
                if (RenderTextureSize.intValue == (int)Mirror.RenderTextureSize.Manual)
                {
                    EditorGUILayout.PropertyField(ManualSize, new GUIContent { text = "\tSize" });
                }
                EditorGUILayout.PropertyField(ClipPlaneOffset);
                EditorGUILayout.PropertyField(CameraLODLevel, new GUIContent { text = "LOD Level" });
                EditorGUILayout.PropertyField(ReflectLayers);

            }
            EditorGUILayout.EndFoldoutHeaderGroup();



            EditorGUILayout.Space(DefaultSpace);
            EditorGUILayout.PropertyField(CustomShaders);



            EditorGUILayout.Space(DefaultSpace);
            EditorGUILayout.PropertyField(UIImage, new GUIContent { text = "Mirror Tex to Visualize" });
            EditorGUILayout.PropertyField(UIImageDepth, new GUIContent { text = "Depth Tex to Visualize" });


            if (GUILayout.Button("Apply Changes"))
                (target as MirrorManager)?.InitializeMirror();



            serializedObject.ApplyModifiedProperties();
        }

    }

}