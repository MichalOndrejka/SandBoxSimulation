using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AssignSplatMap;

public class UpdateTerrainScript : MonoBehaviour
{
    private Terrain _terrain;

    [SerializeField]
    private MeasureDepth measureDepth;
    [SerializeField]
    private AssignSplatMap assignSplatMap;

    void Start()
    {
        _terrain = GetComponent<Terrain>();
        assignSplatMap = GetComponent<AssignSplatMap>();
    }

    public void UpdateTerrain()
    {
        TerrainData terrainData = _terrain.terrainData;

        if (terrainData == null) return;

        terrainData.heightmapResolution = measureDepth.depthResolution.x;

        float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        // Find the minimum and maximum depth values
        ushort minDepth = (ushort)(measureDepth.minDepth + 100);
        ushort maxDepth = measureDepth.maxDepth;        

        float depthRange = maxDepth - minDepth;
        for (int x = 0; x < measureDepth.depthResolution.x; x++)
        {
            for (int y = 0; y < measureDepth.depthResolution.y; y++)
            {
                int index = measureDepth.depthResolution.x * y + x;
                ushort depth = measureDepth.depthData[index];

                if (depth == 0 && index != 0) depth = measureDepth.depthData[index - 1];

                float normalizedDepth = 1 - ((depth - minDepth) / depthRange);
                normalizedDepth = normalizedDepth / 6;
                heights[x, measureDepth.depthResolution.y - 1 - y] = normalizedDepth;

            }

        }
        
        SmoothHeights(heights, terrainData.heightmapResolution);


        terrainData.SetHeights(0, 0, heights);

        assignSplatMap.ApplyTexture((int)TextureMode.Real);
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
