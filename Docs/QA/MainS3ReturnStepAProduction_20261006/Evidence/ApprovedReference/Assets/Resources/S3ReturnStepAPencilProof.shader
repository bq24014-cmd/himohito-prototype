Shader "HimoHito/Proof/S3ReturnStepAPencil"
{
    Properties
    {
        _MainTex ("Wood", 2D) = "white" {}
        _EndTex ("End grain", 2D) = "white" {}
        _Color ("Reveal tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex, _EndTex;
            fixed4 _Color;
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; float2 face:TEXCOORD1; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float2 face:TEXCOORD1; float3 normal:TEXCOORD2; };
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.face=v.face; o.normal=UnityObjectToWorldNormal(v.normal); return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float x=i.face.x, y=i.face.y;
                float3 grain=tex2D(_MainTex,i.uv).rgb;
                float l=dot(grain,float3(.299,.587,.114));
                float left=1-saturate(x/.28);
                float right=saturate((x-1.47)/.33);
                float3 normal=normalize(i.normal);
                float light=saturate(dot(normal,normalize(float3(-.48,.68,-.68))));
                float illumination=.43+.62*light;
                float3 lacquer=float3(.30,.46,.52) * (.70+.48*l);
                lacquer*=1+.16*left-.24*right;
                // Narrow exposed cedar edges identify a painted wooden pencil,
                // not a free-floating blue beam. No paper labels or ornament pile.
                float edge=1-smoothstep(.025,.070,min(x,1.8-x));
                float3 cedar=grain*float3(.97,.92,.83);
                float3 col=lerp(lacquer,cedar,edge*.72);
                // A cut, unsharpened end face supports the flat walking rim.
                float cap=1-smoothstep(.27,.33,y);
                float2 endUV=float2(x/1.8,y/.32);
                float3 end=tex2D(_EndTex,endUV).rgb;
                float e=dot(end,float3(.299,.587,.114));
                float3 face=end*float3(.99,.94,.86);
                float core=1-smoothstep(.91,1.02,length(float2((x-.90)/.19,(y-.13)/.085)));
                face=lerp(face,float3(.20,.34,.43),core);
                face*=.91+.10*(1-saturate(y/.32));
                col=lerp(col,face,cap);
                // Warm, restrained highlight; outer XY remains within support.
                float rim=1-smoothstep(.018,.070,y);
                col+=rim*float3(.10,.08,.05);
                float bevel=min(x,1.8-x);
                col*=.85+.15*smoothstep(0,.035,bevel);
                // Two subtle gold finish rings are painted decoration, not ledges.
                float band=(1-smoothstep(.015,.025,abs(y-.62)))+(1-smoothstep(.012,.018,abs(y-.69)));
                col=lerp(col,float3(.53,.43,.24),saturate(band)*.6);
                return fixed4(col*illumination*_Color.rgb,_Color.a);
            }
            ENDCG
        }
    }
}
