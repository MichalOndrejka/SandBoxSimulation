using UnityEngine;
using System.Linq; // used for Sum of array

public class AssignSplatMap : MonoBehaviour
{
    private Terrain _terrain;
    private TerrainData _terrainData;

    private float[] _splatWeights;
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
    }

    public enum TextureMode
    {
        Real,
        Colorful,
    }

    void Start()
    {
        // Get the attached terrain component
        _terrain = GetComponent<Terrain>();

        // Get a reference to the terrain data
        _terrainData = _terrain.terrainData;

        ApplyTexture((int)TextureMode.Real);
    }

    public void ApplyTexture(int textureMode)
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

                // Setup an array to record the mix of texture weights at this point
                _splatWeights = new float[_terrainData.alphamapLayers];

                if (textureMode == (int)TextureMode.Real) ApplyRealTexture(height);
                else if (textureMode == (int)TextureMode.Colorful) ApplyColorfulTexture(height);

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
    }

    private void ApplyRealTexture(float height)
    {
        if (height < 100) _splatWeights[(int)Texture.Sand] = 1f;
        else if (height < 150) _splatWeights[(int)Texture.Grass] = 1f;
        else if (height < 200) _splatWeights[(int)Texture.Rock] = 1f;
        else _splatWeights[(int)Texture.Snow] = 1f;
    }

    private void ApplyColorfulTexture(float height)
    {
        if (height < 50) _splatWeights[(int)Texture.DarkBlue] = 1f;
        else if (height < 100) _splatWeights[(int)Texture.LightBlue] = 1f;
        else if (height < 200) _splatWeights[(int)Texture.Green] = 1f;
        else if (height < 250) _splatWeights[(int)Texture.Orange] = 1f;
        else _splatWeights[(int)Texture.Red] = 1f;
    }
}