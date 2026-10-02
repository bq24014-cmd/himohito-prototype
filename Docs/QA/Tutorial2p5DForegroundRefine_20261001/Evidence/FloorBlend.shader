Shader "HimoHitoProof/FloorBlend"
{
    Properties
    {
        [PerRendererData] _MainTex ("Floor texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="False" }
        Cull Off ZWrite Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            sampler2D _MainTex;
            fixed4 _Color;
            output vert(input v)
            {
                output o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color; return o;
            }
            fixed4 frag(output i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)*i.color;
                // Only the reused bottom floor strip; no change to wall/window imagery.
                float l=dot(c.rgb,float3(.2126,.7152,.0722));
                c.rgb=lerp(float3(l,l,l),c.rgb,.94)*float3(.99,1.01,1.025);
                c.a*=1-smoothstep(.145,.17,i.uv.y);
                return c;
            }
            ENDCG
        }
    }
}
