#ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
#define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED

#include "../../Pipeline/ShaderLibrary/Core.hlsl"
#include "../../Pipeline/ShaderLibrary/Shadows.hlsl"
#include "../../Pipeline/ShaderLibrary/CommonOperations.hlsl"
#if defined(LOD_FADE_CROSSFADE)
#include "../../Pipeline/ShaderLibrary/LODCrossFade.hlsl"
#endif
#if defined (_AKMU_CARPAINT) || defined (_AKMU_CARPAINT_GEOMETRY)
#include "../../CarPaint/Shader/CarPaintDefinitions.hlsl"
#endif
#if defined (_AKMU_CARPAINT_GEOMETRY)
#include "../../Geom/Shader/GeometryDefinitions.hlsl"
#endif

// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
	float3 positionWS               : TEXCOORD1;
#ifdef _AKMU_CARPAINT_GEOMETRY
	float3 barycentricCoord         : TEXCOORD2;
#endif
	//UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsToGeometry
{
	float3 positionWS   : TEXCOORD0;
	float3 normalWS     : TEXCOORD1;
	float2 texcoord     : TEXCOORD2;
	//UNITY_VERTEX_INPUT_INSTANCE_ID
};

VaryingsToGeometry ShadowPassVertexMine(Attributes input)
{
	VaryingsToGeometry output;
	UNITY_SETUP_INSTANCE_ID(input);

	output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
	output.normalWS = TransformObjectToWorldNormal(input.normalOS);
	output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);

	return output;
}

float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}
float4 GetShadowPositionHClip(float3 positionWS, float3 normalWS)
{
	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
	positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

	return positionCS;
}


Varyings ShadowPassVertex(Attributes input)
{
	Varyings output = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(input);

	output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
	output.positionCS = 0;
	output.positionCS = GetShadowPositionHClip(input);
	output.positionWS = 0;
	output.positionWS = TransformObjectToWorld(input.positionOS.xyz);


#ifdef _AKMU_CARPAINT_GEOMETRY
	output.barycentricCoord = 0;
#endif

	return output;
}

#ifdef _AKMU_CARPAINT_GEOMETRY

[maxvertexcount(24)]
void ShadowPassGeometry(triangle VaryingsToGeometry input[3], inout TriangleStream<Varyings> outputStream)
{
//global values for each stream
#if !defined _AKMU_GEOM_EXTRUDE && !defined _AKMU_GEOM_VOXEL && !defined _AKMU_GEOM_WIREFRAME //they uses their own appends
	Varyings output[3];

	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		output[i] = (Varyings)0;
		//use input values as default
		output[i].positionWS = input[i].positionWS;
		output[i].positionCS = GetShadowPositionHClip(input[i].positionWS, input[i].normalWS);
		output[i].uv = input[i].texcoord;
	}
#endif //global start

#ifdef _AKMU_GEOM_ANIMATION

	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		//calculate new animation position
		output[i].positionWS += _AnimSize * ((sin(_Time.y * _AnimSpeed) * PI * 2) + 1) * _AnimDirection;
		//calculate shadow pos according to new position
		output[i].positionCS = GetShadowPositionHClip(output[i].positionWS, input[i].normalWS);
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
		//set new shadow coord
		output[i].positionCS = GetShadowPositionHClip(output[i].positionWS, input[i].normalWS);
	}

#endif //shrink

#ifdef _AKMU_GEOM_LOWPOLY
	//low poly does not change anything on shadows!
#endif //low poly

#ifdef _AKMU_GEOM_EXTRUDE
	Varyings outputExtrude = (Varyings)0;
	VaryingsToGeometry top[3];

	float3 dirExtrude[3];

	[unroll(3)]
	for (int k = 0; k < 3; ++k)
	{
		dirExtrude[k] = input[(k + 1) % 3].positionWS.xyz - input[k].positionWS.xyz;
	}

	float3 extrudeDirWS = normalize(cross(dirExtrude[0], -dirExtrude[2]));
#if defined(_EXTRUDEANIMATION_OFF)
	float extrudeAmount = _ExtrudeSize;
#else
	float extrudeAmount = _ExtrudeSize * (sin((gold_noise(input[0].texcoord, 810) * 2 + _Time.y * _ExtrudeAnimSpeed) * PI * 2) + 1);
#endif

	//extrude face
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		top[i] = input[i];
		top[i].positionWS.xyz += extrudeDirWS * extrudeAmount;

		outputExtrude.uv = top[i].texcoord;
		outputExtrude.positionCS = GetShadowPositionHClip(top[i].positionWS, top[i].normalWS);

		outputStream.Append(outputExtrude);
	}

	outputStream.RestartStrip();

	//constract sides
	[unroll(3)]
	for (int j = 0; j < 3; ++j)
	{
		float3 sideNormalWS = normalize(cross(dirExtrude[j], extrudeDirWS));

		outputExtrude.uv = top[(j + 1) % 3].texcoord;
		outputExtrude.positionWS = top[(j + 1) % 3].positionWS;
		outputExtrude.positionCS = GetShadowPositionHClip(outputExtrude.positionWS, sideNormalWS);
		outputStream.Append(outputExtrude);

		outputExtrude.uv = top[j].texcoord;
		outputExtrude.positionWS = top[j].positionWS;
		outputExtrude.positionCS = GetShadowPositionHClip(outputExtrude.positionWS, sideNormalWS);
		outputStream.Append(outputExtrude);

		outputExtrude.uv = input[(j + 1) % 3].texcoord;
		outputExtrude.positionWS = input[(j + 1) % 3].positionWS;
		outputExtrude.positionCS = GetShadowPositionHClip(outputExtrude.positionWS, sideNormalWS);
		outputStream.Append(outputExtrude);

		outputExtrude.uv = input[j].texcoord;
		outputExtrude.positionWS = input[j].positionWS;
		outputExtrude.positionCS = GetShadowPositionHClip(outputExtrude.positionWS, sideNormalWS);
		outputStream.Append(outputExtrude);

		outputStream.RestartStrip();
	}
#endif //extrude

#ifdef _AKMU_GEOM_WIREFRAME
#ifdef _WIREFRAME_SHADOW_ONLY
	Varyings output[3];

	//use this normal to extend the vertices to create them on same plane
	float3 normalWSAvg = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3;

	//calculate directions
	float3 dirWireframe[3];
	[unroll(3)]
	for (int k = 0; k < 3; ++k)
	{
		dirWireframe[k] = input[(k + 1) % 3].positionWS.xyz - input[k].positionWS.xyz;
	}
	//------------------------------------------------------------------------------------------------------------------------
	//for 0-1-2, use 0-1 as static and move the 2 to the 0 then 1 respectively
	output[0] = (Varyings)0;
	output[0].positionWS = input[0].positionWS;
	output[0].positionCS = GetShadowPositionHClip(output[0].positionWS, normalWSAvg);
	//
	output[1] = (Varyings)0;
	output[1].positionWS = input[1].positionWS;
	output[1].positionCS = GetShadowPositionHClip(output[1].positionWS, normalWSAvg);
	// pass 2 to 0
	output[2] = (Varyings)0;
	output[2].positionWS = input[0].positionWS - dirWireframe[2] * _WireframeShadowSize;
	output[2].positionCS = GetShadowPositionHClip(output[2].positionWS, normalWSAvg);
	
	outputStream.Append(output[0]);
	outputStream.Append(output[1]);
	outputStream.Append(output[2]);

	// pass 2 to 1
	output[2] = (Varyings)0;
	output[2].positionWS = input[1].positionWS + dirWireframe[1] * _WireframeShadowSize;
	output[2].positionCS = GetShadowPositionHClip(output[2].positionWS, normalWSAvg);

	outputStream.Append(output[0]);
	outputStream.Append(output[1]);
	outputStream.Append(output[2]);

	outputStream.RestartStrip();

	//------------------------------------------------------------------------------------------------------------------------
	//for 1-2-0, use 1-2 as static and move the 0 to the 1 then 2 respectively
	output[1] = (Varyings)0;
	output[1].positionWS = input[1].positionWS;
	output[1].positionCS = GetShadowPositionHClip(output[1].positionWS, normalWSAvg);
	
	output[2] = (Varyings)0;
	output[2].positionWS = input[2].positionWS;
	output[2].positionCS = GetShadowPositionHClip(output[2].positionWS, normalWSAvg);
	//pass 0 to 1
	output[0] = (Varyings)0;
	output[0].positionWS = input[1].positionWS - dirWireframe[0] * _WireframeShadowSize;
	output[0].positionCS = GetShadowPositionHClip(output[0].positionWS, normalWSAvg);

	outputStream.Append(output[0]);
	outputStream.Append(output[1]);
	outputStream.Append(output[2]);

	//pass 0 to 2
	output[0] = (Varyings)0;
	output[0].positionWS = input[2].positionWS + dirWireframe[2] * _WireframeShadowSize;
	output[0].positionCS = GetShadowPositionHClip(output[0].positionWS, normalWSAvg);

	outputStream.Append(output[0]);
	outputStream.Append(output[1]);
	outputStream.Append(output[2]);

	outputStream.RestartStrip();
#if !defined _WIREFRAMEQUAD_ON
	//------------------------------------------------------------------------------------------------------------------------
	//for 2-0-1, use 2-0 as static and move the 1 to the 2 then 0 respectively
	output[2] = (Varyings)0;
	output[2].positionWS = input[2].positionWS;
	output[2].positionCS = GetShadowPositionHClip(output[2].positionWS, normalWSAvg);
	//
	output[0] = (Varyings)0;
	output[0].positionWS = input[0].positionWS;
	output[0].positionCS = GetShadowPositionHClip(output[0].positionWS, normalWSAvg);
	// pass 1 to 0
	output[1] = (Varyings)0;
	output[1].positionWS = input[0].positionWS + dirWireframe[0] * _WireframeShadowSize;
	output[1].positionCS = GetShadowPositionHClip(output[1].positionWS, normalWSAvg);

	outputStream.Append(output[0]);
	outputStream.Append(output[1]);
	outputStream.Append(output[2]);

	// pass 1 to 2
	output[1] = (Varyings)0;
	output[1].positionWS = input[2].positionWS + dirWireframe[1] * _WireframeShadowSize;
	output[1].positionCS = GetShadowPositionHClip(output[1].positionWS, normalWSAvg);

	outputStream.Append(output[0]);
	outputStream.Append(output[1]);
	outputStream.Append(output[2]);

	outputStream.RestartStrip();
#endif //quad
#else
	Varyings output[3];

	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		output[i] = (Varyings)0;
		//use input values as default
		output[i].positionWS = input[i].positionWS;
		output[i].positionCS = GetShadowPositionHClip(input[i].positionWS, input[i].normalWS);
		output[i].uv = input[i].texcoord;
	}

	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		outputStream.Append(output[i]);
	}

	outputStream.RestartStrip();
#endif //shadow only
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
	disX = max(dis, disX) + (input[0].texcoord.x + input[1].texcoord.y) / 100;
	disY = max(dis, disY) + (input[1].texcoord.x + input[2].texcoord.y) / 100;
	disZ = max(dis, disZ) + (input[2].texcoord.x + input[0].texcoord.y) / 100;

	float3 bar = (float3(minX, minY, minZ) + float3(maxX, maxY, maxZ)) / 2;

	//float2 uvAvgVoxel = (input[0].texcoord + input[1].texcoord + input[2].texcoord) / 3;
	/*float3 normalWSAvgVoxel = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3;
	normalWSAvgVoxel = normalize(normalWSAvgVoxel);*/

	Varyings o1 = (Varyings)0;
	o1.uv = input[0].texcoord;
	Varyings o2 = (Varyings)0;
	o2.uv = input[1].texcoord;
	Varyings o3 = (Varyings)0;
	o3.uv = input[2].texcoord;
	Varyings o4 = (Varyings)0;
	o4.uv = input[0].texcoord;

	//float3 viewDir = normalize(UnityWorldSpaceViewDir(o1.positionWS));
	//float normalDivider = 100000;
	{
		//front
		float3 normal = float3(0, 0, 1); //z+
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionWS = float3(bar + float3(-disX, -disY, disZ));
		o1.positionCS = GetShadowPositionHClip(o1.positionWS, normal);
		outputStream.Append(o1);
		o2.positionWS = float3(bar + float3(disX, -disY, disZ));
		o2.positionCS = GetShadowPositionHClip(o2.positionWS, normal);
		outputStream.Append(o2);
		o3.positionWS = float3(bar + float3(-disX, disY, disZ));
		o3.positionCS = GetShadowPositionHClip(o3.positionWS, normal);
		outputStream.Append(o3);
		o4.positionWS = float3(bar + float3(disX, disY, disZ));
		o4.positionCS = GetShadowPositionHClip(o4.positionWS, normal);
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//back
		float3 normal = float3(0, 0, -1); //z-
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionWS = float3(bar + float3(-disX, disY, -disZ));
		o1.positionCS = GetShadowPositionHClip(o1.positionWS, normal);
		outputStream.Append(o1);
		o2.positionWS = float3(bar + float3(disX, disY, -disZ));
		o2.positionCS = GetShadowPositionHClip(o2.positionWS, normal);
		outputStream.Append(o2);
		o3.positionWS = float3(bar + float3(-disX, -disY, -disZ));
		o3.positionCS = GetShadowPositionHClip(o3.positionWS, normal);
		outputStream.Append(o3);
		o4.positionWS = float3(bar + float3(disX, -disY, -disZ));
		o4.positionCS = GetShadowPositionHClip(o4.positionWS, normal);
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//left
		float3 normal = float3(1, 0, 0); //x+
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionWS = float3(bar + float3(disX, disY, disZ));
		o1.positionCS = GetShadowPositionHClip(o1.positionWS, normal);
		outputStream.Append(o1);
		o2.positionWS = float3(bar + float3(disX, -disY, disZ));
		o2.positionCS = GetShadowPositionHClip(o2.positionWS, normal);
		outputStream.Append(o2);
		o3.positionWS = float3(bar + float3(disX, disY, -disZ));
		o3.positionCS = GetShadowPositionHClip(o3.positionWS, normal);
		outputStream.Append(o3);
		o4.positionWS = float3(bar + float3(disX, -disY, -disZ));
		o4.positionCS = GetShadowPositionHClip(o4.positionWS, normal);
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//right
		float3 normal = float3(-1, 0, 0); //x-
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionWS = float3(bar + float3(-disX, disY, -disZ));
		o1.positionCS = GetShadowPositionHClip(o1.positionWS, normal);
		outputStream.Append(o1);
		o2.positionWS = float3(bar + float3(-disX, -disY, -disZ));
		o2.positionCS = GetShadowPositionHClip(o2.positionWS, normal);
		outputStream.Append(o2);
		o3.positionWS = float3(bar + float3(-disX, disY, disZ));
		o3.positionCS = GetShadowPositionHClip(o3.positionWS, normal);
		outputStream.Append(o3);
		o4.positionWS = float3(bar + float3(-disX, -disY, disZ));
		o4.positionCS = GetShadowPositionHClip(o4.positionWS, normal);
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//top
		float3 normal = float3(0, 1, 0); //y+
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionWS = float3(bar + float3(disX, disY, -disZ));
		o1.positionCS = GetShadowPositionHClip(o1.positionWS, normal);
		outputStream.Append(o1);
		o2.positionWS = float3(bar + float3(-disX, disY, -disZ));
		o2.positionCS = GetShadowPositionHClip(o2.positionWS, normal);
		outputStream.Append(o2);
		o3.positionWS = float3(bar + float3(disX, disY, disZ));
		o3.positionCS = GetShadowPositionHClip(o3.positionWS, normal);
		outputStream.Append(o3);
		o4.positionWS = float3(bar + float3(-disX, disY, disZ));
		o4.positionCS = GetShadowPositionHClip(o4.positionWS, normal);
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//bottom
		float3 normal = float3(0, -1, 0); //y-
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionWS = float3(bar + float3(-disX, -disY, -disZ));
		o1.positionCS = GetShadowPositionHClip(o1.positionWS, normal);
		outputStream.Append(o1);
		o2.positionWS = float3(bar + float3(disX, -disY, -disZ));
		o2.positionCS = GetShadowPositionHClip(o2.positionWS, normal);
		outputStream.Append(o2);
		o3.positionWS = float3(bar + float3(-disX, -disY, disZ));
		o3.positionCS = GetShadowPositionHClip(o3.positionWS, normal);
		outputStream.Append(o3);
		o4.positionWS = float3(bar + float3(disX, -disY, disZ));
		o4.positionCS = GetShadowPositionHClip(o4.positionWS, normal);
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}
#endif //voxel

#if !defined _AKMU_GEOM_EXTRUDE && !defined _AKMU_GEOM_VOXEL && !defined _AKMU_GEOM_WIREFRAME //they uses their own appends
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		outputStream.Append(output[i]);
	}

	outputStream.RestartStrip();
#endif //global end
}
#endif //akmu geometry

#ifndef _AKMU_CARPAINT
half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

    return 0;
}
#endif

#ifdef _AKMU_CARPAINT
half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
	half4 albedoAlpha = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));

#ifdef _AKMU_CARPAINT_GEOMETRY
	half wireframe = 0;
#ifdef _AKMU_GEOM_WIREFRAME
		wireframe = saturate(WireframeBS(input.barycentricCoord, _WireframeSize));
#endif

#ifdef _AKMU_GEOM_WIREFRAME
		//Alpha(min(albedoAlpha.a, wireframe), _BaseColor, _Cutoff);
		float a = Alpha(wireframe, 0, _Cutoff);
		if (a <= 0)
			Alpha(0, 0, _Cutoff);
#else
		Alpha(albedoAlpha, _BaseColor, _Cutoff);
#endif
#else
	Alpha(albedoAlpha, _BaseColor, _Cutoff);
#endif

#ifdef LOD_FADE_CROSSFADE
	LODFadeCrossFade(input.positionCS);
#endif    

	return 0;
}
#endif

#endif
