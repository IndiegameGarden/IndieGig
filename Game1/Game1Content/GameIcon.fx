// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

// center constant
const float2 Center = float2(0.5,0.5);

// size settings for the 'shadow box' border
const float ShadowBoxWidth = 0.75/2;
const float ShadowBoxHeight = 0.75/2;
const float2 ShadowBoxScale = float2(0.6,0.6);
const float BorderThickness = 18;		// lower nr is bigger border, typ. 21
const float BorderReflection = 0.35;		// 0-1

// modify the sampler state on the zero texture sampler, used by SpriteBatch
sampler TextureSampler : register(s0) = 
sampler_state
{
    AddressU = Clamp;
    AddressV = Clamp;
};

// shader uses 'color', the DrawColor, for special effects that are set per sprite:
// color.a - transparency result 0...1
// color.r - saturation 0...1 (0=black&white, 1=colored-full)
// color.g - intensity 0 (dark)...1 (light)
//
// alternative: color.r/g/b used as a 'time' variable.
//
float4 PixelShaderFunction(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 tex = tex2D(TextureSampler, ((texCoord - Center)/ShadowBoxScale+Center) ) ;		  
	float4 res ;
	float alpha ;
	res = float4(0,0,0,0);
	
	// check for shadow box bounds
	if ( texCoord.x < (Center.x + ShadowBoxWidth) &&
		 texCoord.x > (Center.x - ShadowBoxWidth) &&
		 texCoord.y > (Center.y - ShadowBoxHeight) &&
		 texCoord.y < (Center.y + ShadowBoxHeight) )
	{
		res.a=1;
		float d=0;
		if (texCoord.x < Center.x)
			d = abs(texCoord.x - (Center.x-ShadowBoxWidth) );
		else if (texCoord.x > Center.x)
			d = abs(texCoord.x - (Center.x+ShadowBoxWidth) );
		float c = 1-BorderThickness*d;
		if (c<0) c=0;
		
		if (texCoord.y < Center.y)
			d = abs(texCoord.y - (Center.y-ShadowBoxHeight) );
		else if (texCoord.y > Center.y)
			d = abs(texCoord.y - (Center.y+ShadowBoxHeight) );
		float c2 = 1-BorderThickness*d;

		// use the best-case c/c2
		if (c2<0) c2=0;
		if (c2>c) c= c2;

		res = c*res + (1-c) * ( (1-BorderReflection)*float4(0.23,0.23,0.19,0) + BorderReflection*tex); //float4(0.13,0.02,0.042,0);
		if (c==0)
		{
			// copy the bitmap color as-is.
			res=tex;
		}else{
			res.a = 1-c;
		}
	}else{
		res = float4(0,0,0,0);
	}
	
	// apply alpha factor
	//res *= color; 
	return res ;

}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}
