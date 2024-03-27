using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpawnController : MonoBehaviour
{
    [SerializeField]
    private LayerMask terrainLayerMask;
    [SerializeField]
    private GameObject waterParticlePrefab;

    [SerializeField]
    private MeasureDepth measureDepth;

    [SerializeField]
    private int waterSpawnYOffset = 10;
    [SerializeField]
    private int waterSpawnOffset = 5;
    [SerializeField]
    private int numberToSpawnOnX = 3;
    [SerializeField]
    private int numberToSpawnOnZ = 3;

    private Vector3 _handPosition;

    void Update()
    {
        _handPosition = GetHandPosition();
        // Check for mouse click
        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
            Debug.Log("clicked");
            // Cast a ray from the camera to the terrain layer
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits any collider on the terrain layer
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask))
            {
                // Spawn water particle at the hit position
                SpawnWaterParticle(hit.point);
            }
        } else if (_handPosition != Vector3.zero)
        {
            SpawnWaterParticle(_handPosition);
        }
    }

    void SpawnWaterParticle(Vector3 position)
    {
        Vector3 tempPosition = position;
        for(int x = 0; x < numberToSpawnOnX; x++)
        {
            for (int z = 0; z < numberToSpawnOnZ; z++)
            {
                tempPosition.x = position.x + x * waterSpawnOffset;
                tempPosition.y = position.y + waterSpawnOffset;
                tempPosition.z = position.z + z * waterSpawnOffset;
                // Instantiate the water particle prefab at the given position
                Instantiate(waterParticlePrefab, tempPosition, Quaternion.identity);
            }
        }
    }

    private Vector3 GetHandPosition()
    {
        if (measureDepth.depthData.Length == 0)
        {
            return new Vector3(0, 0, 0);
        }

        Debug.Log("Getting Hand Position");

        int minX = 100;
        int minY = 70;
        int maxX = measureDepth.depthResolution.x - 50;
        int maxY = measureDepth.depthResolution.y - 100;
        ushort minDepth = 900;

        int sumX = 0;
        int sumY = 0;
        int count = 0;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                int index = y * measureDepth.depthResolution.x + x;
                ushort depth = measureDepth.depthData[index];
                if (depth < minDepth && depth != 0)
                {
                    sumX += x;
                    sumY += y;
                    count++;
                }
            }
        }

        if (count == 0) {
            return new Vector3(0, 0, 0);
        }

        int avgX = sumX / count;
        int avgZ = sumY / count;
        int height = measureDepth.depthData[avgZ * measureDepth.depthResolution.x + avgX];
        height -= measureDepth.maxDepth;

        return new Vector3(avgX, height, avgZ);
    }
}
