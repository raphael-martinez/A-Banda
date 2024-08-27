// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.36 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.36;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:True;n:type:ShaderForge.SFN_Final,id:1,x:32069,y:32751|emission-26-OUT,alpha-2-A;n:type:ShaderForge.SFN_Tex2d,id:2,x:32832,y:32732,ptlb:Texture2D,ptin:_Texture2D,tex:fd9fd0d694b848d4e89df051ce333548,ntxv:0,isnm:False;n:type:ShaderForge.SFN_VertexColor,id:20,x:32832,y:32962;n:type:ShaderForge.SFN_Add,id:26,x:32470,y:32717|A-2-RGB,B-20-RGB;proporder:2;pass:END;sub:END;*/

Shader "Stencil/Transparent/Stencil Emissive Font" {
    Properties {
        _Texture2D ("Texture2D", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_Level ("Level", Int) = 1
    }
    SubShader {
		Stencil {
			Ref [_Level]
			Comp Equal
			Pass Keep
			Fail Keep
		}
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "ForwardBase"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 2.0
            uniform sampler2D _Texture2D; uniform float4 _Texture2D_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float2 node_39 = i.uv0;
                float4 node_2 = tex2D(_Texture2D,TRANSFORM_TEX(node_39.rg, _Texture2D));
                float4 node_20 = i.vertexColor;
                float3 emissive = (node_2.rgb+node_20.rgb);
                float3 finalColor = emissive;
/// Final Color:
                return fixed4(finalColor,node_2.a*i.vertexColor.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
