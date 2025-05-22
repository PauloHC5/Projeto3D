// Automatically patched for procedural instancing with Nature Renderer: https://visualdesigncafe.com/nature-renderer
Shader "Universal Render Pipeline/Nature/SpeedTree7 Billboard (Nature Renderer)"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _HueVariation("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.333
        [MaterialEnum(None,0,Fastest,1)] _WindQuality("Wind Quality", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
            "RenderType" = "TransparentCutout"
            "DisableBatching" = "LODFading"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "SimpleLit"
         "NatureRendererInstancing"="True" 
}
        LOD 400

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward"  "NatureRendererInstancing"="True" }

            HLSLPROGRAM

            #pragma vertex SpeedTree7Vert
            #pragma fragment SpeedTree7Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _FORWARD_PLUS
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #pragma multi_compile __ BILLBOARD_FACE_CAMERA_POS
            #pragma multi_compile __ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            #pragma shader_feature_local EFFECT_BUMP
            #pragma shader_feature_local EFFECT_HUE_VARIATION
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:SetupNatureRenderer forwardadd renderinglayer

            #define ENABLE_WIND

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardPasses.hlsl"
            #include "Assets/Visual Design Cafe/Nature Renderer/Shader Includes/Nature Renderer.templatex"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster" "NatureRendererInstancing"="True" }

            ColorMask 0

            HLSLPROGRAM

            #pragma vertex SpeedTree7VertDepth
            #pragma fragment SpeedTree7FragDepth

            #pragma multi_compile __ BILLBOARD_FACE_CAMERA_POS
            #pragma multi_compile __ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:SetupNatureRenderer forwardadd renderinglayer

            #define ENABLE_WIND
            #define DEPTH_ONLY
            #define SHADOW_CASTER

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardPasses.hlsl"
            #include "Assets/Visual Design Cafe/Nature Renderer/Shader Includes/Nature Renderer.templatex"

            ENDHLSL
        }

        Pass
        {
            Name "GBuffer"
            Tags{"LightMode" = "UniversalGBuffer" "NatureRendererInstancing"="True" }

            HLSLPROGRAM
            #pragma target 4.5

            // Deferred Rendering Path does not support the OpenGL-based graphics API:
            // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
            #pragma exclude_renderers gles3 glcore

            #pragma vertex SpeedTree7Vert
            #pragma fragment SpeedTree7Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile __ BILLBOARD_FACE_CAMERA_POS
            #pragma multi_compile __ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

            #pragma shader_feature_local EFFECT_BUMP
            #pragma shader_feature_local EFFECT_HUE_VARIATION
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:SetupNatureRenderer forwardadd renderinglayer

            #define ENABLE_WIND
            #define GBUFFER

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardPasses.hlsl"
            #include "Assets/Visual Design Cafe/Nature Renderer/Shader Includes/Nature Renderer.templatex"

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly" "NatureRendererInstancing"="True" }

            ColorMask R

            HLSLPROGRAM

            #pragma vertex SpeedTree7VertDepth
            #pragma fragment SpeedTree7FragDepth

            #pragma multi_compile __ BILLBOARD_FACE_CAMERA_POS
            #pragma multi_compile __ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:SetupNatureRenderer forwardadd renderinglayer

            #define ENABLE_WIND
            #define DEPTH_ONLY

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardPasses.hlsl"
            #include "Assets/Visual Design Cafe/Nature Renderer/Shader Includes/Nature Renderer.templatex"

            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals" "NatureRendererInstancing"="True" }

            ColorMask R

            HLSLPROGRAM

            #pragma vertex SpeedTree7VertDepthNormalBillboard
            #pragma fragment SpeedTree7FragDepthNormalBillboard

            #pragma shader_feature_local EFFECT_BUMP
            #pragma multi_compile __ BILLBOARD_FACE_CAMERA_POS
            #pragma multi_compile __ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:SetupNatureRenderer forwardadd renderinglayer

            #define ENABLE_WIND

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Nature/SpeedTree7BillboardPasses.hlsl"
            #include "Assets/Visual Design Cafe/Nature Renderer/Shader Includes/Nature Renderer.templatex"

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
