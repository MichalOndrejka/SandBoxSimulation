using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSimulationScript : MonoBehaviour
{
    public Texture2D waterTexture;

    [SerializeField]
    private MeasureDepth measureDepth;
    private Vector2Int _depthResolution;

    private int waterRadius = 1;
    private float waterSpeed = 0.1f;
    private uint waterMass = 1;

    private bool animating = false;

    private uint[,] waterLocation;

    public ushort[] depthData;

    private void Start()
    {
        _depthResolution = new Vector2Int(measureDepth.depthResolution.x, measureDepth.depthResolution.y);
        waterLocation = new uint[_depthResolution.x, _depthResolution.y];

        InitializeTexture();
    }

    private void Update()
    {
        if (depthData.Length != 0) {
            Vector2Int position = (Vector2Int)GetHandPosition();
        }
    }

    private Vector2Int GetHandPosition()
    {
        Vector2Int handPosition = new Vector2Int(-1, -1);

        int minX = 100;
        int minY = 70;
        int maxX = _depthResolution.x - 50;
        int maxY = _depthResolution.y - 100;

        ushort minDepth = 900;

        System.Random random = new System.Random();

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                int index = y * _depthResolution.x + x;
                ushort depth = depthData[index];

                if (depth < minDepth && depth != 0)
                {
                    // Generate a random number between 0 and 1
                    double randomNumber = random.NextDouble();

                    if (randomNumber <= 0.3)
                    {
                        AddWater(x, y, waterRadius);
                    }
                }
            }
        }
        UpdateWaterTexture();

        if (!animating)
        {
            InvokeRepeating(nameof(animateWater), 0f, waterSpeed);
            animating = true;
        }

        return handPosition;
    }

    private void InitializeTexture()
    {
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
    public void UpdateWaterTexture()
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

    public void AddWater(int centerX, int centerY, int radius)
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

    private void animateWater()
    {
        for (int x = 0; x < _depthResolution.x; x++)
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
        ushort currentDepth = depthData[_depthResolution.x * centerY + centerX];

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
                    ushort newDepth = depthData[_depthResolution.x * newY + newX];

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
