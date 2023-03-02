using UnityEngine;
using System.IO;
using System;

public class WaveSpectrumExample : MonoBehaviour
{
    public ComputeShader fftShader;
    public ComputeShader philipsSpectrum;
    public ComputeShader timeDependentSpectrum;
    public ComputeShader twiddleIndices;
    public ComputeShader IFFT;
    private ComputeBuffer indices;
    [Range(0.0001f,5)]
    public float noiseScale;
    [Range(0.0001f, 1000)]
    public float simulationScale;
    public float Constant_A;
    public float windSpeed;
    public float thresHoldHigh;
    public float threshHoldLow;

    public int Size;
    public Texture2D test;
    public RenderTexture noiseToBake;
    public Texture2D gaussianNoise_1;
    public Texture2D gaussianNoise_2;
    public RenderTexture H0;
    //public RenderTexture H_K_T;
    public RenderTexture WavesData;
    public RenderTexture butterflyTexture;
    public RenderTexture displacement;

    public RenderTexture Buffer_;

    public RenderTexture Dx_Dz;
    public RenderTexture Dy_Dxz;
    public RenderTexture Dyx_Dyz;
    public RenderTexture Dxx_Dzz;

    public float depth; // the overall water depth


    void Start()
    {
        //Create all renderTextures
        H0 = AssetSource.CreateRenderTexture(Size, Size, 2, FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);
        Buffer_ = AssetSource.CreateRenderTexture(Size, Size, 2, FilterMode.Trilinear, TextureWrapMode.Repeat, true, RenderTextureFormat.RGFloat);
        Dx_Dz = AssetSource.CreateRenderTexture(Size, Size, 6, FilterMode.Trilinear, TextureWrapMode.Repeat, true, RenderTextureFormat.RGFloat);
        Dy_Dxz = AssetSource.CreateRenderTexture(Size, Size, 6, FilterMode.Trilinear, TextureWrapMode.Repeat, true, RenderTextureFormat.RGFloat);
        Dyx_Dyz = AssetSource.CreateRenderTexture(Size, Size, 6, FilterMode.Trilinear, TextureWrapMode.Repeat, true, RenderTextureFormat.RGFloat);
        Dxx_Dzz = AssetSource.CreateRenderTexture(Size, Size, 6, FilterMode.Trilinear, TextureWrapMode.Repeat, true, RenderTextureFormat.RGFloat);
        WavesData = AssetSource.CreateRenderTexture(Size, Size, 2, FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);
        butterflyTexture = AssetSource.CreateRenderTexture(Size/32, Size, 0, FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);
        displacement = AssetSource.CreateRenderTexture(Size, Size, 6, FilterMode.Trilinear, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);
        noiseToBake = AssetSource.CreateRenderTexture(Size, Size, 10, FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);

        int timeSpectrumKernel = timeDependentSpectrum.FindKernel("TimeDependentSpectrum");
        int philipsKernel = philipsSpectrum.FindKernel("PhilipsSpectrum");
        int twiddleKernel = twiddleIndices.FindKernel("GenerateButterflyTexture");

        philipsSpectrum.SetTexture(philipsKernel, "H0", H0);
        philipsSpectrum.SetTexture(philipsKernel, "GaussianNoise1", gaussianNoise_1);
        philipsSpectrum.SetTexture(philipsKernel, "GaussianNoise2", gaussianNoise_2);
        philipsSpectrum.SetTexture(philipsKernel, "NoiseToBake", noiseToBake);
        philipsSpectrum.SetTexture(philipsKernel, "WavesData", WavesData);
        philipsSpectrum.SetFloat("NoiseScale", noiseScale);
        philipsSpectrum.SetFloat("N", H0.width);
        philipsSpectrum.SetFloat("L", simulationScale);
        philipsSpectrum.SetFloat("A", Constant_A);
        philipsSpectrum.SetVector("w", new Vector2(1, 1));
        philipsSpectrum.SetFloat("windSpeed", windSpeed);
        philipsSpectrum.SetFloat("threshHoldHigh", thresHoldHigh);
        philipsSpectrum.SetFloat("threshHoldLow", threshHoldLow);
        philipsSpectrum.SetFloat("waterDepth", depth);

        timeDependentSpectrum.SetTexture(timeSpectrumKernel, "WavesData", WavesData);
        timeDependentSpectrum.SetTexture(timeSpectrumKernel, "H0", H0);
        timeDependentSpectrum.SetTexture(timeSpectrumKernel, "Dx_Dz", Dx_Dz);
        timeDependentSpectrum.SetTexture(timeSpectrumKernel, "Dy_Dxz", Dy_Dxz);
        timeDependentSpectrum.SetTexture(timeSpectrumKernel, "Dyx_Dyz", Dyx_Dyz);
        timeDependentSpectrum.SetTexture(timeSpectrumKernel, "Dxx_Dzz", Dxx_Dzz);
        timeDependentSpectrum.SetFloat("L", simulationScale);
        timeDependentSpectrum.SetFloat("N", H0.width);

        twiddleIndices.SetFloat("N", butterflyTexture.height);
        twiddleIndices.SetTexture(twiddleKernel, "Result", butterflyTexture);

        //Prepare data to feed the twiddle indices shader
        indices = new ComputeBuffer(256, sizeof(float) * 4);
        int[] bitReverseTable = CreateBitReverseTable(butterflyTexture.height);
        indices.SetData(bitReverseTable);
        twiddleIndices.SetBuffer(twiddleKernel, "indices", indices);

        // Dispatch compute shaders
        philipsSpectrum.Dispatch(philipsKernel, H0.width / 8, H0.height / 8, 1);
        //timeDependentSpectrum.Dispatch(timeSpectrumKernel, H_K_T.width / 8, H_K_T.height / 8, 1);
        twiddleIndices.Dispatch(twiddleKernel, butterflyTexture.width, butterflyTexture.height / 8, 1);
        GameObject.Find("RenderTexture").GetComponent<AssetSource>().renderTexture = noiseToBake;
    }

    private void Update()
    {      
        //int FFTKernalHorizontal = FFT.FindKernel("HorizontalFFT");
        //int FFTKernalVertical = FFT.FindKernel("VerticalFFT");
        int timeSpectrumKernel = timeDependentSpectrum.FindKernel("TimeDependentSpectrum");
        timeDependentSpectrum.SetFloat("t", Time.time);
        timeDependentSpectrum.Dispatch(timeSpectrumKernel, H0.width / 8, H0.height / 8, 1);
        int logSize = (int)Mathf.Log(Size, 2);

        fftShader.SetTexture(0, "butterflyTexture", butterflyTexture);
        fftShader.SetTexture(0, "hkt", Dx_Dz);
        fftShader.SetTexture(0, "heightField", Buffer_);
        fftShader.SetInt("logSize", logSize);
        fftShader.SetInt("Size", Size);
        fftShader.Dispatch(0, Size / 8, Size / 8, 1);
        /*
        for (int i = 0; i < logSize; i++)
        {
            int dispatchSize = Size / (int)Mathf.Pow(2, i);
            pingPong = !pingPong;
            fftShader.SetInt("Step", i);
            fftShader.SetBool("PingPong", pingPong);
            fftShader.Dispatch(0, dispatchSize, dispatchSize, 1);
        }

        fftShader.SetTexture(1, "butterflyTexture", butterflyTexture);
        fftShader.SetTexture(1, "Buffer0", Dx_Dz);
        fftShader.SetTexture(1, "Buffer1", Buffer_);

        for (int i = 0; i < logSize; i++)
        {
            int dispatchSize = Size / (int)Mathf.Pow(2, i);
            pingPong = !pingPong;
            fftShader.SetInt("Step", i);
            fftShader.SetBool("PingPong", pingPong);
            fftShader.Dispatch(1, dispatchSize, dispatchSize, 1);
        }

        fftShader.SetInt("Size", Size);
        fftShader.SetTexture(4, "Buffer0", Buffer_);
        fftShader.Dispatch(4, Size / 8, Size / 8, 1);*/

        GameObject.Find("Plane").GetComponent<Renderer>().sharedMaterial.SetTexture("_RenderTexture", butterflyTexture);
    }   

    //To make the Inverse Fourrier Transform of h(kx,kz,t) aka our time dependent spectrum, we reorder the indices from 1 to N by bit reversing them
    //That way, some values already computed at earlier stage in the algorithm will be reused, inducing an overal O(N*Log2(N))
    //See sources for more details
    public static int[] CreateBitReverseTable(int N) //O(n) -> creates a bit reversed lookup table up until integer N
    {
        int[] bitReverseTable = new int[N];
        int bitCount = (int)Mathf.Log(N, 2);
        /*O(n) * O(1) = O(n) -> relatively efficient. The lookup table can be generated once, so that if
            the data needs to be modified and the index reordered again, the lookup table will stay the same
         */
        for (int i = 0; i < N; i++) 
        {
            bitReverseTable[i] = BitReverse(i, bitCount);
        }
        return bitReverseTable;
    }

    public static int BitReverse(int value, int bitCount) //O(1)  for 8-bit  integers
    {
        int reversed = 0;
        for (int i = 0; i < bitCount; i++)
        {
            reversed = (reversed * 2) + (value % 2);
            value /= 2;
        }
        return reversed;
    }

    //Get rid of the buffer when not needed to free memory
    private void OnDisable()
    {
        indices.Release();
        indices.Release();
    }
}
