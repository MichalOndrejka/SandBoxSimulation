using UnityEngine;

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

    [SerializeField]
    private float verticalScaleFactor = 0.1f;

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
}
