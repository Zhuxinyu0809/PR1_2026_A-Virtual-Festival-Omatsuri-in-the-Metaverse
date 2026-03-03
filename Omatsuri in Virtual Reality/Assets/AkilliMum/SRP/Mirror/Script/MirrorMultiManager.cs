//#define DEBUG_RENDER
//#define DEBUG_LOG

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using UnityEngine.Rendering;

// ReSharper disable UnusedMember.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace AkilliMum.SRP.Mirror
{
    //[ImageEffectAllowedInSceneView]
    //[ExecuteInEditMode]
    public class MirrorMultiManager : MonoBehaviour
    {
        IList<MirrorManager> _originalMirrorManagers = new List<MirrorManager>();

        MirrorManager[,] _copyMirrorManagers;
        IList<RenderTexture>[,] _copyTextures;

        [Tooltip("Mirror in mirror recursive count")]
        [Range(1, 10)]
        public int Depth = 2;
        [Tooltip("Please use this to give unique id's to mirrors which will be drawn together. So if you want to see a mirror inside another mirror, their id must be same!")]
        public string MirrorInMirrorId;
        [Tooltip("Decreases the size of the reflection 2 over X times for each depth, so you may gain performance but may lose reality!")]
        [Range(0, 10)]
        public float DecreaseSize2OverXTimes = 1;
        [Tooltip("Draws shadows only for first X depth (if applicable on real mirror, if it is not; if will disable it anyway)!")]
        [Range(1, 10)]
        public int ShadowDepth = 1;
        [Tooltip("Disables the MSAA after Xth depth (if applicable on real mirror, if it is not; if will disable it anyway)!")]
        [Range(1, 10)]
        public int DisableMSAAAfterXthDepth = 1;
        [Tooltip("Disables the pixel lights after Xth depth (if applicable on real mirror, if it is not; if will disable it anyway)!")]
        [Range(1, 10)]
        public int DisablePixelLightsAfterXthDepth = 1;

        [NonSerialized]
        public Camera _camera;
        [NonSerialized]
        public ScriptableRenderContext _context;

        private void OnEnable()
        {
            InitializeProperties();

        }

        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= ExecuteBeforeCameraRender;
        }

        public async void InitializeProperties()
        {
            A:
            //get originals
            _originalMirrorManagers = GetComponents<MirrorManager>();
            _originalMirrorManagers = _originalMirrorManagers
             .Where(a => a.IsMirrorInMirror && a.MirrorInMirrorId == MirrorInMirrorId).ToList();

            if (_originalMirrorManagers.Count <= 0)
            {
                await Task.Delay(300);
                goto A;
            }

            foreach (var originalMirrorManager in _originalMirrorManagers)
            {
                if (originalMirrorManager._renderTextureManager == null ||
                    originalMirrorManager._cameraManager == null ||
                    originalMirrorManager._rendererManager == null ||
                    originalMirrorManager._optionManager == null)
                {
                    await Task.Delay(300);
                    goto A;
                }
            }

            //create N * Depth script copy
            _copyMirrorManagers = new MirrorManager[_originalMirrorManagers.Count, Depth];
            _copyTextures = new IList<RenderTexture>[_originalMirrorManagers.Count, Depth];

            //Wait real managers
            
            for (int camIndex = 0; camIndex < _originalMirrorManagers.Count; camIndex++)
            {
                for (int depth = 0; depth < Depth; depth++)
                {
                    _copyMirrorManagers[camIndex, depth] = gameObject.AddComponent<MirrorManager>();
#if DEBUG_LOG
                    //Debug.Log("found a mirror in mirror camera shade. adding to list...");
#endif
                    _originalMirrorManagers[camIndex].CopyTo(_copyMirrorManagers[camIndex, depth]);
                    
                    //get the real size of original
                    var size = _originalMirrorManagers[camIndex]._renderTextureManager.GetTextureSizes(
                        _originalMirrorManagers[camIndex]._cameraManager);
                    //set the copy to manual
                    //copy._renderTextureManager.GetSettings().Size = .TextureSize = TextureSizeType.Manual;
                    //decrease the size on each depth; depth/1, depth/2, depth/3 etc
                    //copy.ManualSize = size / (int)Mathf.Pow(2, DecreaseSize2OverXTimes * depth);
                    //copy.ManualSize = copy.ManualSize + copy.ManualSize % 2;
                    //if (copy.ManualSize <= 128)
                    //    copy.ManualSize = 128;
                    var settings = _copyMirrorManagers[camIndex, depth]._renderTextureManager.GetSettings();
                    settings.Size = RenderTextureSize.Manual;
                    // ReSharper disable once PossibleLossOfFraction
                    settings.ManualSize = size[0] / (int)Mathf.Pow(2, DecreaseSize2OverXTimes * depth);
                    settings.ManualSize = settings.ManualSize + settings.ManualSize % 2;
                    if (settings.ManualSize < 128)
                        settings.ManualSize = 128;
                    _copyMirrorManagers[camIndex, depth].RenderTextureSize = RenderTextureSize.Manual;
                    _copyMirrorManagers[camIndex, depth].ManualSize = settings.ManualSize;
                    //Debug.Log("manual size: " + settings.ManualSize);
                    _copyMirrorManagers[camIndex, depth]._renderTextureManager.SetSettings(settings);

                    //shadows?
                    if (_originalMirrorManagers[camIndex].Shadow && ShadowDepth > depth)
                    {
                        _copyMirrorManagers[camIndex, depth].Shadow = true;
                    }
                    else //do not draw shadows for depth > X
                    {
                        _copyMirrorManagers[camIndex, depth].Shadow = false;
                        //copy.ShadowDistance = 0;
                    }

                    ////MSAA
                    //if (_originalMirrorManagers[camIndex].MSAA && DisableMSAAAfterXthDepth > depth)
                    //{
                    //    copy.MSAA = true;
                    //}
                    //else
                    //{
                    //    copy.MSAA = false;
                    //}

                    //Lights
                    if (_originalMirrorManagers[camIndex].DisablePixelLights == false && DisablePixelLightsAfterXthDepth > depth)
                    {
                        _copyMirrorManagers[camIndex, depth].DisablePixelLights = false;
                    }
                    else
                    {
                        _copyMirrorManagers[camIndex, depth].DisablePixelLights = true;
                    }

                    //initialize and redraw
                    //copy.
                    _copyMirrorManagers[camIndex, depth].InitializeMirror();
                }
            }

            //start rendering after setup!!!
            RenderPipelineManager.beginCameraRendering += ExecuteBeforeCameraRender;
        }

        public void ExecuteBeforeCameraRender(
           ScriptableRenderContext context,
           Camera cameraSrp)
        {
            _camera = cameraSrp;

            _context = context;

            RenderReflective();
        }

        int GetNextCamIndex(int camIndex)
        {
            camIndex += 1;
            if (camIndex >= _originalMirrorManagers.Count)
                return 0;
            return camIndex;
        }

        Camera[] cameraList;
        public void RenderReflective()
        {
            if(_originalMirrorManagers == null || _originalMirrorManagers.Count <= 0)
                return;

            if (_originalMirrorManagers.Count < 3)
            {
                //for each script in scene (which are mirror in mirror)
                for (int eachCam = 0; eachCam < _originalMirrorManagers.Count; eachCam++)
                {
                    //continue if mirror is not visible! (other mirrors will draw this one even if it is not visible anyway :) )
                    if (!_originalMirrorManagers[eachCam]._rendererManager.IsObjectVisible(_camera))
                    {
#if DEBUG_LOG
                    Debug.Log(eachCam + " is not visible, skipping it!");
#endif
                        continue;
                    }

                    cameraList = new Camera[Depth];

                    var nextCamIndex = eachCam;

                    cameraList[0] = null; //the main cam

                    for (int eachDepth = 1; eachDepth < Depth; eachDepth++)
                    {
                        //get camera according to last cam (reflection of it)
                        cameraList[eachDepth] = _originalMirrorManagers[nextCamIndex]
                            .RenderReflective(this, cameraList[eachDepth - 1], true, false)[0];
#if DEBUG_LOG
                                            Debug.Log("added cam=" + cameraList[eachDepth] + " for eachCam=" + eachCam + " eachDepth=" + eachDepth);
#endif
                        //get next cam
                        nextCamIndex = GetNextCamIndex(nextCamIndex);
                    }

                    nextCamIndex = eachCam;
                    if (Depth % 2 == 0) //if depth is odd we have to start from other cam
                        nextCamIndex = GetNextCamIndex(nextCamIndex);

                    //draw
                    for (int eachDepth = Depth - 1; eachDepth >= 0; eachDepth--)
                    {
#if DEBUG_LOG
                                            Debug.Log("draw next camIndex: " + nextCamIndex + " depth:" + eachDepth);
#endif
                        if (eachDepth == Depth - 1)
                        {
                            //do not draw anything for last depth
                            //_copyMirrorManagers[nextCamIndex, eachDepth].ReflectLayers = -1;
                            _copyMirrorManagers[nextCamIndex, eachDepth].DisableMirrorInMirror();
                            ////set intensity 0, so we draw only material texture (or material color) for nice display
                            //_copyMirrorManagers[nextCamIndex, eachDepth]._cameraManager.GetSettings()..Intensity = 0;
                            ////transparency may seem bad on last depth, so draw it as reflected always
                            //_copyMirrorManagers[nextCamIndex, eachDepth].WorkType = WorkType.Reflect;
                            ////enable g buffer, because real mirror (intensity 1) may bad results for last depth
                            //_copyMirrorManagers[nextCamIndex, eachDepth].DisableGBuffer = false;
                            ////disable probes anyway :)
                            ////_copyCameraShades[nextCamIndex, eachDepth].DisableRProbes = true;
                        }
                        _copyMirrorManagers[nextCamIndex, eachDepth]
                          .RenderReflective(this, cameraList[eachDepth], nextCamIndex == eachCam);

                        //get next cam
                        nextCamIndex = GetNextCamIndex(nextCamIndex);
                    }

                    //break; //todo: break on first loop, remove later

                    //get the latest (true) draw of the cam
                    _copyTextures[eachCam, 0] = _copyMirrorManagers[eachCam, 0]._renderTextureManager.CopyTextures();
                }

                //set latest (true) draws
                for (int eachCam = 0; eachCam < _originalMirrorManagers.Count; eachCam++)
                {
                    _copyMirrorManagers[eachCam, 0]._renderTextureManager.PasteTextures(_copyTextures[eachCam, 0]);
                    _copyMirrorManagers[eachCam, 0]._rendererManager.UpdateMaterials(
                        _copyMirrorManagers[eachCam, 0]._renderTextureManager,
                        _copyMirrorManagers[eachCam, 0]._cameraManager);
                }
            }
            else
            {
                //for each script in scene (which are mirror in mirror)
                for (int eachCam = 0; eachCam < _originalMirrorManagers.Count; eachCam++)
                {
                    //continue if mirror is not visible! (other mirrors will draw this one even if it is not visible anyway :) )
                    if (!_originalMirrorManagers[eachCam]._rendererManager.IsObjectVisible(_camera))
                    {
#if DEBUG_LOG
                        Debug.Log(eachCam + " is not visible, skipping it!");
#endif
                        continue;
                    }

                    cameraList = new Camera[_originalMirrorManagers.Count + 1];
                    int nextCam; // = eachCam;
                    int order = 0;
                    cameraList[order] = null;
                    order++;
                    //get the reflected cam for first mirror //DO NOT RENDER!
                    cameraList[order] = _originalMirrorManagers[eachCam].RenderReflective(this, null, true, false)[0];


                    //draw fakes recursion
                    for (int disableCam = 0; disableCam < _originalMirrorManagers.Count; disableCam++)
                    {
                        var cameram = _originalMirrorManagers[disableCam].RenderReflective(this, cameraList[1], true, false)[0];

                        //do not draw anything for last depth
                        //_copyMirrorManagers[disableCam, 2].ReflectLayers = -1;
                        _copyMirrorManagers[disableCam, 2].DisableMirrorInMirror();
                        ////set intensity 0, so we draw only material texture (or material color) for nice display
                        //_copyMirrorManagers[disableCam, 2].Intensity = 0;
                        ////transparency may seem bad on last depth, so draw it as reflected always
                        //_copyMirrorManagers[disableCam, 2].WorkType = WorkType.Reflect;
                        ////enable g buffer, because real mirror (intensity 1) may bad results for last depth
                        //_copyMirrorManagers[disableCam, 2].DisableGBuffer = false;
                        ////disable probes anyway :)
                        ////_copyCameraShades[disableCam, 2].DisableRProbes = true;

                        _copyMirrorManagers[disableCam, 2].RenderReflective(this, cameram);
                        _copyTextures[disableCam, 2] = _copyMirrorManagers[disableCam, 2]._renderTextureManager.CopyTextures();

                    }


                    //draw first recursion
                    nextCam = eachCam;
                    for (int toDrawCam = 0; toDrawCam < _originalMirrorManagers.Count; toDrawCam++)
                    {
                        if (eachCam == toDrawCam)
                        {
                            continue;
                        }

                        nextCam = GetNextCamIndex(nextCam);

                        //set fake textures
                        for (int disableCam = 0; disableCam < _originalMirrorManagers.Count; disableCam++)
                        {
                            _copyMirrorManagers[disableCam, 2]._renderTextureManager.PasteTextures(_copyTextures[disableCam, 2]);
                            _copyMirrorManagers[disableCam, 2]._rendererManager.UpdateMaterials(
                                _copyMirrorManagers[disableCam, 2]._renderTextureManager,
                                _copyMirrorManagers[disableCam, 2]._cameraManager);
                        }

                        //draw first recursion
                        _copyMirrorManagers[nextCam, 1].RenderReflective(this, cameraList[1], false);
                        _copyTextures[nextCam, 1] = _copyMirrorManagers[nextCam, 1]._renderTextureManager.CopyTextures();
                    }

                    //paste first recursion real textures
                    for (int toDrawCam = 0; toDrawCam < _originalMirrorManagers.Count; toDrawCam++)
                    {
                        _copyMirrorManagers[toDrawCam, 1]._renderTextureManager.PasteTextures(_copyTextures[toDrawCam, 1]);
                        _copyMirrorManagers[toDrawCam, 1]._rendererManager.UpdateMaterials(
                            _copyMirrorManagers[toDrawCam, 1]._renderTextureManager,
                            _copyMirrorManagers[toDrawCam, 1]._cameraManager);
                    }

                    //real one
                    _copyMirrorManagers[eachCam, 0].RenderReflective(this);
                    //get the latest (true) draw of the cam and copy the textures
                    _copyTextures[eachCam, 0] = _copyMirrorManagers[eachCam, 0]._renderTextureManager.CopyTextures();
                }

                //set latest (true) draws from textures
                for (int eachCam = 0; eachCam < _originalMirrorManagers.Count; eachCam++)
                {
                    _copyMirrorManagers[eachCam, 0]._renderTextureManager.PasteTextures(_copyTextures[eachCam, 0]);
                    _copyMirrorManagers[eachCam, 0]._rendererManager.UpdateMaterials(
                        _copyMirrorManagers[eachCam, 0]._renderTextureManager,
                        _copyMirrorManagers[eachCam, 0]._cameraManager);
                }
            }

        }
    }

    //public class Container
    //{
    //    public Camera Camera { get; set; }
    //    public CameraShade CameraShade { get; set; }
    //}
}