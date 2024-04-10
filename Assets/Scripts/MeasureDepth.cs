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
    public ushort[] rawDepthData;

    public readonly Vector2Int depthResolution = new Vector2Int(512, 424);

    [SerializeField]
    private float measureFrequency = 10f;
    float _time;

    [SerializeField]
    private float sensorAngle = 5f;

    [SerializeField]
    private float verticalScaleFactor = 0.1f;

    public float customTimeScale = 4f;

    private void Start()
    {
        _time = 0f;
        Time.timeScale = customTimeScale;
        initializeDepthData();
    }

    private void Update()
    {
        rawDepthData = multiSourceManager.GetDepthData();
        _time += Time.deltaTime;
        if (_time / customTimeScale > measureFrequency)
        {
            processDepthData();
            updateTerrainScript.UpdateTerrain();
            _time -= measureFrequency * customTimeScale;
            Debug.Log("Updating terrain heights");
        }
    }

    private void processDepthData()
    {
        for (int i = 0; i < rawDepthData.Length; i++)
        {
            if (rawDepthData[i] < minDepth || rawDepthData[i] > maxDepth) continue;
            // Calculate indices of neighbors
            int x = i % depthResolution.x;
            int y = i / depthResolution.x;

            // Perform linear interpolation using neighbors
            float sum = 0f;
            int count = 0;

            ushort depth = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = Mathf.Clamp(x + dx, 0, depthResolution.x - 1);
                    int ny = Mathf.Clamp(y + dy, 0, depthResolution.y - 1);

                    int neighborIndex = nx + ny * depthResolution.x;

                    // Include neighbors in sum
                    depth = rawDepthData[neighborIndex];
                    if (depth == 0) continue;
                    sum += depth;
                    count++;
                }
            }

            // Calculate the average of neighboring depths
            float averageDepth = sum / count;

            // Assign the averaged depth to the current position
            depthData[i] = (ushort)averageDepth;
        }
    }

    private void initializeDepthData()
    {
        depthData = new ushort[depthResolution.x * depthResolution.y];
        for (int i = 0;i < depthResolution.x * depthResolution.y; i++)
        {
            depthData[i] = 0;
        }
    }
}
