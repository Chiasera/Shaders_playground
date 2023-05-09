using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class AssetSource : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Texture2D texture;
    // Start is called before the first frame update
    public void SaveTexture() //for a Texture2D
    {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        var id = new System.Random();
        var timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
        File.WriteAllBytes(dirPath + "Texture#" + id.Next(0,999) + "-" + timeStamp + ".png", bytes);
        Debug.Log("Texture Saved Successfully!");
    }

    public void toTexture2D()
    {
        Vector2 dimensions = new Vector2(renderTexture.width, renderTexture.height);
        Texture2D tex = new Texture2D((int)dimensions.x, (int)dimensions.y, TextureFormat.RGFloat, false);
        tex.anisoLevel = 1;
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();
        texture = tex; ;
    }

    public static void SaveTexture(Texture2D texture, string fileName) //for a Texture2D
    {
        texture = new Texture2D(256, 256, TextureFormat.RGB24, false);
        //then Save To Disk as PNG
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        File.WriteAllBytes(dirPath + fileName + "-" + timeStamp + ".png", bytes);
    }

    public static Texture2D toTexture2D(RenderTexture renderTexture)
    {
        Vector2 dimensions = new Vector2(renderTexture.width, renderTexture.height);
        Texture2D tex = new Texture2D((int)dimensions.x, (int)dimensions.y, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();
        return tex;
    }

    public static RenderTexture Create2DRenderTexture(int size_w, int size_h, int anisoLevel,
        FilterMode filterMode, TextureWrapMode wrapMode, bool enableRandomWrite, RenderTextureFormat format) //default format
    {
        RenderTexture rt = new RenderTexture(size_w, size_h, 0,
            format, RenderTextureReadWrite.Linear);
        rt.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        rt.anisoLevel = anisoLevel; // Too high will cost a lot, might even put it to 0 if adding geometry or deformation at runtim
        rt.filterMode = filterMode; // Tri/Bilinear lerps between pixels, in our case point is more suitable
        rt.wrapMode = wrapMode; //Tiles the textures
        rt.enableRandomWrite = enableRandomWrite; // necessary to use that texture as a buffer within our compute shaders
        rt.Create();
        return rt;
    }


}
