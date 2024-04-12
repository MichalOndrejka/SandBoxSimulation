using UnityEngine;
using System.Linq;

public class AssignSplatMap : MonoBehaviour
{
    private Terrain _terrain;
    private TerrainData _terrainData;
    [SerializeField]
    private WaterSpawnController waterSpawnController;

    private float[] _splatWeights;

    [SerializeField]
    private int  _balanceHeightFactor;
    enum Texture
    {
        Sand,
        Grass,
        Rock,
        Snow,
        DarkBlue,
        LightBlue,
        Green,
        Orange,
        Red,
        Black,
        Jungle,
        Ocean,
        LichenRock,
        MildVulcano,
        Vulcano,
        BlackSand,
    }

    public enum TextureMode
    {
        Real,
        Colorful,
        ColorfulContour,
        Vulcanic,
        Exotic
    }


    public int currentTextureMode;

    void Start()
    {
        // Get the attached terrain component
        _terrain = GetComponent<Terrain>();

        // Get a reference to the terrain data
        _terrainData = _terrain.terrainData;

        ApplyTexture();
    }

    private void Update()
    {
        // Check for space key press to switch to the next texture mode
        if (Input.GetKeyDown(KeyCode.Space))
        {
            waterSpawnController.RemoveAllParticles();
            // Get the total number of texture modes
            int numModes = System.Enum.GetValues(typeof(TextureMode)).Length;

            // Calculate the next texture mode index
            int nextModeIndex = ((int)currentTextureMode + 1) % numModes;

            // Update the current texture mode to the next mode
            currentTextureMode = nextModeIndex;

            if (currentTextureMode == (int)TextureMode.Vulcanic) {
                waterSpawnController.spawnWater = false;
                Time.timeScale = waterSpawnController.lavaTimeScale;
            } else { 
                waterSpawnController.spawnWater = true;
                Time.timeScale = waterSpawnController.waterTimeScale;
            }

            // Reapply the texture based on the new mode
            ApplyTexture();


        }
    }

    public void ApplyTexture()
    {
        float[,,] splatmapData = new float[_terrainData.alphamapWidth, _terrainData.alphamapHeight, _terrainData.alphamapLayers];

        for (int y = 0; y < _terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < _terrainData.alphamapWidth; x++)
            {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / (float)_terrainData.alphamapHeight;
                float x_01 = (float)x / (float)_terrainData.alphamapWidth;

                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                float height = _terrainData.GetHeight(Mathf.RoundToInt(y_01 * _terrainData.heightmapResolution), Mathf.RoundToInt(x_01 * _terrainData.heightmapResolution));

                // Normalize height
                height = height / _terrainData.size.y;
                //Debug.Log(height);

                // Setup an array to record the mix of texture weights at this point
                _splatWeights = new float[_terrainData.alphamapLayers];

                SetAlpha(height);

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = _splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < _terrainData.alphamapLayers; i++)
                {

                    // Normalize so that sum of all texture weights = 1
                    _splatWeights[i] /= z;

                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = _splatWeights[i];
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        _terrainData.SetAlphamaps(0, 0, splatmapData);

        Debug.Log("Texture applied");
    }

    private void SetAlpha(float height)
    {
        if (currentTextureMode == (int)TextureMode.Real) ApplyRealTexture(height);
        else if (currentTextureMode == (int)TextureMode.ColorfulContour) ApplyColorfulContourTexture(height);
        else if (currentTextureMode == (int)TextureMode.Vulcanic) ApplyVulcanicTexture(height);
        else if (currentTextureMode == (int)TextureMode.Exotic) ApplyExoticTexture(height);
        else ApplyColorfulTexture(height);
    }


    private void ApplyRealTexture(float height)
    {
        if (height < 0.1f) _splatWeights[(int)Texture.Grass] = 1f;
        else if (height < 0.19f) _splatWeights[(int)Texture.Rock] = 1f;
        else _splatWeights[(int)Texture.Snow] = 1f;
    }

    private void ApplyColorfulTexture(float height)
    {
        if (height < 0.07f) _splatWeights[(int)Texture.DarkBlue] = 1f;
        else if (height < 0.1f) _splatWeights[(int)Texture.LightBlue] = 1f;
        else if (height < 0.13f) _splatWeights[(int)Texture.Green] = 1f;
        else if (height < 0.17f) _splatWeights[(int)Texture.Orange] = 1f;
        else _splatWeights[(int)Texture.Red] = 1f;
    }

    private void ApplyColorfulContourTexture(float height)
    {
        if ((int)(height * 1000) % 10 == 0) _splatWeights[(int)Texture.Black] = 1f;
        else if (height < 0.07f) _splatWeights[(int)Texture.DarkBlue] = 1f;
        else if (height < 0.1f) _splatWeights[(int)Texture.LightBlue] = 1f;
        else if (height < 0.13f) _splatWeights[(int)Texture.Green] = 1f;
        else if (height < 0.17f) _splatWeights[(int)Texture.Orange] = 1f;
        else _splatWeights[(int)Texture.Red] = 1f;
    }

    private void ApplyVulcanicTexture(float height)
    {
        if (height < 0.07f) _splatWeights[(int)Texture.LichenRock] = 1f;
        else if (height < 0.12f) _splatWeights[(int)Texture.BlackSand] = 1f;
        else if (height < 0.18f) _splatWeights[(int)Texture.MildVulcano] = 1f;
        else _splatWeights[(int)Texture.Vulcano] = 1f;
    }

    private void ApplyExoticTexture(float height)
    {
        if (height < 0.12f) _splatWeights[(int)Texture.Ocean] = 1f;
        else if (height < 0.13f) _splatWeights[(int)Texture.Sand] = 1f;
        else _splatWeights[(int)Texture.Jungle] = 1f;
    }
}