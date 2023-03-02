#include "PerlinNoise.hlsl"
static const float PI = 3.1415926;

//https://www.alanzucconi.com/2015/09/16/how-to-sample-from-a-gaussian-distribution/
//From uniform to Gaussian distribution

float2 gaussRandom(float2 uv, float noiseScale, float texSize) //Random Gaussian distribution of points on a 2D plane
{
    float noise_00, noise_01;
    Unity_SimpleNoise_float(uv, noiseScale, noise_00);
    Unity_SimpleNoise_float(uv, noiseScale, noise_01);
       
    float u0 = 2 * PI * texSize * noise_00;
    float v0 = sqrt(-2 * log(noise_01));
   
    float2 randVect = float2(v0 * cos(u0), v0 * sin(u0)); 
    return randVect;
}