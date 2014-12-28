// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

// background shader - RGB data contains a thumbnail or image from which colors are used
// and alpha channel should contain intensity of another image (i.e. BW image 8-bit encoded)

// variables set from the outside - must be same for all sprites in a batch rendered with this shader.
// linear time - for animation control
//float Time = 0;
// center of rgb sprite
float2 Center = float2(0.5,0.5);
float Scale = 1.0;

// modify the sampler state on the zero texture sampler, used by SpriteBatch
sampler TextureSampler : register(s0) = 
sampler_state
{
    AddressU = Clamp;
    AddressV = Clamp;
};

// Input: It uses texture coords as the random number seed.
// Output: Random number: [0,1), that is between 0.0 and 0.999999... inclusive.
// Author: Michael Pohoreski
// Copyright: Copyleft 2012 :-)

float random( float2 p )
{
  // We need irrationals for pseudo randomness.
  // Most (all?) known transcendental numbers will (generally) work.
  const float2 r = float2(
    23.1406926327792690,  // e^pi (Gelfond's constant)
     2.6651441426902251); // 2^sqrt(2) (Gelfond–Schneider constant)
  return frac( cos( 123456789. % (1e-7 + 256. * dot(p,r) ) ) );  
}

float4 PixelShaderFunction(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float2 vDif = texCoord - Center ;
	float4 res ;
	//float bri = (tex2D(TextureSampler, texCoord )).a ;
	float2 vDifNorm = normalize(vDif);
	float lDif = length(vDif);
	float lWarped = (1+0.1*random(texCoord))*lDif;
	//res = tex2D(TextureSampler, (vDif/Scale+Center) ) ;		  
	res  = tex2D(TextureSampler, (vDifNorm*lWarped/Scale+Center) ) ;		  
	float bri = (res.r + res.g + res.b) / 3.0 ;
	float alpha = res.a;

	//res *= bri;
	//res.a *= (1-bri);
	//res = res.a * float4(1,1,1,0) + (1-res.a) * res;
	//res = (1-res.a) * float4(1,1,1,0) + (res.a) * res;
	//res = 1-res;
	//res.a = 1;

	//res = (1-alpha) * float4(0,0,0,1) + (alpha) * res;
	//float alphaOut = 1 - alpha*alpha - bri ;
	float alphaOut = 1 - (bri) ;
	if (alphaOut < 0)
		alphaOut = 0;
	if (alphaOut > 1)
		alphaOut = 1;
	res *= alphaOut; // needed to get pre-multiplied alpha properly
	res.a = alphaOut;
	return res ;

}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
    }
}
