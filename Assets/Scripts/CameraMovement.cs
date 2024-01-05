using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector3 camOffset = new Vector3(30, 37, -30);
    private const float moveSpeed = 30;


    private void Start()
    {
        transform.position = camOffset;
    }

    private void Update()
    {
        Vector3 moveDir = Vector3.zero;

        if(Input.GetKey(KeyCode.W)) { moveDir.x--; moveDir.z++; }
        if(Input.GetKey(KeyCode.S)) { moveDir.x++; moveDir.z--; }
        if(Input.GetKey(KeyCode.A)) { moveDir.x--; moveDir.z--; }
        if(Input.GetKey(KeyCode.D)) { moveDir.x++; moveDir.z++; }

        transform.position += moveSpeed * Time.deltaTime * moveDir.normalized;
    }
}
