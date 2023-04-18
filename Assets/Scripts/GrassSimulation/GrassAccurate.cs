using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassAccurate : MonoBehaviour
{
    public Material material;
    public float influenceDistance = 0.5f;
    public float force = 1f;
    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        material.SetVector("_PlayerPosition", player.transform.position);
        material.SetVector("_PlayerVelocity", player.GetComponent<Rigidbody>().velocity);
        material.SetFloat("_InfluenceDistance", influenceDistance);
        material.SetFloat("_Force", force);
    }
}
