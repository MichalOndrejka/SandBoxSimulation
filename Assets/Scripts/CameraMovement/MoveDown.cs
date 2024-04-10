using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _isPressed = false;
    [SerializeField]
    private GameObject mainCamera;

    void Update()
    {
        if (_isPressed)
        {
            mainCamera.transform.Translate(0, -mainCamera.GetComponent<CameraController>().moveSpeed * Time.deltaTime, 0);
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
