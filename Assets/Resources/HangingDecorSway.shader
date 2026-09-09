Shader "HimoHito/Hanging Decor Sway"
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
        [HideInInspector] _StarAngles ("Star angles", Vector) = (0,0,0,0)
        [HideInInspector] _FlagAngles ("Flag angles", Vector) = (0,0,0,0)
        [HideInInspector] _LastFlagAngle ("Last flag angle", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment DecorFrag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float4 _StarAngles;
            float4 _FlagAngles;
            float _LastFlagAngle;

            // Coordinates in the original 1672 x 941 illustration (top-left
            // origin), independent of the imported texture resolution.
            // The narrow feather also carries the painted shadow; outside each
            // rectangle UVs are EXACTLY unchanged. The small-angle inverse turn
            // keeps the fastening point fixed and avoids a cutout-shaped seam.
            float2 Turn(float2 p, float2 pivot, float4 rect, float angle)
            {
                float2 edge = min(p - rect.xy, rect.zw - p);
                float weight = smoothstep(0, 8, min(edge.x, edge.y));
                weight *= smoothstep(pivot.y, pivot.y + 12, p.y);
                float2 d = p - pivot;
                return float2(d.y, -d.x) * (angle * weight);
            }

            fixed4 DecorFrag(v2f IN) : SV_Target
            {
                float2 p = float2(IN.texcoord.x * 1672, (1 - IN.texcoord.y) * 941);
                float2 offset = 0;
                // Three wall stars. The wall and furniture outside these small
                // feathered regions stay still; there is no whole-image wobble.
                if (p.x > 472 && p.x < 666 && p.y > 159 && p.y < 343)
                {
                    offset += Turn(p, float2(539,173), float4(504,160,575,231), _StarAngles.x);
                    offset += Turn(p, float2(632,237), float4(600,227,666,290), _StarAngles.y);
                    offset += Turn(p, float2(505,288), float4(473,278,540,343), _StarAngles.z);
                }
                // Five complete pennants. The clipped end pennants, ceiling
                // string, cloud, shelves and window retain their original art.
                if (p.x > 1064 && p.x < 1570 && p.y < 155)
                {
                    offset += Turn(p, float2(1104,71), float4(1064,39,1148,126), _FlagAngles.x);
                    offset += Turn(p, float2(1209,85), float4(1168,64,1253,151), _FlagAngles.y);
                    offset += Turn(p, float2(1315,86), float4(1273,69,1358,155), _FlagAngles.z);
                    offset += Turn(p, float2(1421,80), float4(1380,50,1466,143), _FlagAngles.w);
                    offset += Turn(p, float2(1525,52), float4(1485,14,1570,111), _LastFlagAngle);
                }
                float2 uv = IN.texcoord + float2(offset.x / 1672, -offset.y / 941);
                // Keep the filtering footprint still too: implicit derivatives
                // of the warped UV can otherwise shimmer at the mask border.
                float2 dx = ddx(IN.texcoord), dy = ddy(IN.texcoord);
                fixed4 color = tex2Dgrad(_MainTex, uv, dx, dy);
                #if ETC1_EXTERNAL_ALPHA
                    color.a = lerp(color.a, tex2Dgrad(_AlphaTex, uv, dx, dy).r, _EnableExternalAlpha);
                #endif
                color *= IN.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
