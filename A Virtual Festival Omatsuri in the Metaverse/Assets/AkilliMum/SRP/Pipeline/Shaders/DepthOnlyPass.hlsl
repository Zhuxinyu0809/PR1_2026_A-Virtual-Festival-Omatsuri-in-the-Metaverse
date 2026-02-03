#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED

#include "../../Pipeline/ShaderLibrary/Core.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "../../Pipeline/ShaderLibrary/LODCrossFade.hlsl"
#endif

struct Attributes
{
    float4 position     : POSITION;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsToGeometry
{
	float3 positionWS   : TEXCOORD0;
	float2 texcoord     : TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings DepthOnlyVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionCS = TransformObjectToHClip(input.position.xyz);
    return output;
}

VaryingsToGeometry DepthOnlyVertexMine(Attributes input)
{
	VaryingsToGeometry output = (VaryingsToGeometry)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

	output.positionWS = TransformObjectToWorld(input.position.xyz);
	output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);

	return output;
}

#ifdef _AKMU_CARPAINT_GEOMETRY

[maxvertexcount(24)]
void DepthOnlyGeometry(triangle VaryingsToGeometry input[3], inout TriangleStream<Varyings> outputStream)
{
	//global values for each stream
#if !defined _AKMU_GEOM_EXTRUDE && !defined _AKMU_GEOM_VOXEL && !defined _AKMU_GEOM_WIREFRAME //they uses their own appends
	Varyings output[3];

	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		output[i] = (Varyings)0;
		//use input values as default
		output[i].positionCS = TransformWorldToHClip(input[i].positionWS);
		output[i].uv = input[i].texcoord;
	}
#endif //global start

#ifdef _AKMU_GEOM_ANIMATION
	//Varyings output[3];
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		//calculate new animation position
		input[i].positionWS += _AnimSize * ((sin(_Time.y * _AnimSpeed) * PI * 2) + 1) * _AnimDirection;
		//calculate shadow pos according to new position
		output[i].positionCS = TransformWorldToHClip(input[i].positionWS);
	}

#endif //animation

#ifdef _AKMU_GEOM_SHRINK

	float3 V[3];
	float3 CG;

	V[0] = input[0].positionWS;
	V[1] = input[1].positionWS;
	V[2] = input[2].positionWS;
	CG = (V[0] + V[1] + V[2]) / 3.;

	[unroll(3)]
	for (int i = 0; i < 3; i++)
	{
		//set new pos
		input[i].positionWS = CG + _ShrinkAmount * (V[i] - CG);
		//set new shadow coord
		output[i].positionCS = TransformWorldToHClip(input[i].positionWS);
	}

#endif //shrink

#ifdef _AKMU_GEOM_LOWPOLY
	//low poly does not change anything on depth!
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

	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		top[i] = input[i];
		top[i].positionWS.xyz += extrudeDirWS * extrudeAmount;

		outputExtrude.uv = top[i].texcoord;
		outputExtrude.positionCS = TransformWorldToHClip(top[i].positionWS);

		outputStream.Append(outputExtrude);
	}

	outputStream.RestartStrip();

	[unroll(3)]
	for (int j = 0; j < 3; ++j)
	{
		float3 sideNormalWS = normalize(cross(dirExtrude[j], extrudeDirWS));

		outputExtrude.uv = top[(j + 1) % 3].texcoord;
		outputExtrude.positionCS = TransformWorldToHClip(top[(j + 1) % 3].positionWS);
		outputStream.Append(outputExtrude);

		outputExtrude.uv = top[j].texcoord;
		outputExtrude.positionCS = TransformWorldToHClip(top[j].positionWS);
		outputStream.Append(outputExtrude);

		outputExtrude.uv = input[(j + 1) % 3].texcoord;
		outputExtrude.positionCS = TransformWorldToHClip(input[(j + 1) % 3].positionWS);
		outputStream.Append(outputExtrude);

		outputExtrude.uv = input[j].texcoord;
		outputExtrude.positionCS = TransformWorldToHClip(input[j].positionWS);
		outputStream.Append(outputExtrude);

		outputStream.RestartStrip();
	}
#endif //extrude

#ifdef _AKMU_GEOM_WIREFRAME
	//change depth on wireframe? //todo
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
		//o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(0, 0, 1); //z+
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, disZ), 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, disZ), 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, disZ), 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, disZ), 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//back
		//o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(0, 0, -1); //z-
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, -disZ), 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, -disZ), 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, -disZ), 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, -disZ), 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//left
		//o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(1, 0, 0); //x+
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, disZ), 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, disZ), 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, -disZ), 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, -disZ), 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//right
		//o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(-1, 0, 0); //x-
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, -disZ), 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, -disZ), 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, disZ), 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, disZ), 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}

	{
		//top
		//o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(0, 1, 0); //y+
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, -disZ), 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, -disZ), 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, disY, disZ), 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, disY, disZ), 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}


	{
		//bottom
		//o1.normalWS = o2.normalWS = o3.normalWS = o3.normalWS = float3(0, -1, 0); //y-
		//float3 toAdd = o1.normalWS / normalDivider; //a little bit movement to decrease z-fight
		o1.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, -disZ), 1.0));
		outputStream.Append(o1);
		o2.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, -disZ), 1.0));
		outputStream.Append(o2);
		o3.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(-disX, -disY, disZ), 1.0));
		outputStream.Append(o3);
		o4.positionCS = mul(UNITY_MATRIX_VP, float4(bar + float3(disX, -disY, disZ), 1.0));
		outputStream.Append(o4);

		outputStream.RestartStrip();
	}
#endif //voxel

#if !defined _AKMU_GEOM_EXTRUDE && !defined _AKMU_GEOM_VOXEL && !defined _AKMU_GEOM_WIREFRAME //they uses their own appends
#endif //global end
}
#endif //akmu geometry

half DepthOnlyFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

    return input.positionCS.z;
}
#endif
