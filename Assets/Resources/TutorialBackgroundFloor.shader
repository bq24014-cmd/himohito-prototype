Shader "HimoHito/Tutorial Background Floor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Floor texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="False" }
        Cull Off ZWrite Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment FloorFrag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            float4 _MainTex_TexelSize;
            fixed4 FloorFrag(v2f input) : SV_Target
            {
                float2 uv = input.texcoord;
                uv.x = min(uv.x, 1 - 1.5 * _MainTex_TexelSize.x);
                fixed4 color = SampleSpriteTexture(uv) * input.color;
                float luminance = dot(color.rgb, float3(.2126,.7152,.0722));
                color.rgb = lerp(float3(luminance,luminance,luminance),color.rgb,.94)*float3(.99,1.01,1.025);
                color.a *= 1 - smoothstep(.145,.17,uv.y);
                return color;
            }
            ENDCG
        }
    }
}
