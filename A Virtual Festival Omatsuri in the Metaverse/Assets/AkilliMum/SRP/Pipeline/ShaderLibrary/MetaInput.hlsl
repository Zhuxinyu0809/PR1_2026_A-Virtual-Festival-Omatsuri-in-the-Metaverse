#ifndef UNIVERSAL_META_INPUT_INCLUDED
#define UNIVERSAL_META_INPUT_INCLUDED

#include "../../Pipeline/Core/ShaderLibrary/MetaPass.hlsl"
#include "../../Pipeline/ShaderLibrary/Lighting.hlsl"
#include "../../Pipeline/Core/ShaderLibrary/Color.hlsl"

#define MetaInput UnityMetaInput
#define MetaFragment UnityMetaFragment
float4 MetaVertexPosition(float4 positionOS, float2 uv1, float2 uv2, float4 uv1ST, float4 uv2ST)
{
    return UnityMetaVertexPosition(positionOS.xyz, uv1, uv2, uv1ST, uv2ST);
}
#endif
