using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RendererWatcher : MonoBehaviour
{
    private void OnBecameInvisible()
    {
        Debug.Log("Renderer became invisible");
    }

    private void OnBecameVisible()
    {
        Debug.Log("Renderer became visible");
    }
}
