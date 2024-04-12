using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaiseUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _isPressed = false;
    [SerializeField]
    private MeasureDepth measureDepth;
    private int speed = 50;

    void Update()
    {
        if (_isPressed)
        {
            if (measureDepth.yHeightAdjustment < 300)
            {
                measureDepth.yHeightAdjustment -= speed * Time.deltaTime;
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
