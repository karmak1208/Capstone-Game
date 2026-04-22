using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Tilemaps;

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
        }
    }

    [SerializeField] Tilemap floormap;
    public InputAction clickAction;
    public InputAction toggleMoveAction;

    private CharacterRoot active => CharacterManager.Instance.ActiveCharacter;
    private Vector2 mousePos => Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    private Collider2D heldCard;
    private bool isPressed = false;
    private bool isDragging = false;
    private Vector2 pressStartPos;
    [SerializeField] private float dragDistanceThreshold;

    void Start()
    {
        clickAction = InputSystem.actions["Click"];
        clickAction.started += OnPressStarted;
        clickAction.canceled += OnPressReleased;

        toggleMoveAction = InputSystem.actions["ToggleMove"];
        toggleMoveAction.performed += OnToggleMove;
    }

    private void OnPressStarted(InputAction.CallbackContext ctx)
    {
        isPressed = true;
        isDragging = false;
        pressStartPos = mousePos;
    }
    void Update()
    {
        if (!isPressed || isDragging) return;

        Vector2 currentPos = mousePos;
        float distance = Vector2.Distance(currentPos, pressStartPos);

        if (distance > dragDistanceThreshold)
        {
            isDragging = true;
            DetermineHold();
        }
    }

    private void OnPressReleased(InputAction.CallbackContext ctx)
    {
        if (isPressed && !isDragging)
            DetermineAction();

        isPressed = false;
        isDragging = false;
        ReleaseHold();
    }

    void OnDisable()
    {
        toggleMoveAction.performed -= OnToggleMove;
    }

    private void OnToggleMove(InputAction.CallbackContext ctx)
    {
        if (active == null) return;
        Debug.Log("Toggle Move Input action performed.");
        active.Movement.SetInputEnabled(!active.Movement.inputEnabled);
    }

    void DetermineAction()
    {
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null && hit.CompareTag("Player"))
        {
            SwitchCharacter(hit.GetComponent<CharacterRoot>());
        }
        else if (hit != null && hit.CompareTag("Card"))
        {
            Debug.Log("[INPUT] Card clicked");
            return;
        }
        else
        {
            HandleMoveInput();
        }
    }

    void DetermineHold()
    {
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null && hit.CompareTag("Card"))
        {
            heldCard = hit;
            active?.Inventory.CardHeld(hit);
            Debug.Log("[INPUT] Card held - dragging");
        }
    }

    void ReleaseHold()
    {
        if (heldCard != null)
        {
            Debug.Log("[INPUT] Card released - dropping");
            active?.Inventory.CardReleased(heldCard);
            heldCard = null; // clear cache
        }
    }

    void SwitchCharacter(CharacterRoot clickedCharacter)
    {
        Debug.Log("Player character clicked. Attempting to switch active character.");
        CharacterManager.Instance.SwitchTo(clickedCharacter);
        return; // Exit early to after switching
    }

    void HandleMoveInput()
    {
        Debug.Log("Click detected, attempting to move player.");

        // Find active character to pathfind from
        if (active == null) return;
        else Debug.Log($"Active character: {active.CharacterName}");

        Vector3Int targetCellPos = floormap.WorldToCell(mousePos);
        Vector3Int characterCellPos = floormap.WorldToCell(active.Position);

        active.Movement.MoveTo(characterCellPos, targetCellPos);
    }
    
}
