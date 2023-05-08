using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavierStokesSim : MonoBehaviour
{
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
        fluidTexture = AssetSource.CreateRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);
        inputTexture = AssetSource.CreateRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.ARGBFloat);
        fluidCamera.targetTexture = inputTexture;
        u = AssetSource.CreateRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        v = AssetSource.CreateRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        u_prev = AssetSource.CreateRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat); ;
        v_prev = AssetSource.CreateRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        density = AssetSource.CreateRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        density_prev = AssetSource.CreateRenderTexture(simSize, simSize, 0,
            FilterMode.Point, TextureWrapMode.Repeat, true, RenderTextureFormat.RFloat);
        FetchShaderKernels();
        SetShaderTextures();
        shader.SetFloat("N", simSize - 2);
        shader.SetFloat("diff", diffusion);
        shader.SetFloat("visc", viscosity);
        rend.material.SetTexture("_FluidTexture", fluidTexture);
    }
    private void Update()
    {
        //2D texture requires no more than 2 dimensions worth of GPU threads
        shader.SetFloat("time", Time.time);
        shader.SetFloat("dt", Time.deltaTime); //time between each frame
        DispatchKernels();                                             
    }

    private void FetchShaderKernels()
    {
        mainKernels.Add(shader.FindKernel("Update_Fluid_Input"));
        mainKernels.Add(shader.FindKernel("Diffuse_Vx"));
        //boundary Vx
        boundaryVx = shader.FindKernel("Set_Boundary_Vx");
        boundaryKernels.Add(boundaryVx);
        mainKernels.Add(shader.FindKernel("Diffuse_Vy"));
        //boundary Vy
        boundaryVy = shader.FindKernel("Set_Boundary_Vy");
        boundaryKernels.Add(boundaryVy);
        mainKernels.Add(shader.FindKernel("Project_Forward"));
        mainKernels.Add(shader.FindKernel("Set_Boundary_Div_Forward"));
        mainKernels.Add(shader.FindKernel("Set_Boundary_P_Forward"));
        mainKernels.Add(shader.FindKernel("Solve_Poisson_Forward"));
        mainKernels.Add(shader.FindKernel("UpdateVelocities_Forward"));
        mainKernels.Add(shader.FindKernel("Advect_Vx"));
        //boundary Advect Vx
        boundaryAdvectVx = shader.FindKernel("Set_Boundary_Advect_Vx");
        boundaryKernels.Add(boundaryAdvectVx);
        mainKernels.Add(shader.FindKernel("Advect_Vy"));
        //boundary Advect Vy
        boundaryAdvectVy = shader.FindKernel("Set_Boundary_Advect_Vy");
        boundaryKernels.Add(boundaryAdvectVy);
        mainKernels.Add(shader.FindKernel("Project_Backward"));
        mainKernels.Add(shader.FindKernel("Set_Boundary_Div_Backward"));
        mainKernels.Add(shader.FindKernel("Set_Boundary_P_Backward"));
        mainKernels.Add(shader.FindKernel("Solve_Poisson_Backward"));
        mainKernels.Add(shader.FindKernel("UpdateVelocities_Backward"));
        mainKernels.Add(shader.FindKernel("Diffuse_Density"));
        mainKernels.Add(shader.FindKernel("Set_Boundary_Diffuse_Density"));
        mainKernels.Add(shader.FindKernel("Advect_Density"));
        mainKernels.Add(shader.FindKernel("Set_Boundary_Advect_Density"));
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

    private void DispatchKernels()
    {
        shader.Dispatch(mainKernels[0], simSize / 8, simSize / 8, 1);
        shader.Dispatch(mainKernels[mainKernels.Count - 1], simSize / 8, simSize / 8, 1);


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
