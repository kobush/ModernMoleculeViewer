//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

// Standard Transform Parameters
cbuffer cbPerFrame
{
	matrix World;
	matrix View;
	matrix Projection;
	matrix WorldViewProjection;
	float3 EyePosition;
}

cbuffer cbLight
{
	//TODO: set defaults
	float4 DiffuseColor;
	float3 SpecularColor;
	float  SpecularPower;
	float3 AmbientDown;
	float3 AmbientRange;
	float4 DirLightColorR;
	float4 DirLightColorG;
	float4 DirLightColorB;
	float4 DirSpecColorR;
	float4 DirSpecColorG;
	float4 DirSpecColorB;
	float4 DirToLightX;
	float4 DirToLightY;
	float4 DirToLightZ;
}

//--------------------------------------------------------------------------------------

struct VS_INPUT
{
	float3 Pos : SV_POSITION;	// position
	float3 Normal : NORMAL;		// normal
	float2 TexC : TEXCOORD0;	// texture
};

struct VS_INPUT_INSTANCED
{
	float3 Pos : SV_POSITION;	// position
	float3 Normal : NORMAL;		// normal
	float2 TexC : TEXCOORD0;	// texture

	float4 InstColor : COLOR;
	row_major float4x4 InstWorld : WORLD;
};

struct VS_OUTPUT
{
	float4 PosH : SV_POSITION;	// position in homnogeous coordinates
	float3 PosW : POSITION;		// position in world space
	float3 NormalW : NORMAL;	// normal in world space
	float2 TexC : TEXCOORD0;	// texture coordinates
	float4 Color : COLOR;		// vertex color
};

//--------------------------------------------------------------------------------------

struct Material
{
	float3 Normal;
	float3 DiffuseColor;
	float SpecExp;
	float SpecIntensity;
};


//--------------------------------------------------------------------------------------
// Utils Funtions
//--------------------------------------------------------------------------------------

float3 CalcAmbient(float3 Normal, float3 Color)
{
	// convert from [-1,1] to [0,1]
	float up = Normal.z * 0.5 + 0.5;

	// calculate the ambient value
	float3 ambient = AmbientDown + up * AmbientRange;

		// apply the ambient value to the Color
		return ambient * Color;
}

float4 dot4x1(float4 aX, float4 aY, float4 aZ, float3 b)
{
	return aX * b.xxxx + aY * b.yyyy + aZ * b.zzzz;
}

float4 dot4x4(float4 aX, float4 aY, float4 aZ, float4 bX, float4 bY, float4 bZ)
{
	return aX * bX + aY * bY + aZ * bZ;
}

float3 CalcDirectional(float3 position, Material material)
{
	// Phong diffuse
	/*float NDotL = dot(DirToLight, material.Normal);
	float intensity = saturate(NDotL);
	float3 finalColor = DirLightColor.rgb * intensity;*/

	float4 NDotL = saturate(dot4x1(DirToLightX, DirToLightY, DirToLightZ, material.Normal));
		float3 finalColor = float3(dot(DirLightColorR, NDotL), dot(DirLightColorG, NDotL), dot(DirLightColorB, NDotL));

		finalColor *= material.DiffuseColor;

	// Blinn specularB
	float3 ToEye = EyePosition.xyz - position;
		ToEye = normalize(ToEye);

	// single light
	/*float3 HalfWay = normalize(ToEye + DirToLight);
	float NDotH = saturate(dot(HalfWay, material.Normal));
	finalColor += DirLightColor.rgb * pow(NDotH, material.SpecExp) * material.SpecIntensity;*/

	// four light version
	float4 HalfWayX = ToEye.xxxx + DirToLightX;
		float4 HalfWayY = ToEye.yyyy + DirToLightY;
		float4 HalfWayZ = ToEye.zzzz + DirToLightZ;

		float4 HalfWaySize = sqrt(dot4x4(HalfWayX, HalfWayY, HalfWayZ, HalfWayX, HalfWayY, HalfWayZ));
		float4 NDotH = saturate(dot4x1(HalfWayX / HalfWaySize, HalfWayY / HalfWaySize, HalfWayZ / HalfWaySize, material.Normal));
		float4 SpecValue = pow(NDotH, material.SpecExp.xxxx) * material.SpecIntensity;
		finalColor += float3(dot(DirSpecColorR, SpecValue), dot(DirSpecColorG, SpecValue), dot(DirSpecColorB, SpecValue));

	//float4 pixelIntensity = NDotL + SpecValue;
	//float3 finalColor = float3(dot(DirLightColorR, pixelIntensity), dot(DirLightColorG, pixelIntensity), dot(DirLightColorB, pixelIntensity));

	return finalColor;
	//return float4(0,0,0,0);
}

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------

/*
* Vertex Shader - Transform (no lighting)
*/
VS_OUTPUT VSTransform(VS_INPUT input)
{
	VS_OUTPUT output;

	float4 pos = float4(input.Pos, 1);
	float4 color = DiffuseColor;

	// transform
	output.PosW = mul(pos, World); // world position
	output.PosH = mul(pos, WorldViewProjection); // homogenous projection
	output.NormalW = mul(input.Normal, (float3x3)World); // normal in world space

	// set texture & color
	output.Color = color;
	output.TexC = input.TexC;
	return output;
}

/*
* Vertex Shader - Transform (no lighting)
*/
VS_OUTPUT VSTransformInstanced(VS_INPUT_INSTANCED input)
{
	VS_OUTPUT output;

	float4 pos = float4(input.Pos, 1);
	float4 color = input.InstColor;
	float4x4 world = input.InstWorld;
	float4x4 wvp = mul(mul(world, View), Projection);

	// transform to world position
	output.PosW = mul(pos, world);

	// transform to homogenous clip space
	output.PosH = mul(pos, wvp);

	output.NormalW = mul(input.Normal, (float3x3)world); // normal in world space

	// set texture & color
	output.Color = color;
	output.TexC = input.TexC;
	return output;
}

/*
* Vertex Shader - Transform + Light
*/
VS_OUTPUT VSTransformLight(VS_INPUT input)
{
	VS_OUTPUT output;

	float4 pos = float4(input.Pos, 1);
		float4 color = DiffuseColor;

		// transform
		output.PosW = mul(pos, World); // world position
	output.PosH = mul(pos, WorldViewProjection); // homogenous projection
	output.NormalW = mul(input.Normal, (float3x3)World); // Normal in world space


	// setup material
	Material material;
	material.DiffuseColor = color;
	material.Normal = normalize(output.NormalW);
	material.SpecExp = SpecularPower;
	material.SpecIntensity = 1; //TODO:

	// calc lights
	float3 finalColor = CalcAmbient(material.Normal, material.DiffuseColor);
		finalColor += CalcDirectional(output.PosW, material);

	// copy texture & color
	output.Color = float4(finalColor.rgb, color.a);
	//	output.TexC = input.TexC;
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shaders
//--------------------------------------------------------------------------------------

/*
*	Pixel Shader - Vertex Color (no light)
*/
float4 PSVertexColor(VS_OUTPUT input) : SV_Target
{
	return input.Color;
}

/*
*	Pixel Shader - Light + Vertex Color
*/
float4 PSLightVertexColor(VS_OUTPUT input) : SV_Target
{

	Material material;
	material.DiffuseColor = input.Color.rgb;
	material.Normal = normalize(input.NormalW);
	material.SpecExp = SpecularPower;
	material.SpecIntensity = 1;

	// use this for no specular
	//material.SpecExp = 1;
	//material.SpecIntensity = 0;

	float3 finalColor = material.DiffuseColor;
		finalColor = CalcAmbient(material.Normal, material.DiffuseColor);
	finalColor += CalcDirectional(input.PosW, material);

	return float4(finalColor.rgb, input.Color.a);
}

//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------

//TODO: see https://github.com/sharpdx/SharpDX/blob/master/Source/Tests/SharpDX.Toolkit.Graphics.Tests/TestEffect.fx

technique RenderNoLighting
{
	pass P0
	{
		Profile = 9.3;
		VertexShader = VSTransform;
		PixelShader = PSVertexColor;
		GeometryShader = NULL;
	}
}

technique RenderPerVertexLighting
{
	pass P0
	{
		Profile = 9.3;
		VertexShader = VSTransformLight;
		PixelShader = PSVertexColor;
		GeometryShader = NULL;
	}
}

technique RenderPerPixelLighting
{
	pass P0
	{
		Profile = 9.3;
		VertexShader = VSTransform;
		PixelShader = PSLightVertexColor;
		GeometryShader = NULL;
	}
}

technique RenderPerPixelLightingInstanced
{
	pass P0
	{
		Profile = 9.3;
		VertexShader = VSTransformInstanced;
		PixelShader = PSLightVertexColor;
		GeometryShader = NULL;
	}
}

