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
    public InputAction toggleMoveAction;

    private CharacterRoot active => CharacterManager.Instance?.ActiveCharacter;

    void Start()
    {
        clickAction = InputSystem.actions["Click"];
        clickAction.performed += ctx => HandleMoveInput();

        toggleMoveAction = InputSystem.actions["ToggleMove"];
        toggleMoveAction.performed += OnToggleMove;
        toggleMoveAction.Enable();
    }

    void OnDisable()
    {
        toggleMoveAction.performed -= OnToggleMove;
    }

    private void OnToggleMove(InputAction.CallbackContext ctx)
    {
        Debug.Log("Toggle Move Input action performed.");
        active.Movement.SetInputEnabled(!active.Movement.inputEnabled);
    }


    void HandleMoveInput()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector3.zero);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Debug.Log("Player character clicked. Attempting to switch active character.");
            CharacterRoot clickedCharacter = hit.collider.GetComponent<CharacterRoot>();
            CharacterManager.Instance.SwitchTo(clickedCharacter);
            return; // Exit early to after switching
        }
        Debug.Log("Click detected, attempting to move player.");

        // Find active character to pathfind from
        if (active == null) return;
        else Debug.Log($"Active character: {active.CharacterName}");

        Vector3Int targetCellPos = floormap.WorldToCell(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        Vector3Int characterCellPos = floormap.WorldToCell(active.Position);

        active.Movement.MoveTo(characterCellPos, targetCellPos);
    }

    void SwitchCharacter()
    {
        
    }
}
