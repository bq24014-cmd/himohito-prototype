Shader "HimoHito/Tutorial Background Edge"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
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
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment EdgeFrag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            float4 _MainTex_TexelSize;
            fixed4 EdgeFrag(v2f input) : SV_Target
            {
                float2 uv = input.texcoord;
                // The approved Mid's final column is entirely transparent.
                // Clamp only its outer sampling fringe; geometry/phase stay exact.
                uv.x = min(uv.x, 1 - 1.5 * _MainTex_TexelSize.x);
                fixed4 color = SampleSpriteTexture(uv) * input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
