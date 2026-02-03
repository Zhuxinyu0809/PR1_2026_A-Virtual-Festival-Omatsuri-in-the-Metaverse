#ifndef UNIVERSAL_LIT_META_PASS_INCLUDED
#define UNIVERSAL_LIT_META_PASS_INCLUDED

#include "../../Pipeline/ShaderLibrary/UniversalMetaPass.hlsl"

half4 UniversalFragmentMetaLit(Varyings input) : SV_Target
{
    SurfaceData surfaceData;
	half wireframe;
#ifdef _AKMU_CARPAINT
	#ifdef _AKMU_CARPAINT_GEOMETRY
	InitializeStandardLitSurfaceData(input.uv, float2(0, 0), float2(0, 0), float2(0, 0), float3(0, 0, 0), float3(0, 0, 0), surfaceData, wireframe);
	#else
	InitializeStandardLitSurfaceData(input.uv, float2(0, 0), float2(0, 0), float2(0, 0), float3(0, 0, 0), float3(0, 0, 0), surfaceData, wireframe);
	#endif
#else
	InitializeStandardLitSurfaceData(input.uv, float2(0, 0), float2(0, 0), float2(0, 0), float3(0, 0, 0), float3(0, 0, 0), surfaceData, wireframe);
#endif 

    BRDFData brdfData;
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    MetaInput metaInput;
    metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
    metaInput.Emission = surfaceData.emission;
    return UniversalFragmentMeta(input, metaInput);
}
#endif
