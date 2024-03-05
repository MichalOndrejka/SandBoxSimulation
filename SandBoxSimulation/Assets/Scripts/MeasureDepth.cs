using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MeasureDepth : MonoBehaviour
{
    public Texture2D terrainTexture;
    public Texture2D waterTexture;

    [SerializeField]
    private ushort maxDepth = 1170;
    [SerializeField]
    private ushort minDepth = 900;
    private int waterRadius = 5;
    private float waterSpeed = 0.1f;
    private uint waterMass = 10;

    [SerializeField]
    private MultiSourceManager multiSourceManager;

    private ushort[] _depthData;
    private uint[,] waterLocation;

    private readonly Vector2Int _depthResolution = new Vector2Int(512, 424);

    private bool animating = false;

    private void Awake()
    {
        waterLocation = new uint[_depthResolution.x, _depthResolution.y];
    }

    private void Start()
    {
        InitializeTextures();
    }

    private void Update()
    {
        _depthData = multiSourceManager.GetDepthData();
        UpdateTerrainTexture();

        if (Input.GetMouseButtonDown(0))
        {
            int x = (int)(Input.mousePosition.x / Screen.width * _depthResolution.x);
            int y = (int)(Input.mousePosition.y / Screen.height * _depthResolution.y);

            AddWater(x, y, waterRadius);
            UpdateWaterTexture();

            if (!animating)
            {
                InvokeRepeating("AnimateWater", 0f, waterSpeed);
                animating = true;
            }
        }
    }

    private void InitializeTextures()
    {
        terrainTexture = new Texture2D(_depthResolution.x, _depthResolution.y, TextureFormat.RGB24, false);

        for (int x = 0; x < _depthResolution.x; x++)
        {
            for (int y = 0; y < _depthResolution.y; y++)
            {
                terrainTexture.SetPixel(x, y, Color.black);
            }
        }

        terrainTexture.Apply();

        waterTexture = new Texture2D(_depthResolution.x, _depthResolution.y, TextureFormat.RGBA4444, false);

        for (int x = 0; x < _depthResolution.x; x++)
        {
            for (int y = 0; y < _depthResolution.y; y++)
            {
                waterTexture.SetPixel(x, y, Color.clear);
            }
        }

        waterTexture.Apply();
    }

    private void UpdateTerrainTexture()
    {
        double scale = 255.0 / (maxDepth - minDepth);

        for (int x = 0; x < _depthResolution.x; x++)
        {
            for (int y = 0; y < _depthResolution.y; y++)
            {
                int index = _depthResolution.x * y + x;
                ushort depth = _depthData[index];
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

    private void UpdateWaterTexture()
    {
        for (int x = 0; x < _depthResolution.x; x++)
        {
            for (int y = 0; y < _depthResolution.y; y++)
            {
                if (waterLocation[x, y] > 0) waterTexture.SetPixel(x, y, Color.blue);
                else waterTexture.SetPixel(x, y, Color.clear);
            }
        }

        waterTexture.Apply();
    }

    private void AddWater(int centerX, int centerY, int radius)
    {
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x < 0 || x > _depthResolution.x) continue;
                if (y < 0 || y > _depthResolution.y) continue;
                if (IsInCircle(x, y, centerX, centerY, radius))
                {
                    waterLocation[x, y] += waterMass;
                }
            }
        }
    }

    private bool IsInCircle(int x, int y, int centerX, int centerY, int radius)
    {
        int distanceSquared = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);
        return distanceSquared <= radius * radius;
    }

    private void AnimateWater()
    {
        Debug.Log("Animation");
        for (int x = 0; x < _depthResolution.x ; x++)
        {
            for (int y = 0; y < _depthResolution.y; y++)
            {
                if (waterLocation[x, y] > 0)
                {
                    MoveWater(x, y);
                }
            }
        }

        UpdateWaterTexture();
    }
    private void MoveWater(int centerX, int centerY)
    {
        ushort currentDepth = _depthData[_depthResolution.x * centerY + centerX];

        if (currentDepth <= 0) return;

        int targetX = centerX;
        int targetY = centerY;
        uint targetDepth = currentDepth;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int newX = centerX + dx;
                int newY = centerY + dy;

                if (newX >= 0 && newX < _depthResolution.x && newY >= 0 && newY < _depthResolution.y)
                {
                    ushort newDepth = _depthData[_depthResolution.x * newY + newX];

                    if (newDepth > targetDepth)
                    {
                        targetX = newX;
                        targetY = newY;
                        targetDepth = newDepth;
                    }
                }
            }
        }

        waterLocation[targetX, targetY] += 1;
        waterLocation[centerX, centerY] -= 1;
    }
}
