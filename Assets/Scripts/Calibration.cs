using UnityEngine;
using UnityEngine.SceneManagement;

public class Calibration : MonoBehaviour
{
    private Camera _mainCamera;
    [SerializeField]
    private MeasureDepth measureDepth;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    public void CancelSettings()
    {
        GoToMainMenu();
    }

    // Save player settings
    public void SaveSettings()
    {
        // Save camera position
        PlayerPrefs.SetFloat("CameraPositionX", _mainCamera.transform.position.x);
        PlayerPrefs.SetFloat("CameraPositionY", _mainCamera.transform.position.y);
        PlayerPrefs.SetFloat("CameraPositionZ", _mainCamera.transform.position.z);

        // Save camera size
        PlayerPrefs.SetFloat("CameraSize", _mainCamera.orthographicSize);

        PlayerPrefs.SetFloat("xRotation", measureDepth.xRotation);
        PlayerPrefs.SetFloat("yRotation", measureDepth.yRotation);

        // Save PlayerPrefs to disk
        PlayerPrefs.Save();

        GoToMainMenu();
    }

    // Swithc scene to MainMenu scene
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }


}
