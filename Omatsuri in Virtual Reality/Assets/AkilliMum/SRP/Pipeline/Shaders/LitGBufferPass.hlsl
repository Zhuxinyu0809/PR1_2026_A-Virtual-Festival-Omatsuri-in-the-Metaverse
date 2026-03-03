#ifndef UNIVERSAL_LIT_GBUFFER_PASS_INCLUDED
#define UNIVERSAL_LIT_GBUFFER_PASS_INCLUDED

#include "../../Pipeline/ShaderLibrary/Lighting.hlsl"
#include "../../Pipeline/ShaderLibrary/UnityGBuffer.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "../../Pipeline/ShaderLibrary/LODCrossFade.hlsl"
#endif
#if defined(_AKMU_USE_MIRROR_DEPTH) || defined(_AKMU_DECALTEMP) || defined(_AKMU_DECAL)
	#include "../../Pipeline/ShaderLibrary/DeclareDepthTexture.hlsl"
	#include "../../Decal/Shader/DecalDefinitions.hlsl"
#endif
#if defined(_AKMU_MIRROR)
#include "../../Mirror/Shader/MirrorDefinitions.hlsl"
#endif
#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
#include "../../CarPaint/Shader/CarPaintDefinitions.hlsl"
#endif

// TODO: Currently we support viewDirTS caclulated in vertex shader and in fragments shader.
// As both solutions have their advantages and disadvantages (etc. shader target 2.0 has only 8 interpolators).
// We need to find out if we can stick to one solution, which we needs testing.
// So keeping this until I get manaul QA pass.
#if defined(_PARALLAXMAP) && (SHADER_TARGET >= 30)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitForwardPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;

	float2 staticLightmapUV   : TEXCOORD1; //it is texcoord_1 actually
	float2 dynamicLightmapUV  : TEXCOORD2; //it is texcoord_2 actually

	//float2 texcoord_1		  : TEXCOORD3;
	//float2 texcoord_2		  : TEXCOORD4;
	float2 texcoord_3		  : TEXCOORD3;

	//uint   id                 : SV_VertexID;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float3 normalOS                 : NORMAL;

	float2 uv                       : TEXCOORD0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
	float3 positionWS               : TEXCOORD1;
#endif

	half3 normalWS                  : TEXCOORD2;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
#ifndef _AKMU_CARPAINT_GEOMETRY
	half4 tangentWS                 : TEXCOORD3;    // xyz: tangent, w: sign
#endif
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half3 vertexLighting            : TEXCOORD4;    // xyz: vertex lighting
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	float4 shadowCoord              : TEXCOORD5;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR) && !defined (_AKMU_CARPAINT_GEOMETRY)
	half3 viewDirTS                 : TEXCOORD6;
#endif

	DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);

#ifdef DYNAMICLIGHTMAP_ON
	float2  dynamicLightmapUV       : TEXCOORD8; // Dynamic lightmap UVs
#else
	float2  dynamicLightmapUV       : TEXCOORD8; // for uv2 usage
#endif

	float3 viewDirWS                : TEXCOORD9;

//#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
	float4 positionOS               : TEXCOORD10;
//#endif

	//for other UV's
#ifndef _AKMU_CARPAINT_GEOMETRY  //uv_1 (second uv) is used for lightmap, I do not need it really
	float2 uv_1                     : TEXCOORD11;
#endif
	float2 uv_2                     : TEXCOORD12;
	float2 uv_3                     : TEXCOORD13;

	float4 screenPos                : TEXCOORD14;

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
	float3 barycentricCoord         : TEXCOORD15;
#endif

#if !defined (_AKMU_CARPAINT_GEOMETRY)
	float4 viewRayOS                : TEXCOORD16; // xyz: viewRayOS, w: extra copy of positionVS.z 
	float4 cameraPosOSAndFogFactor  : TEXCOORD17;
	float distance                  : TEXCOORD18;
#endif

	float eyeIndex					: TEXCOORD19;

	float4 positionCS               : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        inputData.positionWS = input.positionWS;
    #endif

    inputData.positionCS = input.positionCS;
	half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
#if (defined(_NORMALMAP) || defined(_DETAIL)) && !defined _AKMU_CARPAINT_GEOMETRY
	float sgn = input.tangentWS.w;      // should be either +1 or -1
	float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
	inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
#else
	inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
        inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    inputData.fogCoord = 0.0; // we don't apply fog in the guffer pass

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        inputData.vertexLighting = input.vertexLighting.xyz;
    #else
        inputData.vertexLighting = half3(0, 0, 0);
    #endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    inputData.bakedGI = SAMPLE_GI(input.vertexSH,
        GetAbsolutePositionWS(inputData.positionWS),
        inputData.normalWS,
        inputData.viewDirectionWS,
        inputData.positionCS.xy);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitGBufferPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

//#ifdef DUAL_PARABOLOID_MAPPING    // 自定义的变种开关
		//float3 vertex = mul(UNITY_MATRIX_MV, input.positionOS).xyz;    // 变换到相机坐标系
		//vertex.z = -vertex.z;    // 右手坐标系，相机前方为-Z，翻转轴向
		//float magnitude = length(vertex.xyz);
		//float3 normalizedVertPos = vertex.xyz / magnitude;    // 归一
		//output.objectPos.xy = normalizedVertPos.xy / (normalizedVertPos.z + 1);    // Normal = Incident + Reflection
		//output.objectPos.y = -output.objectPos.y;   // DX下如果渲染到 RenderTexture则需要加上这句
		//// 因为我采用了合适的正交相机，z和w不需要做修改
		//input.positionOS.xy = output.objectPos.xy;
	//#endif

	output.positionOS = input.positionOS;

	output.normalOS = input.normalOS;

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
	carPaintVertexPosition(input.positionOS, input.normalOS);
#endif

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

#ifdef _AKMU_MIRROR
	output.distance = MirrorDistance(input.positionOS);

	//todo: check this again!!!
	/*if (_ClipUV < 99) {
		output.uv = TRANSFORM_TEX(
			_ClipUV == 0 ? input.texcoord :
			(_ClipUV == 1 ? input.lightmapUV :
				(_ClipUV == 2 ? input.texcoord3 :
					input.texcoord4)), _BaseMap);
	}*/
#endif

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
	output.positionOS = input.positionOS;// mul(UNITY_MATRIX_MVP, input.positionOS);
#endif

	output.screenPos = ComputeScreenPos(vertexInput.positionCS);

	// normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

	output.uv = TRANSFORM_TEX(input.texcoord * _UV0TileOffset.xy + _UV0TileOffset.zw, _BaseMap);
#ifndef _AKMU_CARPAINT_GEOMETRY
	//output.uv_1 = TRANSFORM_TEX(input.texcoord_1 * _UV1TileOffset.xy + _UV1TileOffset.zw, _BaseMap);
	output.uv_1 = TRANSFORM_TEX(input.staticLightmapUV * _UV1TileOffset.xy + _UV1TileOffset.zw, _BaseMap);
#endif
	//output.uv_2 = TRANSFORM_TEX(input.texcoord_2 * _UV2TileOffset.xy + _UV2TileOffset.zw, _BaseMap);
	output.uv_2 = TRANSFORM_TEX(input.dynamicLightmapUV * _UV2TileOffset.xy + _UV2TileOffset.zw, _BaseMap);
	output.uv_3 = TRANSFORM_TEX(input.texcoord_3 * _UV3TileOffset.xy + _UV3TileOffset.zw, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;

#if (defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)) && !defined _AKMU_CARPAINT_GEOMETRY
	real sign = input.tangentOS.w * GetOddNegativeScale();
	half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif

#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) && !defined _AKMU_CARPAINT_GEOMETRY
	output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR) && !defined _AKMU_CARPAINT_GEOMETRY
	half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
	half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
	output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH4(vertexInput.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.vertexSH);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
	half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
	output.vertexLighting = vertexLight;
#endif

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
	output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

#if defined(_AKMU_USE_MIRROR_DEPTH) || defined(_AKMU_DECALTEMP) || defined(_AKMU_DECAL)
	decalVertexProcess(vertexInput.positionVS, output.viewRayOS, output.cameraPosOSAndFogFactor, 0);
#endif

    return output;
}

// Used in Standard (Physically Based) shader
FragmentOutput LitGBufferPassFragment(Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if defined(_AKMU_USE_MIRROR_DEPTH) || defined(_AKMU_DECALTEMP) || defined(_AKMU_DECAL)
	float4 shadowCoord;
	half3 viewDirWS;
	half4 WorldNormal = decalCalculateNormalPosShadow(input.screenPos, input.uv, input.viewRayOS, 
		input.cameraPosOSAndFogFactor, input.positionWS, shadowCoord, viewDirWS);
#endif

#if defined(_PARALLAXMAP)
	#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
		half3 viewDirTS = input.viewDirTS;
	#elif !defined _AKMU_CARPAINT_GEOMETRY
		half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
		half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
	#endif
	ApplyPerPixelDisplacement(viewDirTS, input.uv, input.uv_1, input.uv_2, input.uv_3);
#endif

    SurfaceData surfaceData;
	half wireframe;
    #if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
	#ifdef _AKMU_CARPAINT_GEOMETRY
		InitializeStandardLitSurfaceData(input.uv, float2(0,0), input.uv_2, input.uv_3, input.barycentricCoord, input.positionWS, surfaceData, wireframe);
	#else
		InitializeStandardLitSurfaceData(input.uv, input.uv_1, input.uv_2, input.uv_3, float3(0, 0, 0), float3(0, 0, 0), surfaceData, wireframe);
	#endif
#else
	InitializeStandardLitSurfaceData(input.uv, input.uv_1, input.uv_2, input.uv_3, float3(0, 0, 0), float3(0, 0, 0), surfaceData, wireframe);
#endif

#ifdef _AKMU_GEOM_WIREFRAME
	//make invisible surface if wireframe active and basecolor alpha < 1
	if (wireframe <= 0&& surfaceData.alpha < 1)
		discard;
#endif

	float4 reflection = float4(1, 1, 1, 0);
#ifdef _AKMU_MIRROR
	reflection = ProcessMirror(input.uv, input.screenPos, input.eyeIndex, surfaceData, input.distance);
#endif //MIRROR

#if defined(_AKMU_USE_MIRROR_DEPTH) || defined(_AKMU_DECALTEMP) || defined(_AKMU_DECAL)
	//surfaceData.normalTS = WorldNormal.rgb;
#endif //DECAL

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
	half3 flakeNormal = half3(0, 0, 0);
	#if defined (_AKMU_CARPAINT_GEOMETRY)
		float fresnel = carPaintEffects(input.uv, float2(0,0), input.uv_2, input.uv_3, input.positionWS, 
			input.normalWS, input.normalOS, surfaceData, flakeNormal);
	#else
		float fresnel = carPaintEffects(input.uv, input.uv_1, input.uv_2, input.uv_3, input.positionWS,
			input.normalWS, input.normalOS, surfaceData, flakeNormal);
	#endif
#endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _AKMU_MIRROR
	MirrorFresnel(input.normalWS, inputData.viewDirectionWS);
#endif

#if defined(_AKMU_USE_MIRROR_DEPTH) || defined(_AKMU_DECALTEMP) || defined(_AKMU_DECAL)
	inputData.shadowCoord = shadowCoord; //!!!!!!!!!!!!!!!!!!!!!!!!!!
#endif

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
	carPaintApplyColors(surfaceData, fresnel, flakeNormal, input.positionWS, wireframe);
#endif

#ifdef _AKMU_MIRROR
	surfaceData.emission = MirrorEmission(surfaceData, reflection);
#endif

    // Stripped down version of UniversalFragmentPBR().

    // in LitForwardPass GlobalIllumination (and temporarily LightingPhysicallyBased) are called inside UniversalFragmentPBR
    // in Deferred rendering we store the sum of these values (and of emission as well) in the GBuffer
    BRDFData brdfData;
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

#ifdef _AKMU_MIRROR
	surfaceData.emission = MirrorEmission(surfaceData, reflection);
#endif

//todo: do we need to apply decal,livery etc here?

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
    half3 color = GlobalIllumination(brdfData, inputData.bakedGI, surfaceData.occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);

#if defined (_AKMU_MIRROR) && defined (_AKMU_MIRROR_FULL)
	surfaceData.emission = 0; //remove
	color.rgb = MirrorFullColor(color.rgb, reflection);
#endif




//test
// #if defined(_AKMU_USE_MIRROR_DEPTH) || defined(_AKMU_DECALTEMP) || defined(_AKMU_DECAL)
// float2 screenSpaceUV = input.screenPos.xy / input.screenPos.w;
// 	float sceneRawDepth;
// sceneRawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(screenSpaceUV)).r;
// surfaceData.emission = 0;
// color.rgb = 0;//sceneRawDepth;
// surfaceData.alpha = 1;
// #endif



    return BRDFDataToGbuffer(brdfData, inputData, surfaceData.smoothness, surfaceData.emission + color, surfaceData.occlusion);
}

#ifdef _AKMU_CARPAINT_GEOMETRY
[maxvertexcount(24)]
void LitGBufferPassGeometry(triangle Varyings input[3], inout TriangleStream<Varyings> outputStream)
{
//global start !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
#if !defined _AKMU_GEOM_EXTRUDE && !defined _AKMU_GEOM_VOXEL && !defined _AKMU_GEOM_WIREFRAME //they uses their own appends
	Varyings output[3];
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		output[i] = input[i]; //use existing parameters
	}
#endif //global start

#ifdef _AKMU_GEOM_ANIMATION

	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		//calculate new animation position
		output[i].positionWS += _AnimSize * ((sin(_Time.y * _AnimSpeed) * PI * 2) + 1) * _AnimDirection;
		//calculate new camera space pos according to new position
		output[i].positionCS = TransformWorldToHClip(output[i].positionWS);
	}

#endif //animation

#ifdef _AKMU_GEOM_SHRINK

	float3 V[3];
	float3 CG;

	V[0] = output[0].positionWS;
	V[1] = output[1].positionWS;
	V[2] = output[2].positionWS;
	CG = (V[0] + V[1] + V[2]) / 3.;

	[unroll(3)]
	for (int i = 0; i < 3; i++)
	{
		//set new pos
		output[i].positionWS = CG + _ShrinkAmount * (V[i] - CG);
		//set new coord
		output[i].positionCS = TransformWorldToHClip(output[i].positionWS);
	}

#endif //shrink

#ifdef _AKMU_GEOM_LOWPOLY
	float2 uvAvgLowPoly = (output[0].uv + output[1].uv + output[2].uv) / 3;
	float3 normalWSAvgLowPoly = (output[0].normalWS + output[1].normalWS + output[2].normalWS) / 3;
	normalWSAvgLowPoly = normalize(normalWSAvgLowPoly);

	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		if (_SingleColor > 0.5)
		{
			output[i].uv = uvAvgLowPoly; //open if you wanna distort the UV (texture) too
		}
		output[i].uv = uvAvgLowPoly;
		output[i].normalWS = normalWSAvgLowPoly;
	}
#endif //low poly

#ifdef _AKMU_GEOM_EXTRUDE
	Varyings outputExtrude = (Varyings)0;
	Varyings top[3];

	float3 dirExtrude[3];

	[unroll(3)]
	for (int k = 0; k < 3; ++k)
	{
		dirExtrude[k] = input[(k + 1) % 3].positionWS - input[k].positionWS;
	}

	float3 extrudeDir = normalize(cross(dirExtrude[0], -dirExtrude[2]));
#if defined(_EXTRUDEANIMATION_OFF)
	float extrudeAmount = _ExtrudeSize;
#else
	float extrudeAmount = _ExtrudeSize * (sin((gold_noise(input[0].uv, 810) * 2 + _Time.y * _ExtrudeAnimSpeed) * PI * 2) + 1);
#endif

	// Extrude face
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		top[i] = input[i];
		top[i].positionWS = top[i].positionWS + extrudeDir * extrudeAmount;
		top[i].positionCS = TransformWorldToHClip(top[i].positionWS);
		
		outputStream.Append(top[i]);
	}

	outputStream.RestartStrip();

	// Construct sides
	[unroll(3)]
	for (int j = 0; j < 3; ++j)
	{
		half3 normalWS = normalize(cross(dirExtrude[j], extrudeDir));

		outputExtrude = top[(j + 1) % 3];
		outputExtrude.normalWS = normalWS;
		outputStream.Append(outputExtrude);

		outputExtrude = top[j];
		outputExtrude.normalWS = normalWS;
		outputStream.Append(outputExtrude);
		
		outputExtrude = input[(j + 1) % 3];
		outputExtrude.normalWS = normalWS;
		outputStream.Append(outputExtrude);
		
		outputExtrude = input[j];
		outputExtrude.normalWS = normalWS;
		outputStream.Append(outputExtrude);

		outputStream.RestartStrip();
	}
#endif //extrude

#ifdef _AKMU_GEOM_WIREFRAME
Varyings output[3];
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		output[i] = input[i]; //use existing parameters
	}

	InitializeDistanceToEdge(
		output[0].positionWS, output[1].positionWS, output[2].positionWS,
		output[0].barycentricCoord, output[1].barycentricCoord, output[2].barycentricCoord);

	//set new output and restart
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		outputStream.Append(output[i]);
	}

	outputStream.RestartStrip();
#endif //wireframe

#ifdef _AKMU_GEOM_VOXEL
	float minX = min(min(input[0].positionWS.x, input[1].positionWS.x), input[2].positionWS.x);
	float minY = min(min(input[0].positionWS.y, input[1].positionWS.y), input[2].positionWS.y);
	float minZ = min(min(input[0].positionWS.z, input[1].positionWS.z), input[2].positionWS.z);

	float maxX = max(max(input[0].positionWS.x, input[1].positionWS.x), input[2].positionWS.x);
	float maxY = max(max(input[0].positionWS.y, input[1].positionWS.y), input[2].positionWS.y);
	float maxZ = max(max(input[0].positionWS.z, input[1].positionWS.z), input[2].positionWS.z);


	float dis = _VoxelSize / 2;
	float disX = abs(minX - maxX) / 2;
	float disY = abs(minY - maxY) / 2;
	float disZ = abs(minZ - maxZ) / 2;
	//resize them according to step size and add alittle uv based movement to decrease flickering
	//disX = max(dis, disX - disX % dis + dis);// +(input[0].uv.x) / 100;
	//disY = max(dis, disY - disY % dis + dis);// +(input[0].uv.y) / 100;
	//disZ = max(dis, disZ - disZ % dis + dis);// +(input[0].uv.x + input[0].uv.y) / 100;
	//disX = max(dis, disX - disX % dis + dis);// +(input[0].uv.x) / 100;
	//disY = max(dis, disY - disY % dis + dis);// +(input[0].uv.y) / 100;
	//disZ = max(dis, disZ - disZ % dis + dis);// +(input[0].uv.x + input[0].uv.y) / 100;
	disX = max(dis, disX) + (input[0].uv.x + input[1].uv.y) / 100;
	disY = max(dis, disY) + (input[1].uv.x + input[2].uv.y) / 100;
	disZ = max(dis, disZ) + (input[2].uv.x + input[0].uv.y) / 100;

	float3 bar = (float3(minX, minY, minZ) + float3(maxX, maxY, maxZ)) / 2;

	float2 uvAvgVoxel = (input[0].uv + input[1].uv + input[2].uv) / 3;
	float3 normalWSAvgVoxel = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3;
	normalWSAvgVoxel = normalize(normalWSAvgVoxel);
	
	Varyings o1 = input[0];
	Varyings o2 = input[1];
	Varyings o3 = input[2];
	Varyings o4 = input[0];

	//float3 viewDir = normalize(UnityWorldSpaceViewDir(o1.positionWS));
	float normalDivider = 100000;
	{
		//front
		o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(0,0,1); //z+
		float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, disZ) + toAdd, 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, disZ) + toAdd, 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, disZ) + toAdd, 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, disZ) + toAdd, 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip(); 
	}

	{
		//back
		o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(0, 0, -1); //z-
		float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}
	
	{
		//left
		o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(1, 0, 0); //x+
		float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, disZ) + toAdd, 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, disZ) + toAdd, 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//right
		o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(-1, 0, 0); //x-
		float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, disZ) + toAdd, 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, disZ) + toAdd, 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}
	
	{
		//top
		o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(0, 1, 0); //y+
		float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, disZ) + toAdd, 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, disZ) + toAdd, 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	
	{
		//bottom
		o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(0, -1, 0); //y-
		float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, -disZ) + toAdd, 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, disZ) + toAdd, 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, disZ) + toAdd, 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}
#endif //voxel

//global end!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
#if !defined _AKMU_GEOM_EXTRUDE && !defined _AKMU_GEOM_VOXEL && !defined _AKMU_GEOM_WIREFRAME //they uses their own appends
	//set new output and restart
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		outputStream.Append(output[i]);
	}

	outputStream.RestartStrip();
#endif //global end
}
#endif //akmu geometry


#endif
