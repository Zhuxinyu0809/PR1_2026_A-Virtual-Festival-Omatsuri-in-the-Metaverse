#ifndef UNIVERSAL_PIPELINE_COMMON_OPERATIONS_INCLUDED
#define UNIVERSAL_PIPELINE_COMMON_OPERATIONS_INCLUDED

#define UNITY_PI            3.14159265359f
#define UNITY_TWO_PI        6.28318530718f
#define UNITY_FOUR_PI       12.56637061436f
#define UNITY_INV_PI        0.31830988618f
#define UNITY_INV_TWO_PI    0.15915494309f
#define UNITY_INV_FOUR_PI   0.07957747155f
#define UNITY_HALF_PI       1.57079632679f
#define UNITY_INV_HALF_PI   0.636619772367f

#define FLT_MAX 3.402823466e+38
#define FLT_MIN 1.175494351e-38
#define DBL_MAX 1.7976931348623158e+308
#define DBL_MIN 2.2250738585072014e-308

// rotating UV
const float Deg2Rad = (UNITY_PI * 2.0) / 360.0;

inline float Linear01Depth(float z)
{
	return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
}

//gets the correct uv according to selected option by user etc.
float2 FindUV(uint uvChannel, float2 uv_0, float2 uv_1, float2 uv_2, float2 uv_3)
{
	return uvChannel == 0 ?
		uv_0
		:
		(uvChannel == 1 ?
			uv_1
			:
			(uvChannel == 2 ?
				uv_2
				: uv_3));
}


bool IsCulled(float4 normal) {
	return normal.z > 0;
}

float3 AlignToGrid(float3 pos, float size)
{
	float3 newPos = (float3)0;
	newPos.x = pos.x - fmod(pos.x, size);
	newPos.y = pos.y - fmod(pos.y, size);
	newPos.z = pos.z - fmod(pos.z, size);
	return newPos;
}

// Computes world space view direction, from object space position
inline float3 UnityWorldSpaceViewDir(in float3 worldPos)
{
	return _WorldSpaceCameraPos.xyz - worldPos;
}

float3 DepthToWorld(float2 uv, float depth) {
	float z = (1 - depth) * 2.0 - 1.0;

	float4 clipSpacePosition = float4(uv * 2.0 - 1.0, z, 1.0);

	float4 viewSpacePosition = mul(unity_CameraInvProjection, clipSpacePosition);
	viewSpacePosition /= viewSpacePosition.w;

	float4 worldSpacePosition = mul(unity_ObjectToWorld, viewSpacePosition);

	return worldSpacePosition.xyz;
}

half2 WorldToScreenPos(float3 pos) {
	pos = normalize(pos - _WorldSpaceCameraPos) * (_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y)) + _WorldSpaceCameraPos;
	half2 uv = 0;
	float3 toCam = mul(unity_WorldToCamera, pos);
	float camPosZ = toCam.z;
	float height = 2 * camPosZ / unity_CameraProjection._m11;
	float width = _ScreenParams.x / _ScreenParams.y * height;
	uv.x = (toCam.x + width / 2) / width;
	uv.y = (toCam.y + height / 2) / height;
	return uv;
}

half N21(half2 p)
{
	p = frac(p * half2(123.34, 345.45));
	p += dot(p, p + 34.345);
	return frac(p.x + p.y);
}

half3 layer(half2 UV, half T) {
	float _Size = 15.;

	//half t = fmod(_Time.y + T, 3600);
	half t = fmod(_Time.y * T, 3600);
	half4 col = half4(0, 0, 0, 1.0);
	half aspect = half2(2, 1);
	half2 uv = UV * _Size * aspect;
	uv.y += t * 0.25;
	half2 gv = frac(uv) - 0.5;//-0.5，调整原点为中间
	half2 id = floor(uv);
	half n = N21(id); // 0 1
	t += n * 6.2831; //2PI

	half w = UV.y * 10;
	half x = (n - 0.5) * 0.8;
	x += (0.4 - abs(x)) * sin(3 * w) * pow(sin(w), 6) * 0.45;
	half y = -sin(t + sin(t + sin(t) * 0.5)) * 0.45;
	y -= (gv.x - x) * (gv.x - x);
	half2 dropPos = (gv - half2(x, y)) / aspect; //- half2(x,y) 为了移动
	half drop = smoothstep(0.05, 0.03, length(dropPos));

	half2 trailPos = (gv - half2(x, t * 0.25)) / aspect; //- half2(x,y) 为了移动
	trailPos.y = (frac(trailPos.y * 8) - 0.5) / 8;
	half trail = smoothstep(0.03, 0.01, length(trailPos));
	half fogTrail = smoothstep(-0.05, 0.05, dropPos.y);// 拖尾小水滴慢慢被拖掉了
	fogTrail *= smoothstep(0.5, y, gv.y);// 拖尾小水滴渐变消失
	fogTrail *= smoothstep(0.05, 0.04, abs(dropPos.x));
	trail *= fogTrail;
	//col += fogTrail * 0.5;
	//col += trail;
	//col += drop;
	//if(gv.x > 0.48 || gv.y > 0.49) col = half4(1.0, 0, 0, 1.0); // 辅助线
	half2 offset = drop * dropPos + trail * trailPos;
	return half3(offset, fogTrail);
}

half3 RGBtoHSL(half3 color) {
	half red = color.r;
	half green = color.g;
	half blue = color.b;
	half hue;
	half saturation;
	half lightness;
	half maxChannel = max(max(red, green), blue);
	half minChannel = min(min(red, green), blue);

	/// hue
	if (maxChannel == minChannel) hue = 0;
	else if (minChannel == blue) hue = 60 * (green - red) / (maxChannel - minChannel) + 60;
	else if (minChannel == red) hue = 60 * (blue - green) / (maxChannel - minChannel) + 180;
	else if (minChannel == green) hue = 60 * (red - blue) / (maxChannel - minChannel) + 300;
	hue /= 360;

	/// saturation
	saturation = (maxChannel - minChannel) / (1 - abs(maxChannel + minChannel - 1));

	/// lightness
	lightness = (maxChannel + minChannel) / 2;

	return half3(hue, saturation, lightness);
}

half3 HSLtoRGB(half3 color) {
	half hue = color.r * 360;
	half saturation = color.g;
	half lightness = color.b;
	half3 rgbColor;
	half maxChannel = lightness + (saturation * (1 - abs(2 * lightness - 1))) / 2;
	half minChannel = lightness - (saturation * (1 - abs(2 * lightness - 1))) / 2;
	if (hue < 60) {
		rgbColor.r = maxChannel;
		rgbColor.g = minChannel + (maxChannel - minChannel) * hue / 60;
		rgbColor.b = minChannel;
	}
	else if (hue < 120) {
		rgbColor.r = minChannel + (maxChannel - minChannel) * (120 - hue) / 60;
		rgbColor.g = maxChannel;
		rgbColor.b = minChannel;
	}
	else if (hue < 180) {
		rgbColor.r = minChannel;
		rgbColor.g = maxChannel;
		rgbColor.b = minChannel + (maxChannel - minChannel) * (hue - 120) / 60;
	}
	else if (hue < 240) {
		rgbColor.r = minChannel;
		rgbColor.g = minChannel + (maxChannel - minChannel) * (240 - hue) / 60;
		rgbColor.b = maxChannel;
	}
	else if (hue < 300) {
		rgbColor.r = minChannel + (maxChannel - minChannel) * (hue - 240) / 60;
		rgbColor.g = minChannel;
		rgbColor.b = maxChannel;
	}
	else {
		rgbColor.r = maxChannel;
		rgbColor.g = minChannel;
		rgbColor.b = minChannel + (maxChannel - minChannel) * (360 - hue) / 60;
	}
	return rgbColor;
}

half3 Wetter(half3 color) {
	//float _Saturation("Saturation", Range(0, 0.2)) = 0.1
	float _Saturation = 0.1;
	//_Lightness("Lightness", Range(0.1, 1)) = 1
	float _Lightness = 1;
	half3 hslColor = RGBtoHSL(color);
	hslColor.g += _Saturation;
	hslColor.b = hslColor.b * hslColor.b * _Lightness;
	half3 wetColor = HSLtoRGB(hslColor);
	return wetColor;
}

float3 ProcessRainUv(float3 normalWS, float3 UV)
{
	if (normalWS.z < 0)
	{
		return UV;
	}
	else
	{
		float x = UV.x;
		x = -x + 1;
		return float3(x, UV.y, 0);
	}

	if (normalWS.x > 0)
	{
		return UV;
	}
	else
	{
		float x = UV.x;
		x = -x + 1;

		return float3(x, UV.y, 0);
	}
}

inline float4 ASE_ComputeGrabScreenPos(float4 pos)
{
#if UNITY_UV_STARTS_AT_TOP
	float scale = -1.0;
#else
	float scale = 1.0;
#endif
	float4 o = pos;
	o.y = pos.w * 0.5f;
	o.y = (pos.y - o.y) * _ProjectionParams.x * scale + o.y;
	return o;
}


float2 sampleCubeT(
	float3 v,
	out float faceIndex)
{
	float3 vAbs = abs(v);
	float ma;
	float2 uv;
	if (vAbs.z >= vAbs.x && vAbs.z >= vAbs.y)
	{
		faceIndex = v.z < 0.0 ? 5.0 : 4.0;
		ma = 0.5 / vAbs.z;
		uv = float2(v.z < 0.0 ? -v.x : v.x, -v.y);
	}
	else if (vAbs.y >= vAbs.x)
	{
		faceIndex = v.y < 0.0 ? 3.0 : 2.0;
		ma = 0.5 / vAbs.y;
		uv = float2(v.x, v.y < 0.0 ? -v.z : v.z);
	}
	else
	{
		faceIndex = v.x < 0.0 ? 1.0 : 0.0;
		ma = 0.5 / vAbs.x;
		uv = float2(v.x < 0.0 ? v.z : -v.z, -v.y);
	}
	return uv * ma + 0.5;
}

float getFaceIndex(float3 v)
{
	float3 vAbs = abs(v);
	float faceIndex = -1;
	/*float ma;
	float2 uv;*/
	if (vAbs.z >= vAbs.x && vAbs.z >= vAbs.y)
	{
		faceIndex = v.z < 0.0 ? 5.0 : 4.0;
		/* ma = 0.5 / vAbs.z;
		 uv = float2(v.z < 0.0 ? -v.x : v.x, -v.y);*/
	}
	else if (vAbs.y >= vAbs.x)
	{
		faceIndex = v.y < 0.0 ? 3.0 : 2.0;
		/* ma = 0.5 / vAbs.y;
		 uv = float2(v.x, v.y < 0.0 ? -v.z : v.z);*/
	}
	else
	{
		faceIndex = v.x < 0.0 ? 1.0 : 0.0;
		/*ma = 0.5 / vAbs.x;
		uv = float2(v.x < 0.0 ? v.z : -v.z, -v.y);*/
	}
	//return uv * ma + 0.5;
	return faceIndex;
}

float angleV(float3 first, float3 second)
{
	return degrees(acos(dot(normalize(first), normalize(second))));
}

float radianV(float3 first, float3 second)
{
	return dot(normalize(first), normalize(second));
}
// - based on the Golden Ratio
// - uniform normalized distribution
// - fastest static noise generator function (also runs at low precision)
// float PHI = 1.61803398874989484820459;  // Φ = Golden Ratio
float gold_noise(in float2 uv, in float seed)
{
	return frac(tan(distance(uv * 1.61803398874989484820459, uv) * seed) * uv.x);
}

static const half SQRT3_6 = sqrt(3) / 6;
//static float _MyRadius = 10;
//float4 _MyColor = float4(1,0,0,1);


float3 LocalCorrect(float3 origVec, float3 bboxMin, float3 bboxMax, float3 positionWS, float3 cubemapPos)
{
	// Find the ray intersection with box plane
	float3 invOrigVec = float3(1.0, 1.0, 1.0) / origVec;

	float3 intersecAtMaxPlane = (bboxMax - positionWS) * invOrigVec;

	float3 intersecAtMinPlane = (bboxMin - positionWS) * invOrigVec;

	// Get the largest intersection values (we are not intersted in negative values)
	float3 largestIntersec = max(intersecAtMaxPlane, intersecAtMinPlane);

	// Get the closest of all solutions
	float Distance = min(min(largestIntersec.x, largestIntersec.y), largestIntersec.z);

	// Get the intersection position
	float3 IntersectPositionWS = positionWS + origVec * Distance;

	// Get corrected vector
	float3 localCorrectedVec = IntersectPositionWS - cubemapPos;

	return localCorrectedVec;
}


float3 ODSOffset(float3 worldPos, float ipd)
{
	//based on google's omni-directional stereo rendering thread
	const float EPSILON = 2.4414e-4;
	float3 worldUp = float3(0.0, 1.0, 0.0);
	float3 camOffset = worldPos.xyz - _WorldSpaceCameraPos.xyz;
	float4 direction = float4(camOffset.xyz, dot(camOffset.xyz, camOffset.xyz));
	direction.w = max(EPSILON, direction.w);
	direction *= rsqrt(direction.w);

	float3 tangent = cross(direction.xyz, worldUp.xyz);
	if (dot(tangent, tangent) < EPSILON)
		return float3(0, 0, 0);
	tangent = normalize(tangent);

	float directionMinusIPD = max(EPSILON, direction.w * direction.w - ipd * ipd);
	float a = ipd * ipd / direction.w;
	float b = ipd / direction.w * sqrt(directionMinusIPD);
	float3 offset = -a * direction.xyz + b * tangent;
	return offset;
}

inline float4 UnityObjectToClipPosODS(float3 inPos)
{
	float4 clipPos;
	float3 posWorld = mul(unity_ObjectToWorld, float4(inPos, 1.0)).xyz;
#if defined(STEREO_CUBEMAP_RENDER_ON)
	float3 offset = ODSOffset(posWorld, unity_HalfStereoSeparation.x);
	clipPos = mul(UNITY_MATRIX_VP, float4(posWorld + offset, 1.0));
#else
	clipPos = mul(UNITY_MATRIX_VP, float4(posWorld, 1.0));
#endif
	return clipPos;
}

// Tranforms position from object to homogenous space
inline float4 UnityObjectToClipPos(in float3 pos)
{
#if defined(STEREO_CUBEMAP_RENDER_ON)
	return UnityObjectToClipPosODS(pos);
#else
	// More efficient than computing M*VP matrix product
	return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
#endif
}
inline float4 UnityObjectToClipPos(float4 pos) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
	return UnityObjectToClipPos(pos.xyz);
}

void Unity_RotateAboutAxis_Degrees_float(float3 In, float3 Axis, float Rotation, out float3 Out)
{
	Rotation = radians(Rotation);
	float s = sin(Rotation);
	float c = cos(Rotation);
	float one_minus_c = 1.0 - c;

	Axis = normalize(Axis);
	float3x3 rot_mat =
	{ one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
		one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
		one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
	};
	Out = mul(rot_mat, In);
}

float mod(float a, float b)
{
	return a - floor(a / b) * b;
}
float2 mod(float2 a, float2 b)
{
	return a - floor(a / b) * b;
}
float3 mod(float3 a, float3 b)
{
	return a - floor(a / b) * b;
}
float4 mod(float4 a, float4 b)
{
	return a - floor(a / b) * b;
}

float3 RGBToHSV(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HSVToRGB(float3 c)
{
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

//https://www.indezine.com/products/powerpoint/learn/color/color-rgb.html#:~:text=The%20three%20primary%20colors%20of,How%20does%20that%20happen%3F
static const float4 _ColorSet[12] = {
	float4(0,255, 0, 0),            //0 RED
	float4(1,255,0,125),            //1 -Red-Magenta
	float4(2,255,0,255),           //2 -Magenta
	float4(3,125,0,255),            //3 -Blue-Magenta
	float4(4,0, 0, 255),            //4 BLUE
	float4(5,0,125,255),            //5 -Cyan-Blue
	float4(6,0,255,255),            //6 -Cyan
	float4(7,0,255,125),            //7 -Cyan-Green
	float4(8,0, 255, 0),            //8 GREEN
	float4(9,125,255,0),            //9 -Yellow-Green
	float4(10,255,255,0),            //10 -Yellow
	float4(11,255,125,0)            //11 -Orange
};

int getBestMatchingColor(float3 pixelColor) {
	// largest difference is 255 for every colour component
	int currentDifference = 3 * 255;
	// name of the best matching colour
	int closestColorName = -1;
	// get int values for all three colour components of the pixel
	int pixelColorR = pixelColor.x * 255; //to 0-255
	int pixelColorG = pixelColor.y * 255;
	int pixelColorB = pixelColor.z * 255;

	for (int i = 0; i < 12; i++)
	{
		// continue iterating if the map contains a next colour and the difference is greater than zero.
		// a difference of zero means we've found an exact match, so there's no point in iterating further.
		if (currentDifference > 0) {
			// this colour's name
			int currentColorName = i;
			// this colour's value (last 3 value)
			float3 color = float3(_ColorSet[i].y, _ColorSet[i].z, _ColorSet[i].w);
			// get int values for all three colour components of this colour
			int colorR = color.x;
			int colorG = color.y;
			int colorB = color.z;
			// calculate sum of absolute differences that indicates how good this match is 
			int difference = abs(pixelColorR - colorR) + abs(pixelColorG - colorG) + abs(pixelColorB - colorB);
			// a smaller difference means a better match, so keep track of it
			if (currentDifference > difference) {
				currentDifference = difference;
				closestColorName = currentColorName;
			}
		}
	}

	return closestColorName;

	//Iterator<String> colorNameIterator = mColors.keySet().iterator();
	//// continue iterating if the map contains a next colour and the difference is greater than zero.
	//// a difference of zero means we've found an exact match, so there's no point in iterating further.
	//while (colorNameIterator.hasNext() && currentDifference > 0) {
	//    // this colour's name
	//    String currentColorName = colorNameIterator.next();
	//    // this colour's int value
	//    int color = mColors.get(currentColorName);
	//    // get int values for all three colour components of this colour
	//    int colorR = Color.red(color);
	//    int colorG = Color.green(color);
	//    int colorB = Color.blue(color);
	//    // calculate sum of absolute differences that indicates how good this match is 
	//    int difference = Math.abs(pixelColorR - colorR) + Math.abs(pixelColorG - colorG) + Math.abs(pixelColorB - colorB);
	//    // a smaller difference means a better match, so keep track of it
	//    if (currentDifference > difference) {
	//        currentDifference = difference;
	//        closestColorName = currentColorName;
	//    }
	//}
	//return closestColorName;
}

//#if defined(SHADER_API_OPENGL)  !defined(SHADER_TARGET_GLSL)
//#define UNITY_BUGGY_TEX2DPROJ4
#define UNITY_PROJ_COORD(a) a.xyw

//rotates the point according to our rotation input and returns result
float3 CreateRotation(float3 reflectVector, float4 rotation) {
	/*else {
		rotate.x = 0;
		rotate.y = 0;
	}*/
	//get the dot product over 
	//if (revert > 0)
	//{
	//    int colorName = getBestMatchingColor(output);
	//    if (colorName <= 3)
	//    {
	//        /*rotate = 0;*/
	//        rotate.y = -rotate.x;
	//        rotate.x = 0;
	//    }
	//    else if (colorName <= 6)
	//    {
	//        /*rotate = 0;*/
	//        rotate.y = rotate.x;
	//        rotate.x = 0;
	//    }
	//    

	//    ////float3 vectorMe = output * view;
	//    //int colorName = getBestMatchingColor(normalize(view));
	//    //if (colorName > 4) //5 is marked as green on our static array!
	//    //{
	//    //    rotate = 0;
	//    //}

	//    //if (output.x > rotate.z) //rotate.z zero as default
	//    //{
	//    //    rotate = 0;
	//    //    /*rotate.y = -rotate.x;
	//    //    rotate.x = 0;*/
	//    //}
	//    //float dota = dot(output, -normalize(UNITY_MATRIX_IT_MV[2].xyz)); //cam view negatif direction?
	//    //if (dota < rotate.z) {
	//    //    //rotate = 0;
	//    //    rotate.y = -rotate.x;
	//    //    rotate.x = 0;
	//    //}
	//}

	//rotate.z = 0; //todo; we will use it as a threshold

	//do z rotation first (for correct reflections)
	if (rotation.z != 0) {
		Unity_RotateAboutAxis_Degrees_float(reflectVector, float3 (0, 0, 1), -rotation.z, reflectVector);
	}
	if (rotation.x != 0) {
		Unity_RotateAboutAxis_Degrees_float(reflectVector, float3 (1, 0, 0), -rotation.x, reflectVector);
	}
	if (rotation.y != 0) {
		Unity_RotateAboutAxis_Degrees_float(reflectVector, float3 (0, 1, 0), -rotation.y, reflectVector);
	}


	return reflectVector;
}
#endif
