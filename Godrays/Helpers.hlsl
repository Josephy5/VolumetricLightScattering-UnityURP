#ifndef HELPERS_HLSL
#define HELPERS_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

// helper method for calculating random hashes
float rand(float2 p){
    // this formula below is from https://www.shadertoy.com/view/4djSRW
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); 
}
// get the shadows from the main light
float MainLightShadowAtten(float3 worldPosition)
{
    return MainLightRealtimeShadow(TransformWorldToShadowCoord(worldPosition));
}
// get the world position from the UV
float3 GetWorldPosFromUV(float2 uv)
{
    #if UNITY_REVERSED_Z
        float depth = SampleSceneDepth(uv);
    #else
        // Adjust Z to match NDC for OpenGL ([-1, 1])
        float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
    #endif
                
    //return results of our world position
    float3 worldPos = ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
    return worldPos;
}
// helper method for Henyey GreenStein formula
float HenyeyGreenStein(float LoV, float g)
{
    float result = 1.0f - g * g;
    //added abs to prevent undesirable calculations like calculating with 0
    result /= 4.0f * PI * pow(abs(1.0f + g * g - 2.0f * g * LoV), 1.5f);
    return result;
}
// helper method for Schlick formula
float Schlick(float LoV, float k)
{
    return (1.0f - k * k) / (4.0 * PI * pow(1.0f + k * LoV, 2.0f));
}
#endif