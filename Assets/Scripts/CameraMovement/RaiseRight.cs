using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaiseRight : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _isPressed = false;
    [SerializeField]
    private MeasureDepth measureDepth;
    private int speed = 50;

    void Update()
    {
        if (_isPressed)
        {
            if (-400 < measureDepth.xHeightAdjustment)
            {
                measureDepth.xHeightAdjustment -= speed * Time.deltaTime;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
    }
}
