using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private Vector3 dragOrigin;
    private bool isDragging = false;
    private InputAction lookAction;
    private InputAction scrollAction;

    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;

    private void Start()
    {
        lookAction = InputSystem.actions["Look"];
        scrollAction = InputSystem.actions["Scroll"];
    }

    void Update()
    {
       CameraDrag();
       CameraScroll();
    }

    void CameraDrag()
    {
         // On mouse button down, record the world position
        if (lookAction.WasPressedThisFrame())
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            isDragging = true;
        }

        // While dragging, move the camera
        if (lookAction.IsPressed() && isDragging)
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Camera.main.transform.position += difference;
        }

        if (lookAction.WasReleasedThisFrame())
        {
            isDragging = false;
        }
    }

    void CameraScroll()
    {
        float scroll = scrollAction.ReadValue<float>();
        if (scroll < 0)
        {
            Camera.main.orthographicSize += zoomSpeed;
            Debug.Log($"[CAM] Zooming out. New orthographic size: {Camera.main.orthographicSize}");
        }
        else if (scroll > 0)
        {
            Camera.main.orthographicSize -= zoomSpeed;
            Debug.Log($"[CAM] Zooming in. New orthographic size: {Camera.main.orthographicSize}");
        }
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }
}
