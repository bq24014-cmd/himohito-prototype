Shader "HimoHito/MainS4Ruler"
{
 Properties {
 _MainTex("Existing warm continuous wood",2D)="white"{}
 _EndTex("Natural top end grain",2D)="white"{}
 _MarkTex("Accurate ruler ink",2D)="black"{}
 _Width("World width",Float)=1.8
 _Height("World height",Float)=10
 }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex,_EndTex,_MarkTex;float _Width,_Height;
 struct appdata{float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;float2 face:TEXCOORD1;};
 struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;float2 face:TEXCOORD2;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.uv=v.uv;o.face=v.face;return o;}
 fixed4 frag(v2f i):SV_Target{
 float3 n=normalize(i.normal);float light=saturate(dot(n,normalize(float3(-.48,.68,-.68))));
 fixed3 face=tex2D(_MainTex,i.uv).rgb*float3(.92,.95,1);
 float crown=smoothstep(.20,.78,n.y)*(1-smoothstep(.20,.29,i.face.y));
 fixed3 cut=tex2D(_EndTex,float2(.27+i.face.x/_Width*.35,.23+i.face.y/.29*.24)).rgb*float3(.97,.91,.83);
 fixed3 wood=lerp(face,cut,crown)*(.43+.62*light);
 fixed4 ink=tex2D(_MarkTex,float2(i.face.x/_Width,1-i.face.y/_Height));
 // Ink is only color. Alpha remains one across every collider-supported point.
 return fixed4(lerp(wood,ink.rgb*(.8+.2*light),ink.a),1);
 }
 ENDCG }
 }
}
