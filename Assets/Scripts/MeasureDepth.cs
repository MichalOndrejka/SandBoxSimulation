using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MeasureDepth : MonoBehaviour
{
    public Texture2D terrainTexture;

    public ushort maxDepth = 1170;
    public ushort minDepth = 900;

    [SerializeField]
    private MultiSourceManager multiSourceManager;
    [SerializeField]
    private WaterSimulationScript waterSimulationScript;

    public ushort[] depthData;
    private ushort[] _rawDepthData;

    public readonly Vector2Int depthResolution = new Vector2Int(512, 424);

    private int imageNumber = 0;

    private void Start()
    {
        InitializeTexture();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CreateHeightmap();
        }
        _rawDepthData = multiSourceManager.GetDepthData();
        depthData = multiSourceManager.GetDepthData();
        waterSimulationScript.depthData = depthData;
        if (Input.GetMouseButton(0))
        {
            UpdateTerrainTexture();
        }
    }

    private void handleRawDepthData()
    {
        for(int i = 0; i < _rawDepthData.Length; i++) { 
            ushort rawDepth = _rawDepthData[i];
            if (rawDepth < minDepth) continue;
            else if (rawDepth > maxDepth) continue;
            else
            {
                depthData[i] = rawDepth;
            }
        }
    }

    private void CreateHeightmap()
    {
        Debug.Log("Creating HeightMap of image " + imageNumber);
        int width = depthResolution.x;
        int height = depthResolution.y;
        Texture2D depthTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Normalize and set the depth data to the texture
        for (int i = 0; i < depthData.Length; i++)
        {
            int x = i % width;
            int y = i / width;

            float normalizedDepth = Mathf.InverseLerp(maxDepth, 200, depthData[i]);
            Color depthColor = new Color(normalizedDepth, normalizedDepth, normalizedDepth, 1f);
            depthTexture.SetPixel(x, y, depthColor);
        }

        // Apply changes and encode the texture to PNG
        depthTexture.Apply();
        byte[] pngBytes = depthTexture.EncodeToPNG();

        // Save the PNG file
        System.IO.File.WriteAllBytes("Assets/PointGesture/Point" + imageNumber + ".png", pngBytes);

        Debug.Log("HeightMap created of image " + imageNumber);

        imageNumber++;
    }

    private void InitializeTexture()
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
                    // Calculate smooth gradient from red to green to blue based on depth
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
