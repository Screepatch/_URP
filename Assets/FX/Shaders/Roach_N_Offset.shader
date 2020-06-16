Shader "Pixonic/Roach_N_Offset"{
    //Variables
    Properites{
        _MainTexture("Main Color(RGB) Hello!", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }

    Subshader{
        Pass{
            CGPROGRAM

            #pragma vertex vertexFunction
            #pragma fragment fragmentFunction

            #include "UnityCG.cginc"

            //Vertices
            //Normal
            //Color
            //UV

            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            }

            struct v2f{
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            }

            //Build our object!
            v2f vertexFunction(appdata IN){
                v2f OUT;
                
                OUT.position =
                OUT.uv =



                return OUT;
            }

            //Vertex function
            //Build The Object
            
            //Fragment function
            //Color it in!

            ENDCG
        }
    }
}