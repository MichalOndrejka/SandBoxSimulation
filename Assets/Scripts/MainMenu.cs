using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartSimulation()
    {
        SceneManager.LoadScene("Simulation");
    }
    public void StartCalibration()
    {
        SceneManager.LoadScene("Calibration");
    }
}
