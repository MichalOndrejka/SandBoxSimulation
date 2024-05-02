using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Change scene to Simulation scene
    public void StartSimulation()
    {
        SceneManager.LoadScene("Simulation");
    }

    // Change scene to Calibration scene
    public void StartCalibration()
    {
        SceneManager.LoadScene("Calibration");
    }
}
