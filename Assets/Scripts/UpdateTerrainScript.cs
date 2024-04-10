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
        terrainData.size = new Vector3(770, terrainData.size.y, 1000);

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

        terrainData.SetHeights(0, 0, heights);
        Debug.Log("TerrainData updated");

        assignSplatMap.ApplyTexture();
    }
}
