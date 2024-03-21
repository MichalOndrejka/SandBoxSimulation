using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UpdateTerrainScript : MonoBehaviour
{
    private Terrain _terrain;

    [SerializeField]
    private MeasureDepth measureDepth;
    private AssignSplatMap _assignSplatMap;

    [SerializeField]
    private float updateTime = 3f;
    float _time;

    void Start()
    {
        _time = 0f;
        _terrain = GetComponent<Terrain>();
        _assignSplatMap = GetComponent<AssignSplatMap>(); //?
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if (_time > updateTime)
        {
            //UpdateTerrain();
            _time -= updateTime;
        }
    }

    public void UpdateTerrain()
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

        // Smooth the heights
        SmoothHeights(heights, terrainData.heightmapResolution);


        terrainData.SetHeights(0, 0, heights);
        Debug.Log("TerrainData updated");
    }

    void SmoothHeights(float[,] heights, int resolution)
    {
        for (int x = 1; x < resolution - 1; x++)
        {
            for (int y = 1; y < resolution - 1; y++)
            {
                float avgHeight = (
                    heights[x - 1, y - 1] + heights[x, y - 1] + heights[x + 1, y - 1] +
                    heights[x - 1, y] + heights[x, y] + heights[x + 1, y] +
                    heights[x - 1, y + 1] + heights[x, y + 1] + heights[x + 1, y + 1]
                ) / 9f;
                heights[x, y] = avgHeight;
            }
        }
    }
}
