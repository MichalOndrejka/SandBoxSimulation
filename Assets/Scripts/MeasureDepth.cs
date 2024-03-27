using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MeasureDepth : MonoBehaviour
{
    [SerializeField]
    private MultiSourceManager multiSourceManager;
    [SerializeField]
    private UpdateTerrainScript updateTerrainScript;

    public Texture2D terrainTexture;

    public ushort maxDepth = 1170;
    public ushort minDepth = 900;

    public ushort[] depthData;

    public readonly Vector2Int depthResolution = new Vector2Int(512, 424);

    [SerializeField]
    private float measureFrequency = 10f;
    float _time;

    [SerializeField]
    private float sensorAngle = 5f;

    [SerializeField]
    private float verticalScaleFactor = 0.1f;

    [SerializeField]
    private float customTimeScale = 4f;

    private void Start()
    {
        _time = 0f;
        Time.timeScale = customTimeScale;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if (_time / customTimeScale > measureFrequency)
        {
            depthData = multiSourceManager.GetDepthData();
            updateTerrainScript.UpdateTerrain();
            _time -= measureFrequency * customTimeScale;
            Debug.Log("Updating terrain heights");
        }
    }
}
