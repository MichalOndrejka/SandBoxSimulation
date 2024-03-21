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
    private int waterSpawnYOffset = 10;
    [SerializeField]
    private int waterSpawnOffset = 5;
    [SerializeField]
    private int numberToSpawnOnX = 3;
    [SerializeField]
    private int numberToSpawnOnZ = 3;

    void Update()
    {
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
}
