using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Get the x and y coordinates of the click
            float x = eventData.position.x;
            float y = eventData.position.y;

            Debug.Log("Left mouse button clicked at coordinates: (" + x + ", " + y + ")");
        }
    }
}
