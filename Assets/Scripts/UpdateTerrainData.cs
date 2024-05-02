using UnityEngine;

public class UpdateTerrainData : MonoBehaviour
{
    private Terrain _terrain;

    [SerializeField]
    private MeasureDepth measureDepth;
    [SerializeField]
    private UpdateTerrainTexture updateTerrainTexture;
    [SerializeField]
    private bool updateTerrain;

    void Start()
    {
        _terrain = GetComponent<Terrain>();
        updateTerrainTexture = GetComponent<UpdateTerrainTexture>();
    }

    // Update the terrain data with heights calculated from depth data form MeasureDepth script
    public void UpdateTerrain()
    {
        // Check if terrain update is enabled (for debug purposes)
        if (!updateTerrain) return;

        TerrainData terrainData = _terrain.terrainData;

        if (terrainData == null) return;

        // Set necessary values
        terrainData.heightmapResolution = measureDepth.depthResolution.x;
        terrainData.size = new Vector3(750, terrainData.size.y, 1100);

        // Create heights
        float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        // Find the minimum and maximum depth values
        ushort minDepth = (ushort)(measureDepth.minDepth + 100);
        ushort maxDepth = measureDepth.maxDepth;        

        // Loop throught depths and normalize them
        float depthRange = maxDepth - minDepth;
        for (int x = 0; x < measureDepth.depthResolution.x; x++)
        {
            for (int y = 0; y < measureDepth.depthResolution.y; y++)
            {
                int index = measureDepth.depthResolution.x * y + x;
                ushort depth = measureDepth.depthData[index];

                // If depth is zero due to Kinect imperfection replace it with value from the left
                if (depth == 0 && index != 0) depth = measureDepth.depthData[index - 1];

                // Terrain data takes values from 0 to 1, so normalization is requiered
                float normalizedDepth = 1 - ((depth - minDepth) / depthRange);
                normalizedDepth = normalizedDepth / 6;
                heights[x, measureDepth.depthResolution.y - 1 - y] = normalizedDepth;

            }

        }

        // Set heights
        terrainData.SetHeights(0, 0, heights);

        // Update terrain texture
        updateTerrainTexture.ApplyTexture();
    }
}
