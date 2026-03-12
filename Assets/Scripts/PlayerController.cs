using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public LayerMask obstacleLayer;

    [Header("Character Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private float movementRange;

    public bool isMoving = false;
    [SerializeField] private int tileSize;

    void Start()
    {
    }
    void Update()
    {
    }

    public IEnumerator MovePlayerAlongPath(List<Vector3Int> pathList)
    {
        isMoving = true;
        foreach (Vector3Int tile in pathList)
        {
            Debug.Log($"Moving towards tile: {tile}");

            // Keep moving each frame until we reach the tile
            while (transform.position != (Vector3)tile)
            {
                transform.position = Vector3.MoveTowards(transform.position, tile, moveSpeed * Time.deltaTime);
                yield return null; // Wait one frame, then continue the while loop
            }
        }
        isMoving = false;
    }

}
