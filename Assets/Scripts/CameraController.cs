using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class CameraController : MonoBehaviour
{
    private Vector3 dragOrigin;
    private bool isDragging = false;
    private InputAction lookAction;
    private InputAction scrollAction;

    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;

    [SerializeField] private float maxDistance;

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

    bool IsOverDistanceLimit()
    {
        Vector2 dist = (Vector2)Camera.main.transform.position - Vector2.zero;
        return dist.sqrMagnitude > maxDistance * maxDistance;
    }

    void CameraDrag()
    {
        if (lookAction.WasPressedThisFrame())
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            isDragging = true;
        }

        if (lookAction.IsPressed() && isDragging)
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Camera.main.transform.position += difference;
        }

        if (lookAction.WasReleasedThisFrame())
        {
            isDragging = false;
        }

        if (IsOverDistanceLimit())
        {
            Vector3 pos = Camera.main.transform.position;
            Vector2 clamped = Vector2.ClampMagnitude(new Vector2(pos.x, pos.y), maxDistance);
            Camera.main.transform.position = new Vector3(clamped.x, clamped.y, pos.z);
        }
    }

    void CameraScroll()
    {
        float scroll = scrollAction.ReadValue<float>();
        if (scroll < 0)
        {
            Camera.main.orthographicSize += zoomSpeed;
        }
        else if (scroll > 0)
        {
            Camera.main.orthographicSize -= zoomSpeed;
        }
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }
}
