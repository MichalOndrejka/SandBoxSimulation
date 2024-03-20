using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MeasureDepth : MonoBehaviour
{
    [SerializeField]
    private MultiSourceManager multiSourceManager;
    [SerializeField]
    private WaterSimulationScript waterSimulationScript;
    [SerializeField]
    private UpdateTerrainScript updateTerrainScript;

    public Texture2D terrainTexture;

    public ushort maxDepth = 1170;
    public ushort minDepth = 900;

    public ushort[] depthData;

    public readonly Vector2Int depthResolution = new Vector2Int(512, 424);

    [SerializeField]
    private float measureFrequency = 1f;
    float _time;

    private void Start()
    {
        _time = 0f;
        InitializeTerrainTexture();
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if (_time > measureFrequency)
        {
            depthData = multiSourceManager.GetDepthData();
            UpdateTerrainTexture();
            _time -= measureFrequency;
        }
    }

    private void InitializeTerrainTexture()
    {
        terrainTexture = new Texture2D(depthResolution.x, depthResolution.y, TextureFormat.RGB24, false);

        for (int x = 0; x < depthResolution.x; x++)
        {
            for (int y = 0; y < depthResolution.y; y++)
            {
                terrainTexture.SetPixel(x, y, Color.black);
            }
        }

        terrainTexture.Apply();
    }

    private void UpdateTerrainTexture()
    {
        double scale = 255.0 / (maxDepth - minDepth);

        for (int x = 0; x < depthResolution.x; x++)
        {
            for (int y = 0; y < depthResolution.y; y++)
            {
                int index = depthResolution.x * y + x;
                ushort depth = depthData[index];
                byte intensity = (byte)((depth - minDepth) * scale);

                if (depth < minDepth || depth > maxDepth)
                {
                    terrainTexture.SetPixel(x, y, Color.black);
                }
                else
                {
                    float red = Mathf.Clamp(255 - intensity, 0, 255) / 255f;
                    float green = Mathf.Clamp(intensity < 128 ? intensity * 2 : 255 - (intensity - 128) * 2, 0, 255) / 255f;
                    float blue = Mathf.Clamp(intensity >= 128 ? (intensity - 128) * 2 : 0, 0, 255) / 255f;

                    terrainTexture.SetPixel(x, y, new Color(red, green, blue));
                }
            }
        }

        terrainTexture.Apply();
    }
}
