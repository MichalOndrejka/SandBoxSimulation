using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Calibration : MonoBehaviour
{
    private Camera _mainCamera;

    private float _defaultX = 442.97f;
    private float _defaultY = 654.2f;
    private float _defaultZ = 545.7f;
    private float _defaultSize = 205.6f;

    private void Start()
    {
        _mainCamera = Camera.main;
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
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
