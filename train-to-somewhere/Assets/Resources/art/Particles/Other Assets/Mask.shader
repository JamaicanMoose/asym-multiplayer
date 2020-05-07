Shader "Masking/Mask"
{
    
	SubShader
	{
		Tags{ "Queue" = "Transparent-1" "IgnoreProjector" = "True"}

		ColorMask 0
		Zwrite On
		Pass{}
    }
}
