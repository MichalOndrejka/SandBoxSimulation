using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UpdateTerrainScript : MonoBehaviour
{
    private Terrain _terrain;

    [SerializeField]
    private MeasureDepth measureDepth;

    [SerializeField]
    private float updateTime = 3f;
    float _time;

    void Start()
    {
        _time = 0f;
        _terrain = GetComponent<Terrain>();
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if (_time > updateTime)
        {
            UpdateTerrain();
            _time -= updateTime;
        }
    }

    void UpdateTerrain()
    {
        TerrainData terrainData = _terrain.terrainData;

        if (terrainData == null) return;

        terrainData.heightmapResolution = measureDepth.depthResolution.x;

        float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        // Find the minimum and maximum depth values
        ushort minDepth = ushort.MaxValue;
        ushort maxDepth = ushort.MinValue;
        foreach (ushort depth in measureDepth.depthData)
        {
            if (depth < minDepth)
                minDepth = depth;
            if (depth > maxDepth)
                maxDepth = depth;
        }

        minDepth = (ushort)(measureDepth.minDepth + 100);
        maxDepth = measureDepth.maxDepth;

        float depthRange = maxDepth - minDepth;
        for (int x = 0; x < measureDepth.depthResolution.x; x++)
        {
            for (int y = 0; y < measureDepth.depthResolution.y; y++)
            {
                int index = measureDepth.depthResolution.x * y + x;
                ushort depth = measureDepth.depthData[index];
                if (depth == 0 && index != 0) depth = measureDepth.depthData[index - 1];

                float normalizedDepth = (float) 1 - ((depth - minDepth) / depthRange);
                normalizedDepth = normalizedDepth / 3;
                heights[x, measureDepth.depthResolution.y - 1 - y] = normalizedDepth; // Assign normalized depth as height

            }

        }
        terrainData.SetHeights(0, 0, heights);
        Debug.Log("TerrainData updated");
    }
}