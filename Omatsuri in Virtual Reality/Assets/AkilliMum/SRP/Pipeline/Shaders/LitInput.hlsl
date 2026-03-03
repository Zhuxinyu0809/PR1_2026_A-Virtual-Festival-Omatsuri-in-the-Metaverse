#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "../../Pipeline/ShaderLibrary/Core.hlsl"
#include "../../Pipeline/Core/ShaderLibrary/CommonMaterial.hlsl"
#include "../../Pipeline/ShaderLibrary/SurfaceInput.hlsl"
#include "../../Pipeline/Core/ShaderLibrary/ParallaxMapping.hlsl"
#include "../../Pipeline/ShaderLibrary/DBuffer.hlsl"
#include "../../Pipeline/ShaderLibrary/CommonOperations.hlsl"
#if defined (_AKMU_CARPAINT_GEOMETRY)
//#include "../../CarPaint/Shader/CarPaintDefinitions.hlsl"
#include "../../CarPaint/Shader/AkMuWireframe.hlsl"
#endif

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
    //do not defined detail
#else
#if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
#define _DETAIL
#endif
#endif

//global sampler to reduce samplers, already included in globalsaplers.hlsl
//SAMPLER(sampler_PointClamp);
//SAMPLER(sampler_LinearClamp);
//SAMPLER(sampler_PointRepeat);
//SAMPLER(sampler_LinearRepeat);

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
//do not defined detail
#else
float4 _DetailAlbedoMap_ST;
#endif

half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _Parallax;
half _OcclusionStrength;
half _ClearCoatMask;
half _ClearCoatSmoothness;

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
//do not defined detail
#else
half _DetailAlbedoMapScale;
half _DetailNormalMapScale;
#endif

half _Surface;

float4 _UV0TileOffset;
float4 _UV1TileOffset;
float4 _UV2TileOffset;
float4 _UV3TileOffset;

float O_REFLECTION_PROBE_BLENDING;
float O_REFLECTION_PROBE_BOX_PROJECTION;
float O_LIGHT_COOKIES;
//float O_WRITE_RENDERING_LAYERS;
float O_LOD_FADE_CROSSFADE;
float O_DEBUG_DISPLAY;
// //float O_LIGHTMAP_ON;
// float O_DYNAMICLIGHTMAP_ON;
// //float O_LIGHT_LAYERS;
//float O_LIGHTMAP_SHADOW_MIXING;
//float O_SHADOWS_SHADOWMASK;
//float O_DIRLIGHTMAP_COMBINED;
// //float O_DOTS_INSTANCING_ON;
// //float O_ADDITIONAL_LIGHT_SHADOWS;
//float O_SHADOWS_SOFT;
// //float O_EMISSION;

uint _BaseMapUV;
uint _SpecularUV;
uint _MetallicUV;
uint _NormalUV;
uint _ParallaxUV;
uint _OcclusionUV;
uint _ClearCoatUV;
uint _EmissionUV;

#ifdef _AKMU_SHADOW_RECEIVER
float _AdditionalLightsColorDistance;
#endif

#ifdef _AKMU_MIRROR
//my variables
float _IsMultiPass;

half _FULLMIRROR;

float4 _LeftOrCenterTexture_ST;
float4 _RightTexture_ST;
float4 _RefractionTex_ST;
float4 _MaskTex_ST;
float4 _RippleTex_ST;
float4 _WaveNoiseTex_ST;

half _EnableDepthBlur;

half _EnableSimpleDepth;
float _SimpleDepthCutoff;
//float _DepthBlur;
float _NearClip;
float _FarClip;

half _ReflectionIntensity;
float _UseFresnel;
//float _UseFresnelPower;
float _LODLevel;
//float _WetLevel;
float _MixBlackColor;

half _EnableRefraction;
half _ReflectionRefraction;

half _EnableMask;
half _MaskCutoff;
half _MaskEdgeDarkness;
half4 _MaskTiling;

//half _UseOpaqueCamImage; //todo:

half _EnableWave;
half _WaveSize;
half _WaveDistortion;
half _WaveSpeed;

half _EnableRipple;
half _RippleSize;
half _RippleRefraction;
half _RippleDensity;
half _RippleSpeed;

float _WorkType;
//float _DeviceType;
//half4 _MirrorCenter;
int _ClipUV;
int _ClipEye;
int _ClipPercentage;

half _EnableLocallyCorrection;
float4 _BBoxMin;
float4 _BBoxMax;
float4 _EnviCubeMapPos;
float _EnableRotation;
float4 _EnviRotation;
float4 _EnviPosition;
#endif

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
//#ifdef _AKMU_CARPAINT_TRIPLANAR
//float4 _TriplanarUpMap_ST;
//float4 _TriplanarSideMap_ST;
//float4 _TriplanarFaceMap_ST;
//float4 _TriPlanarTileOffset;
//#endif

#ifdef _AKMU_CARPAINT_DIRTBUMP
//float _EnableDirt;
uint _DirtUV;
float4 _DirtMap_ST;
float4 _DirtBumpMap_ST;
//float _DirtUsage;
half4 _DirtColor;
float _DirtMetalic;
float _DirtSmoothness;
float _DirtMapCutoff;
//float4 _DirtTileOffset;
#endif

#ifdef _AKMU_CARPAINT_LIVERY
//and together
//float _EnableLivery;
uint _LiveryUV;
float4 _LiveryMap_ST;
//float _LiveryUsage;
half4 _LiveryColor;
float _LiveryMetalic;
float _LiverySmoothness;
//float4 _LiveryTileOffset;
#endif

#ifdef _AKMU_CARPAINT_DECAL
//and together
//float _EnableDecal;
uint _DecalUV;
float4 _DecalMap_ST;
//float _DecalUsage;
half4 _DecalColor;
float _DecalMetalic;
float _DecalSmoothness;
//float4 _DecalTileOffset;
#endif

#ifdef _AKMU_CARPAINT_FLAKESBUMP
//and together
//float _EnableFlake;
float4 _FlakesBumpMap_ST;
//float _FlakesUsage;
float _FlakesBumpMapScale;
float _FlakesBumpStrength;
#endif

half4 _BaseColorNext;
half4 _FresnelColor;
half4 _FresnelColorNext;
half4 _FresnelColor2;
half4 _FresnelColor2Next;
float _FresnelPower;
float _FresnelGap;

float _WeatherType;

//float3 _RendererCenter;
//float3 _RendererRotation;

#if defined _AKMU_CARPAINT_WEATHER
//#if defined _AKMU_CARPAINT_RAINY || defined _AKMU_CARPAINT_SNOWY
float4 _DropletMask_ST;
float4 _RivuletMask_ST;
//#endif
//#if defined _AKMU_CARPAINT_RAINY
//float _EnableRain;
//float _Wetness;
//half _Rain;
half _AKMU_CARPAINT_RAINY;
float _Distortion;
float4 _TilingDroplet;
float4 _TilingRivulet;
float _Droplets_Strength;
//float4 _RivuletBump_ST;
//float _GlobalRotation;
float _RivuletRotation;
float _RivuletLockDirection;
float _RivuletSpeed;
float _RivuletsStrength;
float _DropletsGravity;
float _DropletsStrikeSpeed;
float _WaveSize;
float _WaveSpeed;
float _WaveDistortion;
//#endif
//#ifdef _AKMU_CARPAINT_SNOWY
//and together with snow
//float4 _SnowTexture_ST;
//float4 _SnowNormal_ST;
//float _EnableSnow;
half _AKMU_CARPAINT_SNOWY;
half _Snow;
half4 _SnowColor;
half4 _SnowDirection;
half _SnowLevel;
half _SnowGlossiness;
half _SnowMetallic;
uint _SnowUV;
uint _SnowNormalUV;
//half _SnowCutoff;
//#endif
#endif

//float _FadeEffectUsage;
//float4 _FadeEffectPosition;
//float _FadeEffectPower;
//float _FadeEffectDistance;

float _Brightness;
float _FresnelIntensity;

int _EnableRealTimeReflection;
int _Marker;
float4 _BBoxMin;
float4 _BBoxMax;
float4 _EnviCubeMapPos;
float4 _EnviCubeMapLength;
//float _EnviCubeSmoothness;
//float _EnviCubeIntensity;
float _MixMultiplier;
//float _EnableRotation;
float4 _EnviRotation;
//float4 _EnviPosition;

#ifdef _AKMU_CARPAINT_GEOMETRY
#ifdef _AKMU_GEOM_VOXEL
float _Voxel;
float _VoxelSize;
#endif

#ifdef _AKMU_GEOM_ANIMATION
float _Animation;
float _AnimSize;
float _AnimSpeed;
float3 _AnimDirection;
#endif

#ifdef _AKMU_GEOM_SHRINK
float _Shrink;
float _ShrinkAmount;
#endif

#ifdef _AKMU_GEOM_LOWPOLY
float _LowPoly;
float _SingleColor;
#endif

#ifdef _AKMU_GEOM_EXTRUDE
float _Extrude;
float _ExtrudeSize;
float _ExtrudeAnimate;
float _ExtrudeAnimSpeed;
float _ExtrudePos;
#endif

#ifdef _AKMU_GEOM_WIREFRAME
float _Wireframe;
float _WireframeSize;
float _WireframeQuad;
float _WireframeShadowOnly;
float _WireframeShadowSize;
float _WireframeSmoothness;
float _WireframeMetallic;
half4 _WireframeBaseColor;
//half4 _WireframeSpecColor;
half4 _WireframeEmissionColor;
#endif
#endif
#endif

#if defined(_AKMU_USE_MIRROR_DEPTH)
#undef _AKMU_USE_MIRROR_DEPTH
#endif
#if defined(_AKMU_USE_MIRROR_DEPTH) || defined(_AKMU_DECALTEMP) || defined(_AKMU_DECAL)
float4 _TempDepthTexture_ST;
float _ProjectionAngleDiscardThreshold;
//half4 _Color;
half2 _AlphaRemap;
half _MulAlphaToRGB;
#endif

CBUFFER_END

// NOTE: Do not ifdef the properties for dots instancing, but ifdef the actual usage.
// Otherwise you might break CPU-side as property constant-buffer offsets change per variant.
// NOTE: Dots instancing is orthogonal to the constant buffer above.
#ifdef UNITY_DOTS_INSTANCING_ENABLED

UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
UNITY_DOTS_INSTANCED_PROP(float , _Smoothness)
UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
UNITY_DOTS_INSTANCED_PROP(float , _BumpScale)
UNITY_DOTS_INSTANCED_PROP(float , _Parallax)
UNITY_DOTS_INSTANCED_PROP(float , _OcclusionStrength)
UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatMask)
UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatSmoothness)

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
//do not defined details
#else
UNITY_DOTS_INSTANCED_PROP(float , _DetailAlbedoMapScale)
UNITY_DOTS_INSTANCED_PROP(float , _DetailNormalMapScale)
#endif

UNITY_DOTS_INSTANCED_PROP(float , _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

#define _BaseColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _BaseColor)
#define _SpecColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _SpecColor)
#define _EmissionColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _EmissionColor)
#define _Cutoff                 UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Cutoff)
#define _Smoothness             UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Smoothness)
#define _Metallic               UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Metallic)
#define _BumpScale              UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _BumpScale)
#define _Parallax               UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Parallax)
#define _OcclusionStrength      UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _OcclusionStrength)
#define _ClearCoatMask          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _ClearCoatMask)
#define _ClearCoatSmoothness    UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _ClearCoatSmoothness)

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
//do not define details
#else
#define _DetailAlbedoMapScale   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _DetailAlbedoMapScale)
#define _DetailNormalMapScale   UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _DetailNormalMapScale)
#endif

#define _Surface                UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Surface)
#endif

TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);

#if defined _AKMU_MIRROR || defined _AKMU_DECAL
//these will be used in both mirror and decal
float4 _LeftOrCenterDepthTexture_ST;
TEXTURE2D_X_FLOAT(_LeftOrCenterDepthTexture);
SAMPLER(sampler_LeftOrCenterDepthTexture);
float4 _RightDepthTexture_ST;
TEXTURE2D_X_FLOAT(_RightDepthTexture);
SAMPLER(sampler_RightDepthTexture);
#endif

#ifdef _AKMU_MIRROR
//my variables
TEXTURE2D(_LeftOrCenterTexture);       SAMPLER(sampler_LeftOrCenterTexture);
TEXTURE2D(_RightTexture);       SAMPLER(sampler_RightTexture);

#ifdef _AKMU_MIRROR_REFRACTION
TEXTURE2D(_RefractionTex);       ///////////////////SAMPLER(sampler_RefractionTex);
#endif
#ifdef _AKMU_MIRROR_MASK
TEXTURE2D(_MaskTex);       /////////////////SAMPLER(sampler_MaskTex);
#endif
#ifdef _AKMU_MIRROR_RIPPLE
TEXTURE2D(_RippleTex);       //////////////////SAMPLER(sampler_RippleTex);
#endif
#ifdef _AKMU_MIRROR_WAVE
TEXTURE2D(_WaveNoiseTex);       /////////////////SAMPLER(sampler_WaveNoiseTex);
#endif
#endif

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
//do not define detail
#else
TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);
#endif

TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_ClearCoatMap);       SAMPLER(sampler_ClearCoatMap);

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
// HDR environment map decode instructions
//half4 _EnviCubeMapToMix1_HDR;
// mix reflection probe
//TEXTURECUBE(_EnviCubeMapToMix1);
//SAMPLER(sampler_EnviCubeMapToMix1);

//todo:
//// HDR environment map decode instructions
//half4 _EnviCubeBox_HDR;
//// Default reflection probe
//TEXTURECUBE(_EnviCubeBox);
//SAMPLER(sampler_EnviCubeBox);

// HDR environment map decode instructions
half4 _EnviCubeMapMain_HDR;
// Default reflection probe
TEXTURECUBE(_EnviCubeMapMain);
SAMPLER(sampler_EnviCubeMapMain);

//todo:
//half4 _EnviCubeMapSecondary_HDR;
//// Default reflection probe
//TEXTURECUBE(_EnviCubeMapSecondary);
//SAMPLER(sampler_EnviCubeMapSecondary);

//will use rain texture slots because of the 
// Shader error in maximum ps_5_0 sampler register index (16) exceeded at (on d3d11)
//#ifdef _AKMU_CARPAINT_SNOWY
//TEXTURE2D(_SnowTexture);       SAMPLER(sampler_SnowTexture); == _RivuletMask
//TEXTURE2D(_SnowNormal);       SAMPLER(sampler_SnowNormal); == _RivuletBump
//#endif

#if defined _AKMU_CARPAINT_WEATHER
//#if defined _AKMU_CARPAINT_RAINY || defined _AKMU_CARPAINT_SNOWY
TEXTURE2D(_RivuletMask);       ///////////////SAMPLER(sampler_RivuletMask);
//TEXTURE2D(_RivuletBump);       //////////////SAMPLER(sampler_RivuletBump);
TEXTURE2D(_DropletMask);       //////////////SAMPLER(sampler_DropletMask);
//#endif
#endif

#ifdef _AKMU_CARPAINT_DIRTBUMP
TEXTURE2D(_DirtMap);       ///////////SAMPLER(sampler_DirtMap);
TEXTURE2D(_DirtBumpMap);       //////////////SAMPLER(sampler_DirtBumpMap);
#endif

#ifdef _AKMU_CARPAINT_LIVERY
TEXTURE2D(_LiveryMap);       //////////////////SAMPLER(sampler_LiveryMap);
#endif

#ifdef _AKMU_CARPAINT_DECAL
TEXTURE2D(_DecalMap);       ////////////SAMPLER(sampler_DecalMap);
#endif

#ifdef _AKMU_CARPAINT_FLAKESBUMP
TEXTURE2D(_FlakesBumpMap);       /////////////////SAMPLER(sampler_FlakesBumpMap);
#endif

//#ifdef _AKMU_CARPAINT_TRIPLANAR
//TEXTURE2D(_TriPlanarUpMap);       SAMPLER(sampler_TriPlanarUpMap);
//TEXTURE2D(_TriPlanarSideMap);       SAMPLER(sampler_TriPlanarSideMap);
//TEXTURE2D(_TriPlanarFaceMap);       SAMPLER(sampler_TriPlanarFaceMap);
//#endif
#endif

#if defined(_AKMU_USE_MIRROR_DEPTH) || defined(_AKMU_DECALTEMP) || defined(_AKMU_DECAL)
TEXTURE2D(_TempDepthTexture);       SAMPLER(sampler_TempDepthTexture);
#endif

#ifdef _SPECULAR_SETUP
#define SAMPLE_METALLICSPECULAR(uv_0, uv_1, uv_2, uv_3) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, FindUV(_SpecularUV, uv_0, uv_1, uv_2, uv_3))
#else 
#define SAMPLE_METALLICSPECULAR(uv_0, uv_1, uv_2, uv_3) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, FindUV(_MetallicUV, uv_0, uv_1, uv_2, uv_3))
#endif

half4 SampleMetallicSpecGloss(float2 uv_0, float2 uv_1, float2 uv_2, float2 uv_3, half albedoAlpha)
{
    half4 specGloss;

#ifdef _METALLICSPECGLOSSMAP
    specGloss = half4(SAMPLE_METALLICSPECULAR(uv_0, uv_1, uv_2, uv_3));
#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * _Smoothness;
#else
    specGloss.a *= _Smoothness;
#endif
#else // _METALLICSPECGLOSSMAP
#if _SPECULAR_SETUP
    specGloss.rgb = _SpecColor.rgb;
#else
    specGloss.rgb = _Metallic.rrr;
#endif

#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * _Smoothness;
#else
    specGloss.a = _Smoothness;
#endif
#endif

    return specGloss;
}

half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
    half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    return LerpWhiteTo(occ, _OcclusionStrength);
#else
    return half(1.0);
#endif
}

#define FLT_MAX 3.402823466e+38
#define FLT_MIN 1.175494351e-38
#define DBL_MAX 1.7976931348623158e+308
#define DBL_MIN 2.2250738585072014e-308

// Returns clear coat parameters
// .x/.r == mask
// .y/.g == smoothness
half2 SampleClearCoat(float2 uv)
{
#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoatMaskSmoothness = half2(_ClearCoatMask, _ClearCoatSmoothness);

#if defined(_CLEARCOATMAP)
    clearCoatMaskSmoothness *= SAMPLE_TEXTURE2D(_ClearCoatMap, sampler_ClearCoatMap, uv).rg;
#endif

    return clearCoatMaskSmoothness;
#else
    return half2(0.0, 1.0);
#endif  // _CLEARCOAT
}

void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv_0, inout float2 uv_1, inout float2 uv_2, inout float2 uv_3)
{
#if defined(_PARALLAXMAP)
    if (_ParallaxUV == 0)
        uv_0 += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv_0);
    if (_ParallaxUV == 1)
        uv_1 += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv_1);
    if (_ParallaxUV == 2)
        uv_2 += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv_2);
    if (_ParallaxUV == 3)
        uv_3 += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv_3);
    //uv += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv);
#endif
}
void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv)
{
#if defined(_PARALLAXMAP)
    uv += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv);
#endif
}

// Used for scaling detail albedo. Main features:
// - Depending if detailAlbedo brightens or darkens, scale magnifies effect.
// - No effect is applied if detailAlbedo is 0.5.
half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
{
    // detailAlbedo = detailAlbedo * 2.0h - 1.0h;
    // detailAlbedo *= _DetailAlbedoMapScale;
    // detailAlbedo = detailAlbedo * 0.5h + 0.5h;
    // return detailAlbedo * 2.0f;

    // A bit more optimized
    return half(2.0) * detailAlbedo * scale - scale + half(1.0);
}

half3 ApplyDetailAlbedo(float2 detailUv, half3 albedo, half detailMask)
{
#if defined(_DETAIL)
    half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, detailUv).rgb;

    // In order to have same performance as builtin, we do scaling only if scale is not 1.0 (Scaled version has 6 additional instructions)
#if defined(_DETAIL_SCALED)
    detailAlbedo = ScaleDetailAlbedo(detailAlbedo, _DetailAlbedoMapScale);
#else
    detailAlbedo = half(2.0) * detailAlbedo;
#endif

    return albedo * LerpWhiteTo(detailAlbedo, detailMask);
#else
    return albedo;
#endif
}

half3 ApplyDetailNormal(float2 detailUv, half3 normalTS, half detailMask)
{
#if defined(_DETAIL)
#if BUMP_SCALE_NOT_SUPPORTED
    half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv));
#else
    half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv), _DetailNormalMapScale);
#endif

    // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
    // For visual consistancy we going to do in all cases
    detailNormalTS = normalize(detailNormalTS);

    return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
#else
    return normalTS;
#endif
}

inline void InitializeStandardLitSurfaceData(float2 uv_0, float2 uv_1, float2 uv_2, float2 uv_3, float3 baryCoord, float3 posWS,
    out SurfaceData outSurfaceData, out half wireframe)
{
    wireframe = 0;
#if defined (_AKMU_CARPAINT) && defined (_AKMU_CARPAINT_GEOMETRY) && defined (_AKMU_GEOM_WIREFRAME)
    wireframe = saturate(WireframeBS(baryCoord, _WireframeSize));
#endif

    half4 albedoAlpha = SampleAlbedoAlpha(FindUV(_BaseMapUV, uv_0, uv_1, uv_2, uv_3), TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
#if defined (_AKMU_CARPAINT) && defined (_AKMU_CARPAINT_GEOMETRY) && defined (_AKMU_GEOM_WIREFRAME)
    //outSurfaceData.alpha = Alpha(min(albedoAlpha.a, wireframe), _BaseColor, _Cutoff);
    outSurfaceData.alpha = Alpha(wireframe, 1, _Cutoff);
    if (outSurfaceData.alpha <= 0)
        outSurfaceData.alpha = _BaseColor.a;
#else
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
#endif

    half4 specGloss = SampleMetallicSpecGloss(uv_0, uv_1, uv_2, uv_3, albedoAlpha.a);

#if defined (_AKMU_CARPAINT) && defined (_AKMU_CARPAINT_GEOMETRY) && defined (_AKMU_GEOM_WIREFRAME)
    outSurfaceData.albedo = lerp(albedoAlpha.rgb, _WireframeBaseColor.rgb, wireframe);
#elif defined _AKMU_CARPAINT
    outSurfaceData.albedo = albedoAlpha.rgb; // * _BaseColor.rgb //closed for car paint
#else
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
#endif
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

#if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
#else
#if defined (_AKMU_CARPAINT) && defined (_AKMU_CARPAINT_GEOMETRY) && defined (_AKMU_GEOM_WIREFRAME)
    outSurfaceData.metallic = lerp(specGloss.r, _WireframeMetallic, wireframe);
#else
    outSurfaceData.metallic = specGloss.r;
#endif
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
#endif

#if defined (_AKMU_CARPAINT) && defined (_AKMU_CARPAINT_GEOMETRY) && defined (_AKMU_GEOM_WIREFRAME)
    outSurfaceData.smoothness = lerp(specGloss.a, _WireframeSmoothness, wireframe);
#else
    outSurfaceData.smoothness = specGloss.a;
#endif

    outSurfaceData.normalTS = SampleNormal(FindUV(_NormalUV, uv_0, uv_1, uv_2, uv_3), TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(FindUV(_OcclusionUV, uv_0, uv_1, uv_2, uv_3));

#if defined (_AKMU_GEOM_WIREFRAME)
    outSurfaceData.emission = lerp(
        SampleEmission(FindUV(_EmissionUV, uv_0, uv_1, uv_2, uv_3), _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap)),
        //outSurfaceData.alpha, outSurfaceData.alpha),
        SampleEmission(FindUV(_EmissionUV, uv_0, uv_1, uv_2, uv_3), _WireframeEmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap)),
        //outSurfaceData.alpha, outSurfaceData.alpha),
        wireframe);
#elif defined _AKMU_CARPAINT
    /*outSurfaceData.emission = SampleEmission(FindUV(_EmissionUV, uv_0, uv_1, uv_2, uv_3), _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap),
        outSurfaceData.alpha, outSurfaceData.alpha);*/
    outSurfaceData.emission = SampleEmission(FindUV(_EmissionUV, uv_0, uv_1, uv_2, uv_3), _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
#else
    outSurfaceData.emission = SampleEmission(FindUV(_EmissionUV, uv_0, uv_1, uv_2, uv_3), _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
#endif

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(FindUV(_ClearCoatUV, uv_0, uv_1, uv_2, uv_3));
    outSurfaceData.clearCoatMask = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
#else
    outSurfaceData.clearCoatMask = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
#endif

#if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
#endif
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
