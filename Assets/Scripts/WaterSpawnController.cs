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

    [SerializeField]
    private int maxParticleCount;

    private Vector3 _handPosition;

    [SerializeField]
    private float liquidSpawnCooldown;
    [SerializeField]
    private float liquidRemoveCooldown;
    float _liquidSpawnTime;
    float _liquidRemoveTime;

    public float waterTimeScale = 2f;
    public float lavaTimeScale = 0.4f;
    public bool spawnWater;

    public Color WaterColor;
    public Color LavaColor;

    [SerializeField]
    private Material textureWithShader;

    public TerrainData terrainData;

    private void Awake()
    {
        // Set the time scale based on current liquid ismulation mode
        if (spawnWater) Time.timeScale = waterTimeScale;
        else Time.timeScale = lavaTimeScale;
    }

    private void Start()
    {
        _liquidSpawnTime = 0f;
        _liquidRemoveTime = 0f;
    }

    void Update()
    {
        // If water simulation set the shader to show water
        if (spawnWater)
        { 
            textureWithShader.SetColor("_Color", WaterColor);
            // Set the stroke alpha (_Stroke) property
            textureWithShader.SetFloat("_Stroke", 0.1f);

            // Set the water transparency (_Cutoff) property
            textureWithShader.SetFloat("_Cutoff", 1.0f - 0.5f);

        // Else set the shader to show lava
        }
        else
        {
            textureWithShader.SetColor("_Color", LavaColor);
            textureWithShader.SetFloat("_Cutoff", 1.0f - 0.1f);
        }

        _liquidSpawnTime += Time.deltaTime;

        // Check for hand and spawn liquid if hand detected
        if (_liquidSpawnTime / Time.timeScale > liquidSpawnCooldown)
        {
            _handPosition = GetHandPosition();
            _liquidSpawnTime = 0f;
            if (_handPosition != Vector3.zero)
            {
                SpawnParticle(_handPosition);
            }
        }


        _liquidRemoveTime += Time.deltaTime;

        if (_liquidRemoveTime / Time.timeScale > liquidRemoveCooldown)
        {
            RemoveAllParticles();
            _liquidRemoveTime = 0f;
        }

        // Check for mouse click and spawn water on cursor if detected
        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
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

    // Function to spawn liquid particle on a specific position, spawn numberToSpawn particled with waterSpawnOffset offset
    void SpawnParticle(Vector3 position)
    {
        // Dont spawn particle if too many in scene
        if (transform.childCount >= maxParticleCount) return;

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

        // Reset remove time since water was just spawned
        _liquidRemoveTime = 0f;
    }

    // Check for hand in depth data
    private Vector3 GetHandPosition()
    {
        // Check if depth data are valid
        if (measureDepth.rawDepthData.Length == 0)
        {
            return new Vector3(0, 0, 0);
        }

        // Variables used to ignore edges od depth data
        int minX = 100;
        int minY = 100;
        int maxX = measureDepth.depthResolution.x - minX;
        int maxY = measureDepth.depthResolution.y - minY;
        ushort minDepth = 900;

        int sumX = 0;
        int sumY = 0;
        int count = 0;

        // Loop through dpeth data and save hand location
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

        // If no hand return vector with zeroes
        if (count == 0) {
            return new Vector3(0, 0, 0);
        }

        // Calucate hand coordinates
        int avgX = sumX / count;
        int avgZ = sumY / count;
        Vector3 handPos = terrain.transform.InverseTransformPoint(new Vector3(avgX, 0f, avgZ));
        float height = terrain.SampleHeight(handPos);
        if (height == 600)
        {
            return new Vector3(0, 0, 0);
        }
        float posX = (float)avgX / (float)measureDepth.depthResolution.x;
        float posZ = (float)avgZ / (float)measureDepth.depthResolution.y;
        posX *= terrainData.size.z;
        posZ *= terrainData.size.x;

        // Return the hand coordiantes in world space
        return new Vector3(800 - posZ, 150, posX);
    }

    // Remove all particles within a scene
    public void RemoveAllParticles()
    {
        // Get the transform of the current object
        Transform parentTransform = transform;

        // Loop through each child object of the parent
        foreach (Transform child in parentTransform)
        {
            // Destroy the child game object
            Destroy(child.gameObject);
        }
    }
}
