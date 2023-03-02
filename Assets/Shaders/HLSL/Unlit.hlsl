#ifndef MYRP_UNLIT_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

cbuffer UnityPerFrame
{
    float4x4 unity_MatrixVP;
};

cbuffer UnityPerDraw
{
    float4x4 unity_ObjectToWorld;
}

struct VertexInput
{
    float4 pos : POSITION;
};

struct VertexOutput
{
    float4 clipPos : SV_POSITION;
};

VertexOutput UnlitPassVertex(VertexInput input)
{
    VertexOutput output;
    float4 worldPos = mul(unity_ObjectToWorld, input.pos); //convert input in object space to world space
    output.clipPos = mul(unity_MatrixVP, worldPos);
    return output;
}

float4 UnlitPassFragment(VertexOutput input) : SV_TARGET
{
    return float4(1,1,0,1);
}


#endif // MYRP_UNLIT_INCLUDED