Shader "Doors/BasicLight"
{
    Properties
    {
        _AmbientColor("Ambient Color", Color) = (0.25, 0.25, 0.25, 1)
        _DiffuseColor("Diffuse Color", Color) = (1, 1, 1, 1)
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _Glossiness("Glossiness", Range(1, 512)) = 20

        _DiffuseMap("Diffuse Texture", 2D) = "white" {}
        _SpecularMap("Specular Texture", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        _LightColor("Light Color", Color) = (1, 1, 1, 1)
        _LightPos("Light Position", Vector) = (100, 100, 100, 0)
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                    float4 tangent : TANGENT;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float3 lightVec : TEXCOORD1;
                    float3 eyeVec : TEXCOORD2;
                    float3 worldNormal : TEXCOORD3;
                    float3 worldBinormal : TEXCOORD4;
                    float3 worldTangent : TEXCOORD5;
                    float4 vertex : SV_POSITION;
                };

                float4 _AmbientColor;
                float4 _DiffuseColor;
                float4 _SpecularColor;
                float _Glossiness;

                sampler2D _DiffuseMap;
                sampler2D _SpecularMap;
                sampler2D _NormalMap;

                float4 _LightColor;
                float4 _LightPos;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;

                    o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                    o.worldBinormal = cross(o.worldNormal, o.worldTangent) * v.tangent.w;

                    float3 worldSpacePos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.lightVec = _LightPos.xyz - worldSpacePos;
                    o.eyeVec = _WorldSpaceCameraPos - worldSpacePos;

                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    float4 colorTexture = tex2D(_DiffuseMap, i.uv);
                    float4 specularTexture = tex2D(_SpecularMap, i.uv);
                    float3 normal = UnpackNormal(tex2D(_NormalMap, i.uv));

                    float3 Nn = normalize(i.worldNormal);
                    float3 Bn = normalize(i.worldBinormal);
                    float3 Tn = normalize(i.worldTangent);

                    float3 N = (normal.z * Nn) + (normal.x * Bn) + (normal.y * -Tn);
                    N = normalize(N);

                    float3 L = normalize(i.lightVec);
                    float3 V = normalize(i.eyeVec);

                    float4 ambient = _AmbientColor * colorTexture;

                    float diffuseLight = saturate(dot(N, L));
                    float4 diffuse = diffuseLight * _DiffuseColor * colorTexture * _LightColor;

                    float3 H = normalize(L + V);
                    float NdotH = saturate(dot(N, H));
                    float gloss = _Glossiness * specularTexture.a;
                    float specPower = pow(NdotH, gloss);

                    float4 specular = specPower * _SpecularColor * specularTexture;

                    return (ambient + diffuse + specular) * _LightColor;
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}