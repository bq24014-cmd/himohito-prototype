Shader "HimoHito/S3ReturnStepBMarkerProof"
{
    Properties { _MainTex("Subtle surface texture",2D)="white"{} _Color("Reveal tint",Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; fixed4 _Color;
            struct appdata { float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;float2 face:TEXCOORD1; };
            struct v2f { float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 face:TEXCOORD1;float3 normal:TEXCOORD2; };
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.face=v.face;o.normal=UnityObjectToWorldNormal(v.normal);return o;}
            float strokeMask(float distance,float width){return 1-smoothstep(width,width+.018,abs(distance));}
            float feltGlyph(float2 p,float id)
            {
                float2 cell=floor(p*float2(5,7));
                float inside=step(0,p.x)*step(p.x,.999)*step(0,p.y)*step(p.y,.999);
                float left=1-step(.5,cell.x);
                float top=step(5.5,cell.y), middle=1-step(.5,abs(cell.y-3)), bottom=1-step(.5,cell.y);
                float on=0;
                if(id<.5)on=max(left,max(top,middle));
                else if(id<1.5)on=max(left,max(top,max(middle,bottom)));
                else if(id<2.5)on=max(left,bottom);
                else on=max(top,1-step(.5,abs(cell.x-2)));
                float2 f=frac(p*float2(5,7));
                float gap=smoothstep(.03,.10,min(f.x,1-f.x))*smoothstep(.03,.10,min(f.y,1-f.y));
                return on*inside*gap;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float x=i.face.x,y=i.face.y;
                float3 grain=tex2D(_MainTex,i.uv).rgb;
                float noise=dot(grain,float3(.299,.587,.114));
                float3 normal=normalize(i.normal);
                float lit=.56+.45*saturate(dot(normal,normalize(float3(-.43,.64,-.70))));
                float roundShade=.74+.26*sqrt(saturate(1-pow((x-.9)/.9,2)));
                float3 ink=float3(.55,.30,.21);
                float3 ivory=float3(.75,.69,.55);
                float cap=1-smoothstep(2.16,2.22,y);
                float heel=smoothstep(9.92,10.0,y);
                float3 col=lerp(ivory,ink,saturate(cap+heel));
                col*=.94+.10*noise;
                // A molded cap, not wood grain: longitudinal ribs stop above its joint.
                float ribs=pow(.5+.5*cos(x*6.2831853/.20),12);
                float ribArea=smoothstep(.42,.54,y)*(1-smoothstep(1.90,2.03,y));
                col-=cap*ribs*ribArea*float3(.065,.040,.030);
                float seam=strokeMask(y-2.22,.024)+strokeMask(y-9.96,.022);
                col*=1-.35*saturate(seam);
                float lip=strokeMask(y-2.10,.026);
                col+=lip*float3(.055,.040,.025);
                // Warm paper sleeve with only two quiet printed bands and a nib symbol.
                float sleeve=smoothstep(2.65,2.72,y)*(1-smoothstep(8.90,8.98,y));
                col=lerp(col,ivory*(.94+.08*noise),sleeve);
                float bands=strokeMask(y-3.20,.040)+strokeMask(y-8.40,.035);
                float printInset=smoothstep(.23,.29,x)*(1-smoothstep(1.49,1.56,x));
                col=lerp(col,ink*.80,saturate(bands)*printInset*.78);
                // A quiet, unbranded FELT label identifies the capped pen.
                float letter=floor((y-4.10)/.87);
                float text=feltGlyph(float2(frac((y-4.10)/.87)/.79,(1.33-x)/.86),letter);
                text*=step(0,letter)*step(letter,3);
                col=lerp(col,ink*.83,text*.67);
                // Closed flat cap crown and bevel. The entire rim sits at collider top.
                float crown=1-smoothstep(.22,.31,y);
                float3 crownCol=ink*(1.06+.16*(1-saturate(y/.31)));
                crownCol+=strokeMask(y-.23,.015)*float3(.045,.025,.016);
                float oval=1-smoothstep(.92,1.0,length(float2((x-.9)/.86,(y-.13)/.12)));
                crownCol+=oval*float3(.035,.023,.015);
                col=lerp(col,crownCol,crown);
                col*=roundShade;
                col+=exp(-pow((x-.37)/.18,2))*float3(.035,.027,.018);
                col*=1-.10*smoothstep(1.32,1.75,x);
                float edge=1-smoothstep(.008,.055,min(x,1.8-x));
                col*=1-.20*edge;
                col+=(1-smoothstep(.018,.06,y))*float3(.07,.05,.03);
                return fixed4(col*lit*_Color.rgb,_Color.a);
            }
            ENDCG
        }
    }
}
