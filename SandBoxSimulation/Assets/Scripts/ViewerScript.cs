using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewerScript : MonoBehaviour
{
    [SerializeField]
    private MeasureDepth measureDepth;
    [SerializeField]
    private MultiSourceManager multiSourceManager;

    [SerializeField]
    private RawImage terrainImage;
    [SerializeField]
    private RawImage waterImage;

    void Update()
    {
        terrainImage.texture = measureDepth.terrainTexture;

        waterImage.texture = measureDepth.waterTexture;
    }
}
