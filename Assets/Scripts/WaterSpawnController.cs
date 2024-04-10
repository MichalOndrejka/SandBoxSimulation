using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpawnController : MonoBehaviour
{
    [SerializeField]
    private Terrain terrain;
    [SerializeField]
    private LayerMask terrainLayerMask;
    [SerializeField]
    private GameObject liquidParticlePrefab;

    [SerializeField]
    private MeasureDepth measureDepth;

    [SerializeField]
    private int waterSpawnYOffset;
    [SerializeField]
    private int waterSpawnOffset;
    [SerializeField]
    private int numberToSpawnOnX;
    [SerializeField]
    private int numberToSpawnOnY;
    [SerializeField]
    private int numberToSpawnOnZ;

    private Vector3 _handPosition;

    [SerializeField]
    private float waterSpawnCooldown;
    float _time;

    private float waterTimeScale = 2f;
    private float lavaTimeScale = 0.4f;
    public bool spawnWater;

    public Color WaterColor;
    public Color LavaColor;

    [SerializeField]
    private Material textureWithShader;

    private void Awake()
    {
        if (spawnWater) Time.timeScale = waterTimeScale;
        else Time.timeScale = lavaTimeScale;
    }

    private void Start()
    {
        _time = 0f;
    }

    void Update()
    {
        if (spawnWater)
        { 
            textureWithShader.SetColor("_Color", WaterColor);
            // Set the stroke alpha (_Stroke) property
            textureWithShader.SetFloat("_Stroke", 0.1f);

            // Set the water transparency (_Cutoff) property
            textureWithShader.SetFloat("_Cutoff", 1.0f - 0.5f);
        } else
        {
            textureWithShader.SetColor("_Color", LavaColor);
            textureWithShader.SetFloat("_Cutoff", 1.0f - 0.1f);
        }
        _time += Time.deltaTime;
        if (_time / Time.timeScale > waterSpawnCooldown)
        {
            _handPosition = GetHandPosition();
            _time -= waterSpawnCooldown * Time.timeScale;
            if (_handPosition != Vector3.zero)
            {
                SpawnParticle(_handPosition);
            }
        }
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
                SpawnParticle(hit.point);
            }
        }

        //SimulationStep();
    }

    void SpawnParticle(Vector3 position)
    {
        Vector3 tempPosition = position;
        for(int x = 0; x < numberToSpawnOnX; x++)
        {
            for (int z = 0; z < numberToSpawnOnZ; z++)
            {
                for (int y = 0; y < numberToSpawnOnY; y++)
                {
                    tempPosition.x = position.x + x * waterSpawnOffset;
                    tempPosition.y = position.y + y * waterSpawnOffset;
                    tempPosition.z = position.z + z * waterSpawnOffset;
                    // Instantiate the water particle prefab as a child of the current GameObject
                    GameObject particle = Instantiate(liquidParticlePrefab, tempPosition, Quaternion.identity);

                    // Set the instantiated particle as a child of the current GameObject
                    particle.transform.parent = this.transform;
                }
            }
        }
    }

    private Vector3 GetHandPosition()
    {
        if (measureDepth.rawDepthData.Length == 0)
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
                ushort depth = measureDepth.rawDepthData[index];
                if (depth < minDepth && depth != 0)
                {
                    sumX += x;
                    sumY += y;
                    count++;
                }
            }
        }

        if (count == 0) {
            Debug.Log("No hand detected");
            return new Vector3(0, 0, 0);
        }

        int avgX = sumX / count;
        int avgZ = sumY / count;
        Vector3 handPos = terrain.transform.InverseTransformPoint(new Vector3(avgX, 0f, avgZ));
        float height = terrain.SampleHeight(handPos);
        if (height == 600)
        {
            Debug.Log("Invalid height");
            return new Vector3(0, 0, 0);
        }
        float posX = (float)avgX / (float)measureDepth.depthResolution.x;
        float posZ = (float)avgZ / (float)measureDepth.depthResolution.y;
        posX *= 1000;
        posZ *= 800;

        Debug.Log("Spawning Water at X:" + posZ + ", Y:" + height + ", Z:" + posX);
        return new Vector3(800 - posZ, height, posX);

    }
}
