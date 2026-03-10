using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public LayerMask obstacleLayer;

    [Header("Character Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float health;

    private bool isMoving = false;
    [SerializeField] private int tileSize;
    private Vector2 targetPosition;

    private InputAction moveAction;

    void Start()
    {
        moveAction = InputSystem.actions["Move"];
    }
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        ///
        /// Temporary keyboard input handling. Replace with point and click
        ///

        Vector2 movement = moveAction.ReadValue<Vector2>();

        // Prevent diagonal movement
        if (movement.x != 0) movement.y = 0;
        if (movement.y != 0) movement.x = 0;

        if (movement != Vector2.zero)
        {
            Debug.Log($"Input: {movement}");
            // Calculate the target position and start movement
            Vector2 direction = movement.normalized;
            Vector2 desiredPosition = (Vector2)transform.position + direction * tileSize;
            if (!isBlocked(desiredPosition) && !isMoving)
            {
                targetPosition = desiredPosition;
                Debug.Log($"Target Position: {targetPosition}");
                StartCoroutine(MovePlayer());
            }
        }
    }
    bool isBlocked(Vector2 targetPosition) // Check if path to tagert position is blocked by an obstacle
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetPosition - (Vector2)transform.position, tileSize, obstacleLayer);
        Debug.Log($"Raycast hit: {hit.collider?.name ?? "None"}");
        return hit.collider != null;
    }

    IEnumerator MovePlayer()
    {
        Debug.Log($"Moving to: {targetPosition}");

        isMoving = true;
        while ((Vector2)transform.position != targetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        isMoving = false;

    }
}
