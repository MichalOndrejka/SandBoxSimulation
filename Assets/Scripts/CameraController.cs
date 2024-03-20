using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 10f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get input for movement
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        float upDownInput = Input.GetAxis("UpDown");

        // Calculate zoom amount
        float newSize = Mathf.Clamp(Camera.main.orthographicSize - upDownInput * moveSpeed * Time.deltaTime, 1f, Mathf.Infinity);

        // Update the orthographic size of the camera
        Camera.main.orthographicSize = newSize;

        // Calculate movement direction
        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, upDownInput).normalized;

        // Move the camera
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
}
