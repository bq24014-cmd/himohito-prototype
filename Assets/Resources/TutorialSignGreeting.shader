Shader "HimoHito/Guide Sign Greeting"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _TailSway ("Tail sway", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="False" }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment GreetingFrag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            float _TailSway;
            fixed4 GreetingFrag(v2f IN) : SV_Target
            {
                // Loose pink tail in TutorialGuideSign-v1. Board, knot and post stay unwarped.
                float2 uv = IN.texcoord;
                float mask = smoothstep(.50, .53, uv.x) * (1 - smoothstep(.58, .61, uv.x));
                mask *= smoothstep(.24, .28, uv.y) * (1 - smoothstep(.29, .38, uv.y));
                uv.x -= _TailSway * mask;
                fixed4 color = SampleSpriteTexture(uv) * IN.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
