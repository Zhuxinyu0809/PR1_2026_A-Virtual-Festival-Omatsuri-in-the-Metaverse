#ifndef MIRROR_DEFINITIONS_INCLUDED
#define MIRROR_DEFINITIONS_INCLUDED

#include "EyeIndex.hlsl"

float4 GetReflectionTexture(float eyeIndex, float2 uv) {
	//draw to left texture always on clipping
	if (_ClipEye > 0.) {
		if (_LODLevel < 1.) {
			return SAMPLE_TEXTURE2D(_LeftOrCenterTexture, sampler_LeftOrCenterTexture, uv);
		}
		else {
			return SAMPLE_TEXTURE2D_LOD(_LeftOrCenterTexture, sampler_LeftOrCenterTexture, uv, _LODLevel);
			//todo: do blurring here
		}
	}
	else if (eyeIndex == 0.) {
		if (_LODLevel < 1.) {
			return SAMPLE_TEXTURE2D(_LeftOrCenterTexture, sampler_LeftOrCenterTexture, uv);
		}
		else {
			return SAMPLE_TEXTURE2D_LOD(_LeftOrCenterTexture, sampler_LeftOrCenterTexture, uv, _LODLevel);
			//todo: do blurring here
		}
	}
	else {
		if (_LODLevel < 1.) {
			return SAMPLE_TEXTURE2D(_RightTexture, sampler_RightTexture, uv);
		}
		else {
			return SAMPLE_TEXTURE2D_LOD(_RightTexture, sampler_RightTexture, uv, _LODLevel);
		}
	}
}
float GetDepthTexture(float eyeIndex, float2 uv) {
	if (eyeIndex == 0.) {
		return LOAD_TEXTURE2D_X(_LeftOrCenterDepthTexture, uv).r;
	}
	else {
		return LOAD_TEXTURE2D_X(_RightDepthTexture, uv).r;
	}
}

float4 ProcessMirror(float2 uv, float4 screenPos, float eyeIndex, inout SurfaceData surfaceData, float distance)
{
	half4 maskAlpha = half4(1, 1, 1, 1);
	half4 mask = half4(1, 1, 1, 1);

#ifdef _AKMU_MIRROR_MASK
	#if UNITY_VERSION >= 201900
		maskAlpha = SampleAlbedoAlpha(uv / half2(_MaskTiling.r, _MaskTiling.g), TEXTURE2D_ARGS(_MaskTex, sampler_LinearRepeat));
	#else
		maskAlpha = SampleAlbedoAlpha(uv / half2(_MaskTiling.r, _MaskTiling.g), TEXTURE2D_PARAM(_MaskTex, sampler_LinearRepeat));
	#endif

	mask = smoothstep(maskAlpha.a, 0, _MaskCutoff);
#endif

	//todo: moved
	////recalculate according to mask
	//surfaceData.smoothness = lerp(surfaceData.smoothness, 1.0, mask.a);
	//// Water F0 specular is 0.02 (based on IOR of 1.33)
	//surfaceData.specular = lerp(surfaceData.specular, 0.02, mask.a);

	//float2 screenUV = (input.screenPos.xy) / (input.screenPos.w + FLT_MIN);
	float2 screenUV = screenPos.xy / screenPos.w;
	if (_ClipUV < 99) {
		/*float x = (input.uv.x > 0.5 ? 1 : -1) * (_ClipPercentage / 100.);
		float y = (input.uv.y > 0.5 ? 1 : -1) * (_ClipPercentage / 100.);*/
		//float2 newUV = input.uv * float2(x,y); // (100. / _ClipPercentage);
		//input.uv.x = input.uv.x + ((input.uv - input.uv.x) * scale)
		//screenUV = input.uv;
		//todo: is it reversed?
		screenUV = 1 - uv;// +_ClipPercentage / 1000.;// *(_ClipPercentage / 100.) + 0.5;
	}

	//eyeindex.hlsl
	Index_float(screenUV, _IsMultiPass, eyeIndex, screenUV);
	//!!!!!!!!!!!!!!!!!!!!!!!!!! do this after eyeIndex calculation!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	if (_WorkType == 30.) //AR mode, eyeIndex comes wrong on certain angles on AR!!!! so set it to zero always
	{
		eyeIndex = 0;
	}

	//0.1,0.1
	//float2 centerUV = _MirrorCenter.xy / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN);
	//float2 centerUV = _MirrorCenter.xy / (input.screenPos.w + FLT_MIN);
	//0.2,0.2
	//float2 uvDifference = centerUV - screenUV;
	//float2 step = 1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN);
	//screenUV += uvDifference*step;
	//screenUV = input.uv; // 2;// input.uv;// *_ScreenParams.xy; //test

	//#if UNITY_SINGLE_PASS_STEREO  //!!LWRP does not need that, i suppose it already corrects it with UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		//If Single-Pass Stereo mode is active, transform the
		//coordinates to get the correct output UV for the current eye.
	   //float4 scaleOffset = unity_StereoScaleOffset[input.eyeIndex];
	   //screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
	//#endif

	half3 nor = float3(1, 1, 1);
	
#ifdef _AKMU_MIRROR_REFRACTION
	nor = SampleNormal(uv, TEXTURE2D_ARGS(_RefractionTex, sampler_LinearRepeat), 1);
	screenUV.xy += (nor * (_ReflectionRefraction / distance));
#endif

	float4 reflection = float4(1, 1, 1, 0);

	half2 input1 = half2(0, 0);
	half2 input2 = half2(0, 0);
	float2 noiseOffset = float2(0, 0);
	
#ifdef _AKMU_MIRROR_WAVE

	float2 noiseUV1 = screenUV - _Time.y * _WaveSpeed;
	float2 noiseUV2 = screenUV + _Time.y * _WaveSpeed;

	float4 noise1 = SAMPLE_TEXTURE2D(_WaveNoiseTex, sampler_LinearRepeat, noiseUV1 / _WaveSize);
	float4 noise2 = SAMPLE_TEXTURE2D(_WaveNoiseTex, sampler_LinearRepeat, noiseUV2 / _WaveSize);

	noiseOffset = (noise1.xy + noise2.xy) / 2;
	noiseOffset -= float2(.5, .5);
	noiseOffset *= (_WaveDistortion / distance);

	reflection = GetReflectionTexture(eyeIndex, screenUV + noiseOffset);
		
#elif defined _AKMU_MIRROR_RIPPLE

	//else if (_EnableRipple > 0){

		//float2 temp_cast_0 = (_RainDrops_Tile).xx;                                                //RAIN
	float2 temp_cast_0 = (_RippleSize).xx;                                              //RAIN
	float2 uv_TexCoord53 = uv * temp_cast_0;                                      //RAIN
	float2 appendResult57 = (float2(frac(uv_TexCoord53.x), frac(uv_TexCoord53.y))); //RAIN
	// *** BEGIN Flipbook UV Animation vars ***
	// Total tiles of Flipbook Texture
	float fbtotaltiles58 = 8.0 * 8.0;                                                       //RAIN
	// Offsets for cols and rows of Flipbook Texture
	float fbcolsoffset58 = 1.0f / 8.0;                                                      //RAIN
	float fbrowsoffset58 = 1.0f / 8.0;                                                      //RAIN
	// Speed of animation
	//float fbspeed58 = _Time[1] * _RainSpeed;                                              //RAIN
	float fbspeed58 = _Time.y * _RippleSpeed;                                              //RAIN
	// UV Tiling (col and row offset)
	float2 fbtiling58 = float2(fbcolsoffset58, fbrowsoffset58);                             //RAIN
	// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
	// Calculate current tile linear index
	float fbcurrenttileindex58 = round(fmod(fbspeed58 + 0.0, fbtotaltiles58));          //RAIN
	fbcurrenttileindex58 += (fbcurrenttileindex58 < 0) ? fbtotaltiles58 : 0;                //RAIN
	// Obtain Offset X coordinate from current tile linear index
	float fblinearindextox58 = round(fmod(fbcurrenttileindex58, 8.0));              //RAIN
	// Multiply Offset X by coloffset
	float fboffsetx58 = fblinearindextox58 * fbcolsoffset58;                                //RAIN
	// Obtain Offset Y coordinate from current tile linear index
	float fblinearindextoy58 = round(fmod((fbcurrenttileindex58 - fblinearindextox58) / 8.0, 8.0));//RAIN
	// Reverse Y to get tiles from Top to Bottom
	fblinearindextoy58 = (int)(8.0 - 1) - fblinearindextoy58;                                   //RAIN
	// Multiply Offset Y by rowoffset
	float fboffsety58 = fblinearindextoy58 * fbrowsoffset58;                                //RAIN
	// UV Offset
	float2 fboffset58 = float2(fboffsetx58, fboffsety58);                                   //RAIN
	// Flipbook UV
	half2 fbuv58 = appendResult57 * fbtiling58 + fboffset58;                                //RAIN
	// *** END Flipbook UV Animation vars ***
	//float4 temp_output_63_0 = (tex2D(_Mask, customUVs39, float2(0, 0), float2(0, 0)) * i.vertexColor);
	
#ifdef _AKMU_MIRROR_MASK
	//half3 ripNor = SAMPLE_TEXTURE2D(_RippleTex, sampler_LinearRepeat, fbuv58) * _RippleDensity;;
	half3 ripNor = SampleNormal(fbuv58, TEXTURE2D_ARGS(_RippleTex, sampler_LinearRepeat)) * _RippleDensity;
	//half3 bump = SampleNormal(uv / _ReflectionRefraction, TEXTURE2D_ARGS(_RefractionTex, sampler_LinearRepeat), 1);
	
	float3 lerpResult61 = lerp(                                                             //RAIN
		surfaceData.normalTS,
		ripNor,
		mask.a);
	surfaceData.normalTS = lerpResult61;                                                              //RAIN
#else
	surfaceData.normalTS = SAMPLE_TEXTURE2D(_RippleTex, sampler_LinearRepeat, fbuv58) * _RippleDensity;
	surfaceData.normalTS = SampleNormal(fbuv58, TEXTURE2D_ARGS(_RippleTex, sampler_LinearRepeat)) * _RippleDensity;
#endif

	input2 = (surfaceData.normalTS + (_RippleRefraction / distance)); //use as static //so far away pixels will not be refracted very much
	input1 = uv - input2;//2_MainTex_ST; //will be used to animate real texture
	screenUV.xy -= input2;

	reflection = GetReflectionTexture(eyeIndex, screenUV);
#else
	reflection = GetReflectionTexture(eyeIndex, screenUV);
#endif

	//update normals
	
#ifdef _AKMU_MIRROR_REFRACTION
	#if UNITY_VERSION >= 201900
		half3 bump = SampleNormal(uv / _ReflectionRefraction, TEXTURE2D_ARGS(_RefractionTex, sampler_LinearRepeat), 1);
	#else
		half3 bump = SampleNormal(uv / _ReflectionRefraction, TEXTURE2D_PARAM(_RefractionTex, sampler_LinearRepeat), 1);
	#endif

	surfaceData.normalTS =
		_ReflectionIntensity > 0 ?
		(
			mask.a > 0 ?
			bump * float3(0, 0, mask.a)
			:
			surfaceData.normalTS
			)
		:
		surfaceData.normalTS;
#endif

#ifdef _AKMU_MIRROR_WAVE
	//recalculate albedo with similar waves:)
	if (_ReflectionIntensity > 0)
	{

		input1 = uv + noiseOffset * mask.a;//will be used to animate real texture

		surfaceData.albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input1);
	}
#elif defined _AKMU_MIRROR_RIPPLE
	//recalculate albedo with similar ripples:)
	if (_ReflectionIntensity > 0) 
	{
		#if UNITY_VERSION >= 201900
				surfaceData.albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input1);
		#else
				surfaceData.albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input1);
		#endif
	}
#endif

//lerp reflection  
//reflection = lerp(reflection1, reflection2, input.eyeIndex);

	if (_EnableDepthBlur > 0)
	{
		_ReflectionIntensity = reflection.a * _ReflectionIntensity; //alpha value will be set on depth blur shader
		//return 0; //test!! close it 
		//surfaceData.alpha = 1; //test
		surfaceData.alpha = _ReflectionIntensity;
	}
	else if (_EnableSimpleDepth > 0)
	{
		float sceneDepthAtFrag = GetDepthTexture(eyeIndex, screenUV).r;

#if UNITY_REVERSED_Z
		sceneDepthAtFrag = 1 - LinearEyeDepth(sceneDepthAtFrag, _ZBufferParams);
#else
		sceneDepthAtFrag = LinearEyeDepth(sceneDepthAtFrag, _ZBufferParams);
#endif

		float x, y, z, w; //pass camera clipping planes to shader
#if UNITY_REVERSED_Z //SHADER_API_GLES3 // insted of UNITY_REVERSED_Z
		x = -1.0 + _NearClip / _FarClip;
		y = 1;
		z = x / _NearClip;
		w = 1 / _NearClip;
#else
		x = 1.0 - _NearClip / _FarClip;
		y = _NearClip / _FarClip;
		z = x / _NearClip;
		w = y / _NearClip;
#endif

		sceneDepthAtFrag = 1.0 / (z * sceneDepthAtFrag + w);

        float depth = sceneDepthAtFrag;

		depth = clamp(pow(depth, _SimpleDepthCutoff * distance), 0., 1.);
       
        _ReflectionIntensity = depth * _ReflectionIntensity; //change reflection intensity!!  //!!!!!!!!!!!!!!!!! multiply wiht intensity to give the ability to user the reflection amount!

        surfaceData.alpha = depth;
    }

	//if(_MixBlackColor > 0){
	//    float3 check = float3(0.005, 0.005, 0.005); //todo: why 0 does not work???
	//    //if (all(check.rgb == reflection.rgb)){
	//    if (check.r > reflection.r && check.g > reflection.g && check.b > reflection.b){
	//        //reflection.rgb = diffColor.rgb;
	//        _ReflectionIntensity = 0;
	//    } 
	//}

	//in black color mixing (probe mixing) clear color alpha is zero! But 

	if (_MixBlackColor > 0) {
		if (reflection.a <= 0) {
			_ReflectionIntensity = 0;
		}
	}

	if (_WorkType == 30. && all(reflection.rgb == float3(0,0,0)))
	{
		_ReflectionIntensity = 0;
		surfaceData.alpha = 0;
	}

#ifdef _AKMU_MIRROR_MASK
	reflection = reflection * _ReflectionIntensity * pow(mask.a, _MaskEdgeDarkness);
#endif
	
	return reflection;
}

float MirrorDistance(float4 positionOS)
{
	return distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, positionOS));
}

void MirrorFresnel(float3 normalWS, half3 viewDirectionWS)
{
	//is fresnel active?
	if (_UseFresnel > 0.5)
	{
		half NoV = saturate(dot(normalWS, viewDirectionWS));
		half fresnelTerm = Pow4(1.0 - NoV);
		_ReflectionIntensity *= fresnelTerm;
	}
}

half3 MirrorEmission(SurfaceData surfaceData, float4 reflection)
{
	return surfaceData.emission* (1 - _ReflectionIntensity) + reflection * _ReflectionIntensity;
}

half3 MirrorFullColor(half3 color, float4 reflection)
{
	return lerp(color, reflection, _ReflectionIntensity);
}

#endif
