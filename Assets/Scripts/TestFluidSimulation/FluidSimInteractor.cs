using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimInteractor : MonoBehaviour
{
    private Renderer rend;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vel = new Vector3(Mathf.Abs(rb.velocity.x), Mathf.Abs(rb.velocity.y), Mathf.Abs(rb.velocity.z));
        rend.material.SetVector("_Velocity", vel.normalized);
    }
}
