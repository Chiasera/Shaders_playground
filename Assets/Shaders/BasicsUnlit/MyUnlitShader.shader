Shader "Unlit/MyUnlitShader"
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Packages/com.unity.render_pipelines.core/ShaderLibrary/Common.hlsl"
            
            ENDHLSL
        }
    }
}
