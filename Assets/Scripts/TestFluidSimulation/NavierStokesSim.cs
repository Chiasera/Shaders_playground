using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class NavierStokesSim : MonoBehaviour
{
    private const int ITER = 20;
    [SerializeField]
    private ComputeShader shader;
    [SerializeField]
    private RenderTexture fluidTexture;
    [SerializeField]
    int simSize;
    private int boundaryVx, boundaryVy, boundaryAdvectVx, boundaryAdvectVy;
    private List<int> mainKernels;
    private List<int> boundaryKernels;
    //private int initKernel;
    [SerializeField]
    float viscosity = 1.0f;
    [SerializeField]
    float diffusion = 1.0f;
    private Renderer rend;
    private RenderTexture inputTexture;
    [SerializeField] Camera fluidCamera;
    [SerializeField] RenderTexture u_prev;
    [SerializeField] RenderTexture v_prev;
    [SerializeField] RenderTexture u;
    [SerializeField] RenderTexture v;
    [SerializeField] RenderTexture density;
    [SerializeField] RenderTexture density_prev;

    private void Awake()
    {
        boundaryKernels = new List<int>();
        mainKernels = new List<int>();
        rend = GetComponent<Renderer>();
        fluidTexture = AssetSource.Create2DRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);
        inputTexture = AssetSource.Create2DRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);
        fluidCamera.targetTexture = inputTexture;
        u = AssetSource.Create2DRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        v = AssetSource.Create2DRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        u_prev = AssetSource.Create2DRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat); ;
        v_prev = AssetSource.Create2DRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        density = AssetSource.Create2DRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        density_prev = AssetSource.Create2DRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        FetchShaderKernels();
        SetShaderTextures();
        shader.SetFloat("N", simSize - 2);
        shader.SetFloat("diff", diffusion);
        shader.SetFloat("visc", viscosity);
        rend.material.SetTexture("_FluidTexture", fluidTexture);
        StartSimulation();
    }
    private void Update()
    {
        //2D texture requires no more than 2 dimensions worth of GPU threads
        shader.SetFloat("time", Time.time);
        shader.SetFloat("dt", Time.deltaTime); //time between each frame
        shader.SetFloat("diff", diffusion);
        shader.SetFloat("visc", viscosity);
    }

    private void FetchShaderKernels()
    {
        mainKernels.Add(shader.FindKernel("Update_Fluid_Input")); //0
        mainKernels.Add(shader.FindKernel("Diffuse_Vx"));//1
        //boundary Vx
        boundaryVx = shader.FindKernel("Set_Boundary_Vx");
        boundaryKernels.Add(boundaryVx);
        mainKernels.Add(shader.FindKernel("Diffuse_Vy")); //2
        //boundary Vy
        boundaryVy = shader.FindKernel("Set_Boundary_Vy");
        boundaryKernels.Add(boundaryVy);
        mainKernels.Add(shader.FindKernel("Project_Forward"));//3
        mainKernels.Add(shader.FindKernel("Set_Boundary_Div_Forward"));//4
        mainKernels.Add(shader.FindKernel("Set_Boundary_P_Forward"));//5
        mainKernels.Add(shader.FindKernel("Solve_Poisson_Forward"));//6
        mainKernels.Add(shader.FindKernel("UpdateVelocities_Forward"));//7
        mainKernels.Add(shader.FindKernel("Advect_Vx"));//8
        //boundary Advect Vx
        boundaryAdvectVx = shader.FindKernel("Set_Boundary_Advect_Vx");
        boundaryKernels.Add(boundaryAdvectVx);
        mainKernels.Add(shader.FindKernel("Advect_Vy"));//9
        //boundary Advect Vy
        boundaryAdvectVy = shader.FindKernel("Set_Boundary_Advect_Vy");
        boundaryKernels.Add(boundaryAdvectVy);
        mainKernels.Add(shader.FindKernel("Project_Backward"));//10
        mainKernels.Add(shader.FindKernel("Set_Boundary_Div_Backward"));//11
        mainKernels.Add(shader.FindKernel("Set_Boundary_P_Backward"));//12
        mainKernels.Add(shader.FindKernel("Solve_Poisson_Backward"));//13
        mainKernels.Add(shader.FindKernel("UpdateVelocities_Backward"));//14
        mainKernels.Add(shader.FindKernel("Diffuse_Density"));//15
        mainKernels.Add(shader.FindKernel("Set_Boundary_Diffuse_Density"));//16
        mainKernels.Add(shader.FindKernel("Advect_Density"));//17
        mainKernels.Add(shader.FindKernel("Set_Boundary_Advect_Density"));//18
        mainKernels.Add(shader.FindKernel("Update_Fluid_Texture"));
    }

    private void SetShaderTextures()
    {
        foreach(int kernel in mainKernels.Union(boundaryKernels))
        {
            shader.SetTexture(kernel, "FluidTexture", fluidTexture);
            shader.SetTexture(kernel, "u", u);
            shader.SetTexture(kernel, "v", v);
            shader.SetTexture(kernel, "u_prev", u_prev);
            shader.SetTexture(kernel, "v_prev", v_prev);
            shader.SetTexture(kernel, "density", density);
            shader.SetTexture(kernel, "density_prev", density_prev);
            shader.SetTexture(kernel, "InputTexture", inputTexture);
        }
    }

    public async void StartSimulation()
    {
        while (true)
        {
            await DispatchKernels();
        }
    }

    private async Task DispatchKernels()
    {
        //Get velocity input
        shader.Dispatch(mainKernels[0], simSize / 8, simSize / 8, 1);
        //await Task.Yield();
        for(int i = 0; i < ITER; i++)
        {
            shader.Dispatch(mainKernels[1], simSize / 8, simSize / 8, 1);
        }
        //await Task.Yield();
        shader.Dispatch(boundaryKernels[0], simSize / 8, simSize / 8, 1);
        for (int i = 0; i < ITER; i++)
        {
            shader.Dispatch(mainKernels[2], simSize / 8, simSize / 8, 1);
        }
        //await Task.Yield();
        shader.Dispatch(boundaryKernels[1], simSize / 8, simSize / 8, 1);
        //await Task.Yield();
        //await Task.Yield();
        for(int i = 3; i<6; i++)
        {
            shader.Dispatch(mainKernels[i], simSize / 8, simSize / 8, 1);
            //await Task.Yield();
        }
        for (int i = 0; i < ITER; i++)
        {
            shader.Dispatch(mainKernels[6], simSize / 8, simSize / 8, 1);
        }
        //await Task.Yield();
        shader.Dispatch(mainKernels[7], simSize / 8, simSize / 8, 1);
        //await Task.Yield();
        shader.Dispatch(boundaryKernels[0], simSize / 8, simSize / 8, 1);
        //await Task.Yield();
        shader.Dispatch(boundaryKernels[1], simSize / 8, simSize / 8, 1);
        // Task.Yield();
        shader.Dispatch(mainKernels[8], simSize / 8, simSize / 8, 1);
        //await Task.Yield();
        shader.Dispatch(boundaryKernels[2], simSize / 8, simSize / 8, 1);
        //await Task.Yield();
        shader.Dispatch(mainKernels[9], simSize / 8, simSize / 8, 1);
       //await Task.Yield();
        shader.Dispatch(boundaryKernels[3], simSize / 8, simSize / 8, 1);
        shader.Dispatch(mainKernels[15], simSize / 8, simSize / 8, 1);
        shader.Dispatch(mainKernels[16], simSize / 8, simSize / 8, 1);
        shader.Dispatch(mainKernels[17], simSize / 8, simSize / 8, 1);
        shader.Dispatch(mainKernels[18], simSize / 8, simSize / 8, 1);
        shader.Dispatch(mainKernels[mainKernels.Count - 1], simSize / 8, simSize / 8, 1);

        /*shader.Dispatch(mainKernels[10], simSize / 8, simSize / 8, 1);
        //await Task.Yield();
        shader.Dispatch(boundaryKernels[0], simSize / 8, simSize / 8, 1);
        //await Task.Yield();
        shader.Dispatch(boundaryKernels[1], simSize / 8, simSize / 8, 1);*/
        await Task.Yield();
        //Update Vx
        //DiffuseVelocities();
        /*ProjectVelocities();
        Misc();
        UpdateDensity();*/
    }

    private void DiffuseVelocities()
    {
       shader.Dispatch(mainKernels[1], simSize / 8, simSize / 8, 1);
       shader.Dispatch(boundaryVx, simSize / 8, simSize / 8, 1);
       shader.Dispatch(mainKernels[2], simSize / 8, simSize / 8, 1);
       shader.Dispatch(boundaryVy, simSize / 8, simSize / 8, 1);      
    }

    private void ProjectVelocities()
    {
        for (int i = 3; i <= 7; i++)
        {
            shader.Dispatch(mainKernels[i], simSize / 8, simSize / 8, 1);
        }
        shader.Dispatch(boundaryVx, simSize / 8, simSize / 8, 1);
        shader.Dispatch(boundaryVy, simSize / 8, simSize / 8, 1);
    }

    private void Misc()
    {
      shader.Dispatch(mainKernels[8], simSize / 8, simSize / 8, 1);
      shader.Dispatch(boundaryAdvectVx, simSize / 8, simSize / 8, 1);
      shader.Dispatch(mainKernels[9], simSize / 8, simSize / 8, 1);
      shader.Dispatch(boundaryAdvectVy, simSize / 8, simSize / 8, 1);
      for (int i = 10; i <= 14; i++)
      {
          shader.Dispatch(mainKernels[i], simSize / 8, simSize / 8, 1);
      }
      shader.Dispatch(boundaryAdvectVx, simSize / 8, simSize / 8, 1);
      shader.Dispatch(boundaryAdvectVy, simSize / 8, simSize / 8, 1);
    }

    private void UpdateDensity()
    {
        for (int i = 15; i <= mainKernels.Count - 1; i++)
        {
            shader.Dispatch(mainKernels[i], simSize / 8, simSize / 8, 1);
        }
    }
}
