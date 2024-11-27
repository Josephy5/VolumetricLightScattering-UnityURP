Shader "Hidden/Godray"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
        }
        
        Pass
        {
            Name "Volumetric Scattering/God Rays"
            
            Cull Off ZWrite Off ZTest Always Blend One Zero
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma target 4.5
            
            #pragma multi_compile _ _SCHLICK _HENYEY_GREENSTEIN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Godrays/Helpers.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Scattering;
            float _SigmaS;
            float _SigmaT;
            float _Intensity;
            float _Steps;
            float _MaxDistance;
            float _Jitter;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };
            //get data from UVs and the scene
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                
                return output;
            }

            //helper method to contain our conditions for phase functions
            float phaseFunction(float LoV, float g)
            {
                #if defined(_HENYEY_GREENSTEIN)
                    return HenyeyGreenStein(LoV, g);
                #else
                    float k = 1.55f * g - 0.55f * g * g * g;
                    return Schlick(LoV, k);
                #endif
            }

            //method for ray marching on main directional light 
            float3 rayMarchMainLight(float3 startPosition, float3 step, float2 uv)
            {
                float3 rayDirection = normalize(step);
                float stepLength = length(step);
                // Offset the start position to avoid band artifact (convert to noise and we can blur in later stage)
                float3 currentPosition = startPosition + rand(uv) * step * _Jitter;
                Light light = GetMainLight();
                float3 accumFog = 0;
                float transmittance = 1.0;
                
                //precompute first part of formula S, relating to calculating light color intensity
                float3 S1 = light.color * _Intensity * _SigmaS;

                // The part where we use Ray marching
                for (int i=0; i<_Steps; ++i)
                {
                    // calculate the volumetric light scattering through the formulas for ray marching technique used in Frostbite engine
                    // See slide 28 at http://www.frostbite.com/2015/08/physically-based-unified-volumetric-rendering-in-frostbite/
                    float phaseValue = phaseFunction(dot(rayDirection, -light.direction), _Scattering);
                    float shadowAtten = MainLightShadowAtten(currentPosition);
                    float3 S = S1 * phaseValue * shadowAtten;
                    //float3 S = S1 * phaseFunction(dot(rayDirection, -light.direction), _Scattering) * MainLightShadowAtten(currentPosition); old code
                    float3 Sint = (S - S * exp(-_SigmaT * stepLength)) / _SigmaT;
                    
                    accumFog += transmittance * Sint;
                    transmittance *= exp(-_SigmaT * stepLength);
                    currentPosition += step;
                }
                return accumFog;
            }

            //get all the calculations needed to create volumetric fog via ray marching
            float4 frag(Varyings input) : SV_Target
            {
                float3 worldPos = GetWorldPosFromUV(input.uv);
                float3 startPosition = _WorldSpaceCameraPos;

                //calculate the world space light rays
                float3 rayVector = worldPos - startPosition;
                float3 rayDirection = normalize(rayVector);
                float rayLength = min(length(rayVector), _MaxDistance);

                float stepLength = rayLength / _Steps;
                float3 step = rayDirection * stepLength;

                //calcluate the raymarch using the main light from the origin position and input UVs
                float3 accumFog = rayMarchMainLight(startPosition, step, input.uv);
                
                return float4(accumFog, 1.0);
            }
            ENDHLSL
        }
        
        //apply gaussian blur
        UsePass "Hidden/Gaussian_Blur_X/GAUSSIAN_BLUR_X"
        UsePass "Hidden/Gaussian_Blur_Y/GAUSSIAN_BLUR_Y"
    }
}
