float4x4 World;
float4x4 View;
float4x4 Projection;
float xTime;

// the ripple figures showing ripple size, spread, amplitude, etc.
float wave;                // pi/.75 is a good default
float distortion;        // 1 is a good default
float2 centerCoord;        // 0.5,0.5 is the screen center
float divisor;				// a value to measure wave growth rate
uniform extern texture ScreenTexture;
static const float Pi = 3.14159265f; 

sampler ScreenS = sampler_state
{
    Texture = <ScreenTexture>;    
};

// glow figures going here
texture originalImage;
sampler originalSampler = sampler_state
{
	texture = <originalImage>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
};

// we'll add the blur figures here
float positions[] = 
{
	0.0f,
	0.005,
	0.01166667,
	0.01833333,
	0.025,
	0.03166667,
	0.03833333,
	0.045,
};

float weights[] = 
{
	0.0530577,
	0.1028506,
	0.09364651,
	0.0801001,
	0.06436224,
	0.04858317,
	0.03445063,
	0.02294906,
};

float xBlurSize = 0.5f;


texture textureToSampleFrom;
sampler textureSampler = sampler_state
{
	texture = <textureToSampleFrom>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
};

struct PPVertexToPixel
{
	float2 TexCoord : TEXCOORD0;
	float4 Position : POSITION0;
};
struct PPPixelToFrame
{
	float4 Color	: COLOR0;
};

PPVertexToPixel PassThroughVertexShader(float4 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	PPVertexToPixel Output = (PPVertexToPixel)0;
	Output.Position = inPos;
	Output.TexCoord = inTexCoord;
	return Output;
}

//----------PP Technique: Invert-----------//
PPPixelToFrame InvertPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	
	Output.Color = tex2D(textureSampler, PSIn.TexCoord);
	Output.Color = float4(1-Output.Color.r, 1-Output.Color.g, 1-Output.Color.b, 1);
	
	return Output;
}

technique Invert
{
    pass Pass0
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_1_1 InvertPS();
    }
}

//----------PP Technique: Mosaic-----------//
PPPixelToFrame MosaicPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	
	Output.Color = tex2D(textureSampler, PSIn.TexCoord);
	
	if(Output.Color.r == 0 && Output.Color.b == 0 && Output.Color.g == 0)
		Output.Color.r += 0.01f;
	return Output;
}

technique Mosaic
{
    pass Pass0
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 MosaicPS();
    }
}

//------------PP Technique: TimeChange---------------//
PPPixelToFrame TimeChangePS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	
	Output.Color = tex2D(textureSampler, PSIn.TexCoord);
	Output.Color.b *= sin(xTime);
	Output.Color.rg *= cos(xTime);
	Output.Color += 0.2f;
	
	return Output;
}

technique TimeChange
{
    pass Pass0
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 TimeChangePS();
    }
}

//------------PP Technique: HorBlur---------------//
PPPixelToFrame HorBlurPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	
	for(int i = 0; i < 8; i++)
	{
		float4 samplePos = tex2D(textureSampler, PSIn.TexCoord + float2(positions[i], 0)*xBlurSize);
		samplePos *= weights[i];
		float4 sampleNeg = tex2D(textureSampler, PSIn.TexCoord - float2(positions[i], 0)*xBlurSize);
		sampleNeg *= weights[i];
		Output.Color += samplePos + sampleNeg;
	}
	return Output;
}

technique HorBlur
{
    pass Pass0
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 HorBlurPS();
    }
}

//------------PP Technique: VerBlur---------------//
PPPixelToFrame VerBlurPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	
	for(int i = 0; i < 8; i++)
	{
		float4 samplePos = tex2D(textureSampler, PSIn.TexCoord + float2(0, positions[i])*xBlurSize);
		samplePos *= weights[i];
		float4 sampleNeg = tex2D(textureSampler, PSIn.TexCoord - float2(0, positions[i])*xBlurSize);
		sampleNeg *= weights[i];
		Output.Color += samplePos + sampleNeg;
	}
	return Output;
}

technique VerBlur
{
    pass Pass0
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 VerBlurPS();
    }
}

//------------PP Technique: VerBlurAndGlow---------------//
PPPixelToFrame BlendInPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	
	float4 finalColor = tex2D(originalSampler, PSIn.TexCoord);
	finalColor.a = 0.6f;
	
	Output.Color = finalColor;
	return Output;
}

technique VerBlurAndGlow
{
	pass Pass0
	{
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 VerBlurPS();
    }
	pass Pass1
	{
		AlphablendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
        PixelShader = compile ps_2_0 BlendInPS();
    }
}


//------------PP Technique: Ripple---------------//
PPPixelToFrame RipplePS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
    float2 distance1 = abs(PSIn.TexCoord - centerCoord);
    float scalar = length(distance1);

    // invert the scale so 1 is centerpoint
    scalar = abs(1 - scalar);
    
    // calculate how far to distort for this pixel    
    float sinoffset = sin(wave / scalar);
    sinoffset = clamp(sinoffset, 0, 1);
    
    // calculate which direction to distort
    float sinsign = cos(wave / scalar);    
    
    // reduce the distortion effect
    sinoffset = sinoffset * distortion/32;
    
    // pick a pixel on the screen for this pixel, based on
    // the calculated offset and direction
    float4 color = tex2D(textureSampler, PSIn.TexCoord);
    float farness = distance(PSIn.TexCoord, centerCoord);
    if(farness < divisor)
    {
		    color = tex2D(textureSampler, PSIn.TexCoord+(sinoffset*sinsign)); 
    }

    Output.Color = color;   
    return Output;
}

PPPixelToFrame BlendRipplePS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	float4 finalColor = tex2D(originalSampler, PSIn.TexCoord);
    float farness = distance(PSIn.TexCoord, centerCoord);
    if(farness < divisor)
    {
		finalColor.a -= xTime;
	}
	Output.Color = finalColor;
	return Output;
}

technique Ripple
{
    pass Pass0
    {
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 RipplePS();
    }
    pass Pass1
    {
		AlphablendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		PixelShader = compile ps_2_0 BlendRipplePS();
    }
}


//-------------------Bump Mapping-----------------------//


PPPixelToFrame BumpPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	float4 Color = tex2D(originalSampler, PSIn.TexCoord);
	Color -= tex2D(originalSampler , PSIn.TexCoord.xy-0.003)*2.7f;
	Color += tex2D(originalSampler , PSIn.TexCoord.xy+0.003)*2.7f;
	Color.rgb = (Color.r+Color.g+Color.b)/3.0f;
	Output.Color = Color;
	return Output;
}

technique Bump
{
	pass P0
	{
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 BumpPS();
	}

}

//----------------------ColorWringing------------------------------//

PPPixelToFrame ColorWringPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	float4 Color = tex2D(originalSampler, PSIn.TexCoord);
	Color.rgb = (Color.r+Color.g+Color.b)/3.0f;
	if (Color.r<0.2 || Color.r>0.9) Color.r = 0; else Color.r = 1.0f;
	if (Color.g<0.2 || Color.g>0.9) Color.g = 0; else Color.g = 1.0f;
	if (Color.b<0.2 || Color.b>0.9) Color.b = 0; else Color.b = 1.0f;
	Output.Color = Color;
	return Output;
}

technique ColorWring
{
	pass P0
	{
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 ColorWringPS();
	}

}

//-------------------------------GreenShift-------------------------//
PPPixelToFrame GreenShiftPS(PPVertexToPixel PSIn) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
	float4 Color = tex2D(originalSampler, PSIn.TexCoord);
	Color.r -= tex2D( originalSampler , PSIn.TexCoord).r;
	Color.g += tex2D( originalSampler , PSIn.TexCoord).g;
	Color.b -= tex2D( originalSampler , PSIn.TexCoord).b;
	Output.Color = Color;
	return Output;
}

technique GreenShift
{
	pass P0
	{
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 GreenShiftPS();
	}

}


//-----------------XNA Helpsite Effect Sample: Distortion-------------------//
//-----------------------------------------------------------------------------
// Distort.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

texture backGroundTexture;
sampler backGroundSampler = sampler_state
{
	texture = <backGroundTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
};

sampler SceneTexture : register(s0);
sampler DistortionMap : register(s1);

#define SAMPLE_COUNT 15
float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

// The Distortion map represents zero displacement as 0.5, but in an 8 bit color
// channel there is no exact value for 0.5. ZeroOffset adjusts for this error.
const float ZeroOffset = 0.5f / 255.0f;

PPPixelToFrame Distort_PixelShader(PPVertexToPixel PSIn, 
    uniform bool distortionBlur) : COLOR0
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
    // Look up the displacement
    float2 displacement = tex2D(backGroundSampler, PSIn.TexCoord);
    
    float4 finalColor = 0;
    // We need to constrain the area potentially subjected to the gaussian blur to the
    // distorted parts of the scene texture.  Therefore, we can sample for the color
    // we used to clear the distortion map (black).  We used 0 to avoid any potential
    // rounding errors.
    if ((displacement.x == 0) && (displacement.y == 0))
    {
        finalColor = tex2D(backGroundSampler, PSIn.TexCoord);
    }
    else
    {
        // Convert from [0,1] to [-.5, .5) 
        // .5 is excluded by adjustment for zero
        displacement -= .5 + ZeroOffset;

        if (distortionBlur)
        {
            // Combine a number of weighted displaced-image filter taps
            for (int i = 0; i < SAMPLE_COUNT; i++)
            {
                finalColor += tex2D(backGroundSampler, PSIn.TexCoord.xy + 
                    SampleOffsets[i]) * SampleWeights[i];
            }
        }
        else
        {
            // Look up the displaced color, without multisampling
            finalColor = tex2D(backGroundSampler, PSIn.TexCoord.xy + displacement);  
        }
    }
	Output.Color = finalColor;
    return Output;
}

technique Distort
{
    pass
    {
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_1_4 Distort_PixelShader(false);
    }
}

technique DistortBlur
{
    pass
    {
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 Distort_PixelShader(true);
    }
}

//-----------------------------------------------------------------------------
// Distorters.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

// common parameters
float4x4 WorldViewProjection;
float4x4 WorldView;
float DistortionScale;
float Time;


//-----------------------------------------------------------------------------
//
// Displacement Mapping
//
//-----------------------------------------------------------------------------

struct PositionTextured
{
   float4 Position : POSITION;
   float2 TexCoord : TEXCOORD;
};

PositionTextured TransformAndTexture_VertexShader(PositionTextured input)
{
    PositionTextured output;
    
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord;
    
    return output;
}

texture2D DisplacementMap;
sampler2D DisplacementMapSampler = sampler_state
{
    texture = <DisplacementMap>;
};

PPPixelToFrame Textured_PixelShader(PPVertexToPixel PSIn) : COLOR
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
    float4 color = tex2D(backGroundSampler, PSIn.TexCoord);
    
    // Ignore the blue channel    
    Output.Color = float4(color.rg, 0, color.a);
    return Output;
}

technique DisplacementMapped
{
    pass
    {
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_1_1 Textured_PixelShader();
    }
}


//-----------------------------------------------------------------------------
//
// Heat-Haze Displacement
//
//-----------------------------------------------------------------------------

struct PositionPosition
{
    float4 Position : POSITION;
        
    // the pixel shader does not have direct access to the position of the pixel
    // being shaded, so we must pass this information through from the vertex shader
    float4 PositionAsTexCoord : TEXCOORD;
};

PositionPosition TransformAndCopyPosition_VertexShader(float4 position : POSITION)
{
    PositionPosition output;
    
    output.Position = mul(position, WorldViewProjection);
    output.PositionAsTexCoord = output.Position;
    
    return output;
}

PPPixelToFrame HeatHaze_PixelShader(float4 position : TEXCOORD) : COLOR
{
	PPPixelToFrame Output = (PPPixelToFrame)0;
    float2 displacement;
    displacement.x = sin((position.x) + (Time * 1.5)) * sin(position.x) * 
        cos(position.x);
    displacement.y = sin((position.y / -0.25) - (Time * 2.75));
    displacement *= DistortionScale;
    // displacement = displacement / 2;
    
    float4 color = tex2D(textureSampler, position+displacement); 
    //color[2] = 0;
    Output.Color = color;
    return Output;
}

technique HeatHaze
{
    pass
    {
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_2_0 HeatHaze_PixelShader();
    }
}


//-----------------------------------------------------------------------------
//
// Pull-In Displacement
//
//-----------------------------------------------------------------------------

struct PositionNormal
{
   float4 Position : POSITION;
   float3 Normal : NORMAL;
};

struct PositionDisplacement
{
   float4 Position : POSITION;
   float2 Displacement : TEXCOORD;
};

PositionDisplacement PullIn_VertexShader(PositionNormal input)
{
   PositionDisplacement output;

   output.Position = mul(input.Position, WorldViewProjection);
   float3 normalWV = mul(input.Normal, WorldView);
   normalWV.y = -normalWV.y;
   
   float amount = dot(normalWV, float3(0,0,1)) * DistortionScale;
   output.Displacement = float2(.5,.5) + float2(amount * normalWV.xy);

   return output;   
}

float4 DisplacementPassthrough_PixelShader(float2 displacement : TEXCOORD) : COLOR
{  
   return float4(displacement, 0, 1);
}

technique PullIn
{
    pass
    {
        VertexShader = compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_1_4 DisplacementPassthrough_PixelShader();
    }
}

//-----------------------XNA Helpsite: Distorters---------------------//
//-----------------------------------------------------------------------------
//
// Zero Displacement (provided for reference)
//
//-----------------------------------------------------------------------------


float4 TransformOnly_VertexShader(float4 position : POSITION) : POSITION
{
    return mul(position, WorldViewProjection);
}

float4 ZeroDisplacement_PixelShader() : COLOR
{
    return float4(.5, .5, 0, 0);
}

technique ZeroDisplacement
{
    pass
    {
        VertexShader =  compile vs_1_1 PassThroughVertexShader();
        PixelShader = compile ps_1_1 ZeroDisplacement_PixelShader();
    }
}