using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UpdateTerrainScript : MonoBehaviour
{
    [SerializeField]
    private Texture2D sandTexture;
    [SerializeField]
    private Texture2D grassTexture;
    [SerializeField]
    private Texture2D rockTexture;
    [SerializeField]
    private Texture2D snowTexture;

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

        //ApplyTexturesBasedOnHeight();
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

    void ApplyTexturesBasedOnHeight()
    {
        TerrainData terrainData = _terrain.terrainData;
        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;
        int splatmapLayers = terrainData.alphamapLayers;

        // Ensure correct dimensions for the alphamap array
        float[,,] splatmaps = new float[heightmapWidth, heightmapHeight, splatmapLayers];

        // Loop through each point on the heightmap
        for (int x = 0; x < heightmapWidth; x++)
        {
            for (int y = 0; y < heightmapHeight; y++)
            {
                // Get the height at this point
                float height = terrainData.GetHeight(x, y);

                // Determine the blend weights for each texture based on height
                float grassWeight = Mathf.Clamp01((height - 0) / 100f); // Grass texture up to height 100
                float rockWeight = Mathf.Clamp01((height - 100f) / 100f); // Rock texture from height 100 to 200
                float snowWeight = Mathf.Clamp01((height - 200f) / 800f); // Snow texture from height 200 to 1000

                // Set the blend weights in the alphamap array
                splatmaps[x, y, 0] = grassWeight; // Grass texture
                splatmaps[x, y, 1] = rockWeight; // Rock texture
                splatmaps[x, y, 2] = snowWeight; // Snow texture

                // Ensure that the sum of blend weights does not exceed 1
                float totalBlend = grassWeight + rockWeight + snowWeight;
                if (totalBlend > 1)
                {
                    float scale = 1 / totalBlend;
                    splatmaps[x, y, 0] *= scale;
                    splatmaps[x, y, 1] *= scale;
                    splatmaps[x, y, 2] *= scale;
                }
            }
        }

        // Apply the alphamap to the terrain
        terrainData.SetAlphamaps(0, 0, splatmaps);
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
        terrainData.SetHeights(0, 0, heights);
        Debug.Log("TerrainData updated");
    }
}
