Shader "HimoHito/Respawn Weave"
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
        _WeaveProgress ("Progress", Float) = 1
        _WeaveBounds ("World bounds", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex WeaveVert
            #pragma fragment WeaveFrag
            #pragma target 3.0
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            float _WeaveProgress;
            float4 _WeaveBounds;
            struct weave_v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 world : TEXCOORD1;
            };
            weave_v2f WeaveVert(appdata_t IN)
            {
                v2f sprite = SpriteVert(IN);
                weave_v2f OUT;
                OUT.vertex = sprite.vertex;
                OUT.color = sprite.color;
                OUT.uv = sprite.texcoord;
                OUT.world = mul(unity_ObjectToWorld, UnityFlipSprite(IN.vertex, _Flip)).xy;
                return OUT;
            }
            fixed4 WeaveFrag(weave_v2f IN) : SV_Target
            {
                float2 p = saturate((IN.world - _WeaveBounds.xy) / max(_WeaveBounds.zw, .001));
                float row = min(9, floor(p.y * 10));
                float x = fmod(row, 2) < 1 ? p.x : 1-p.x;
                float stitch = (min(11, floor(x * 12)) + 1) / 12;
                clip(_WeaveProgress * 10 - row - stitch);
                fixed4 color = SampleSpriteTexture(IN.uv) * IN.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
