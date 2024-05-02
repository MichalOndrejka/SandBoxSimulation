using UnityEngine;

public class MeasureDepth : MonoBehaviour
{
    [SerializeField]
    private MultiSourceManager multiSourceManager;
    [SerializeField]
    private UpdateTerrainData updateTerrainData;

    public Texture2D terrainTexture;

    public ushort maxDepth = 1250;
    public ushort minDepth = 900;

    public ushort[] depthData;
    public ushort[] rawDepthData;

    public readonly Vector2Int depthResolution = new Vector2Int(512, 424);

    [SerializeField]
    private float measureFrequency;
    float _time;

    public float xRotation;
    public float yRotation;

    [SerializeField]
    private bool enableDepthCapture;


    private void Start()
    {
        _time = 0f;
        loadRotationPresets();
        initializeDepthData();
    }

    private void Update()
    {
        rawDepthData = multiSourceManager.GetDepthData();
        ApplyRotation();
        _time += Time.deltaTime;

        if (_time / Time.timeScale > measureFrequency)
        {
            updateTerrainObject();
            _time -= measureFrequency * Time.timeScale;
        }

        if (!enableDepthCapture) return;

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

    private void loadRotationPresets()
    {
        xRotation = PlayerPrefs.GetFloat("xRotation", 0);
        yRotation = PlayerPrefs.GetFloat("yRotation", 0);
    }

    private void updateTerrainObject()
    {
        processDepthData();
        updateTerrainData.UpdateTerrain();
    }

    // Function for trasholding raw depth data and smoothening
    private void processDepthData()
    {
        for (int i = 0; i < rawDepthData.Length; i++)
        {
            if (rawDepthData[i] < minDepth || rawDepthData[i] > maxDepth) continue;
            // Calculate indices of current processed depth
            int x = i % depthResolution.x;
            int y = i / depthResolution.x;

            // Perform linear interpolation (smoothening) using depths of neighbors
            float sum = 0f;
            int count = 0;

            ushort depth;

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

    // Initialize depth data as an array of zeroes
    private void initializeDepthData()
    {
        depthData = new ushort[depthResolution.x * depthResolution.y];
        for (int i = 0;i < depthResolution.x * depthResolution.y; i++)
        {
            depthData[i] = 0;
        }
    }

    // Apply rotation to depth data
    private void ApplyRotation()
    {
        ushort[] rotatedDepths = new ushort[depthResolution.x * depthResolution.y];

        // Calculate indecies
        int row = 0 - depthResolution.y / 2;
        int col = 0 - depthResolution.x / 2;

        for (int i = rawDepthData.Length - 1; i >= 0; i--)
        {

            ushort depth = rawDepthData[i];

            // Update indecies
            if ((i + 1) % depthResolution.x == 0)
            {
                row++;
                col = 0 - depthResolution.x / 2;
            }

            col++;

            // Ignore depths with value zero, because of Kinect imperfection
            if (depth == 0) continue;

            // The game object is rotated so the x and y need to be swapped in order to represent the word space coordinates
            float xScalingFactor = yRotation / depthResolution.y * row;
            float yScalingFactor = xRotation / depthResolution.x * col;
            rotatedDepths[i] = (ushort)(depth + xScalingFactor + yScalingFactor);
        }

        // Assing rotated depths
        rawDepthData = rotatedDepths;
    }

    // Function for making PNG grayscale images using depth data
    private void MakeDepthPicture(string label)
    {
        // Create image (texture)
        terrainTexture = new Texture2D(depthResolution.x, depthResolution.y, TextureFormat.RGB24, false);

        // Loop through depth data, normalize them and set pixel of terrainTexture to a grayscale value
        for (int i = 0; i < rawDepthData.Length; i++)
        {
            float normalizedDepth = Mathf.InverseLerp(maxDepth, 0, rawDepthData[i]) * 255f;
            byte grayscaleValue = (byte)Mathf.Clamp(normalizedDepth, 0f, 255f);

            Color pixelColor = new Color(grayscaleValue / 255f, grayscaleValue / 255f, grayscaleValue / 255f);
            int x = i % depthResolution.x;
            int y = i / depthResolution.x;
            terrainTexture.SetPixel(x, y, pixelColor);
        }

        // Apply changes
        terrainTexture.Apply();

        // Generate a unique key for the image filename
        string uniqueKey = System.Guid.NewGuid().ToString();

        // Create the file path using the label and unique key
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

        Debug.Log($"Depth image saved: {filePath}");
    }
}
