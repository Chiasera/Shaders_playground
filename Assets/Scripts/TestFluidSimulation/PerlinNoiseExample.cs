using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseExample : MonoBehaviour
{

    public ComputeShader computeShader;
    public RenderTexture renderTexture;
    public float noiseScale;
    // Start is called before the first frame update
    void Start()
    {
        renderTexture = new RenderTexture(256, 256, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        // Set the result texture as the output of the compute shader
        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelHandle, "Result", renderTexture);


        // Set the width and height of the output texture
        computeShader.SetFloat("NoiseScale", noiseScale);
        computeShader.SetFloat("Resolution", renderTexture.width);

        // Dispatch the compute shader
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
    }

    // Update is called once per frame
}
