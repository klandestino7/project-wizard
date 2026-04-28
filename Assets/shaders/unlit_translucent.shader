FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    Forward();
    Depth();
}

COMMON
{
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
    // FORÇA TRANSPARÊNCIA
    RenderState(BlendEnable, true);
    RenderState(SrcBlend, SRC_ALPHA);
    RenderState(DstBlend, INV_SRC_ALPHA);

    // IMPORTANTE
    #define S_TRANSLUCENT 1
    #define BLEND_MODE_ALREADY_SET 1
    #define DEPTH_STATE_ALREADY_SET 1

    #include "common/pixel.hlsl"

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init( i );

		float3 baseColor = float3(0.2, 0.5, 1.0);

		float3 viewDir = normalize( g_vCameraPositionWs - i.vPositionWithOffsetWs );
		float3 normal = normalize( i.vNormalWs );

		float fresnel = pow( 1.0 - saturate(dot(viewDir, normal)), 4.0 );

		// 🔥 borda energética
		float3 emissive = baseColor * fresnel * 3.0;

		m.Albedo = baseColor * 0.2; // bem fraco no centro
		m.Emission = emissive;

		// 👉 AQUI É O QUE FAZ FUNCIONAR
m.Opacity = 0.4 + fresnel * 0.8;

		return ShadingModelStandard::Shade( m );
	}
}