using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    [SerializeField]
    private bool canMove;

    private Camera _mainCamera;

    private float _defaultX = 442.97f;
    private float _defaultY = 654.2f;
    private float _defaultZ = 545.7f;
    private float _defaultSize = 205.6f;

    private void Start()
    {
        _mainCamera = Camera.main;
        LoadCameraSettings();
    }

    void Update()
    {
        if (!canMove) return;
        // Get input for movement
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        float upDownInput = Input.GetAxis("UpDown");

        // Calculate zoom
        float newSize = Mathf.Clamp(Camera.main.orthographicSize - upDownInput * moveSpeed * Time.deltaTime, 1f, Mathf.Infinity);

        // Update the orthographic size of the camera
        Camera.main.orthographicSize = newSize;

        // Calculate movement direction
        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, upDownInput).normalized;

        // Move the camera
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    // Load player settings from calibration
    private void LoadCameraSettings()
    {
        // Load camera position
        float posX = PlayerPrefs.GetFloat("CameraPositionX", _defaultX);
        float posY = PlayerPrefs.GetFloat("CameraPositionY", _defaultY);
        float posZ = PlayerPrefs.GetFloat("CameraPositionZ", _defaultZ);

        Vector3 cameraPosition = new Vector3(posX, posY, posZ);
        _mainCamera.transform.position = cameraPosition;

        // Load camera size
        float cameraSize = PlayerPrefs.GetFloat("CameraSize", _defaultSize);
        _mainCamera.orthographicSize = cameraSize;
    }
}
