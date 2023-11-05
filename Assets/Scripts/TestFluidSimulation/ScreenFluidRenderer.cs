using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(Camera))]
public class ScreenFluidRenderer : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log(Input.mousePosition.normalized);
        }
    }
}