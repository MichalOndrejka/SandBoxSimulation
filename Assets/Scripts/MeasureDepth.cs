using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MeasureDepth : MonoBehaviour
{
    [SerializeField]
    private MultiSourceManager multiSourceManager;
    [SerializeField]
    private UpdateTerrainScript updateTerrainScript;

    public Texture2D terrainTexture;

    public ushort maxDepth = 1250;
    public ushort minDepth = 900;

    public ushort[] depthData;
    public ushort[] rawDepthData;

    public readonly Vector2Int depthResolution = new Vector2Int(512, 424);

    [SerializeField]
    private float measureFrequency = 10f;
    float _time;

    public float xHeightAdjustment;
    public float yHeightAdjustment;


    private void Start()
    {
        xHeightAdjustment = PlayerPrefs.GetFloat("xHeightAdjustment", 0);
        yHeightAdjustment = PlayerPrefs.GetFloat("yHeightAdjustment", 0);
        _time = 0f;
        initializeDepthData();
    }

    private void Update()
    {
        rawDepthData = multiSourceManager.GetDepthData();
        ApplyRotation();
        _time += Time.deltaTime;
        if (_time / Time.timeScale > measureFrequency)
        {
            processDepthData();
            updateTerrainScript.UpdateTerrain();
            _time -= measureFrequency * Time.timeScale;
        }

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            MakeDepthPicture("spawn_liquid");
        } else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            MakeDepthPicture("next_design");
        } else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            MakeDepthPicture("previous_design");
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

    private void ApplyRotation()
    {
        ushort[] processedDepths = new ushort[depthResolution.x * depthResolution.y];

        int row = 0 - depthResolution.y / 2;
        int col = 0 - depthResolution.x / 2;

        for (int i = rawDepthData.Length - 1; i >= 0; i--)
        {

            ushort depth = rawDepthData[i];

            if ((i + 1) % depthResolution.x == 0)
            {
                row++;
                col = 0 - depthResolution.x / 2;
            }

            col++;


            if (depth == 0) continue;

            float rowScalingFactor = yHeightAdjustment / depthResolution.y * row;
            float colScalingFactor = xHeightAdjustment / depthResolution.x * col;
            processedDepths[i] = (ushort)(depth + rowScalingFactor + colScalingFactor);
        }

        rawDepthData = processedDepths;
    }

    private void MakeDepthPicture(string label)
    {
        terrainTexture = new Texture2D(depthResolution.x, depthResolution.y, TextureFormat.RGB24, false);

        for (int i = 0; i < rawDepthData.Length; i++)
        {
            float normalizedDepth = Mathf.InverseLerp(maxDepth, 0, rawDepthData[i]) * 255f;
            byte grayscaleValue = (byte)Mathf.Clamp(normalizedDepth, 0f, 255f);

            Color pixelColor = new Color(grayscaleValue / 255f, grayscaleValue / 255f, grayscaleValue / 255f);
            int x = i % depthResolution.x;
            int y = i / depthResolution.x;
            terrainTexture.SetPixel(x, y, pixelColor);
        }

        terrainTexture.Apply();

        // Generate a unique key for the image filename
        string uniqueKey = System.Guid.NewGuid().ToString();

        // Construct the file path using the label and unique key
        string directoryPath = $"Assets/CollectedImages/{label}/";
        string filePath = $"{directoryPath}{label}_{uniqueKey}.png";

        // Ensure the directory exists, create it if not
        if (!System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        // Save the texture as a PNG file
        byte[] textureBytes = terrainTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, textureBytes);

        // Import the newly created asset into the Unity Editor
        AssetDatabase.ImportAsset(filePath);

        Debug.Log($"Depth image saved: {filePath}");

    }
}
