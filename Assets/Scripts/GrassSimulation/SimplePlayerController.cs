using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerInput playerInput;
    // Start is called before the first frame update
    private Rigidbody rb;
    [SerializeField]
    private float playerSpeed = 5.0f;
    private float smoothingFactor = 0.5f;
    private Vector3 XZ_displacement;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnMovement(InputValue value)
    {
         XZ_displacement = new Vector3(value.Get<Vector2>().x, 0, value.Get<Vector2>().y);      
    }

    private void FixedUpdate()
    {
        var delta_v = rb.velocity - XZ_displacement;
        rb.velocity = Vector3.Lerp(rb.velocity, XZ_displacement * playerSpeed, smoothingFactor);
    }
}
