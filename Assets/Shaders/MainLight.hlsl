#include "UnityCG.cginc"

#ifndef MAINLIGHT_INCLUDED
#define MAINLIGHT_INCLUDED

void MainLight_float(in float3 WorldPos, out half3 Diretion, out half3 Color, out half DistanceAttenuation, out half ShadowAtten)
{
    #ifdef SHADERGRAPH_PREVIEW 
        LightDirection = half3(0,1,0);
        Color = 1;
        DistanceAttenuation = 1;
        ShadowAttenuation = 1;
    #else
        #if SHADOWS_SCREEN
            half4 clipPos = TransformWorldToHClip(WorldPosition);
            half4 shadowCoord = ComputeScreenPos(clipPos);
        #else
        half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        #endif
    #endif
}

#endif