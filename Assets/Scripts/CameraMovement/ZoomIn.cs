using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomIn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _isPressed = false;
    [SerializeField]
    private GameObject mainCamera;
    private float _moveSpeed;

    private void Start()
    {
        _moveSpeed = mainCamera.GetComponent<CameraController>().moveSpeed;
    }

    void Update()
    {
        if (_isPressed)
        {
            float newSize = Mathf.Clamp(Camera.main.orthographicSize - _moveSpeed * Time.deltaTime, 1f, Mathf.Infinity);
            mainCamera.GetComponent<Camera>().orthographicSize = newSize;
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
