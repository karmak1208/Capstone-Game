using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [SerializeField] Tilemap floormap;
    public InputAction clickAction;

    void Start()
    {
        clickAction = InputSystem.actions["Click"];
        clickAction.performed += ctx => HandleMoveInput();
    }

    void HandleMoveInput()
    {
        Debug.Log("Click detected, attempting to move player.");

        // Find active character to pathfind from
        CharacterRoot active = CharacterManager.Instance?.ActiveCharacter;
        if (active == null) return;
        else Debug.Log($"Active character: {active.CharacterName}");

        Vector3Int targetCellPos = floormap.WorldToCell(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        Vector3Int characterCellPos = floormap.WorldToCell(active.Position);

        active.Movement.MoveTo(characterCellPos, targetCellPos);
    }
}
