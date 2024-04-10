using UnityEngine;
using UnityEngine.SceneManagement;

public class Calibration : MonoBehaviour
{
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    public void CancelCameraSettings()
    {
        GoToMainMenu();
    }

    public void SaveCameraSettings()
    {
        // Save camera position
        PlayerPrefs.SetFloat("CameraPositionX", _mainCamera.transform.position.x);
        PlayerPrefs.SetFloat("CameraPositionY", _mainCamera.transform.position.y);
        PlayerPrefs.SetFloat("CameraPositionZ", _mainCamera.transform.position.z);

        // Save camera size
        PlayerPrefs.SetFloat("CameraSize", _mainCamera.orthographicSize);

        // Save PlayerPrefs to disk
        PlayerPrefs.Save();

        GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }


}
