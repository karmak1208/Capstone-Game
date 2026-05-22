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

    /// <summary>
    /// Sets the isPressed flag to true and records the mouse position at the start of the click. 
    /// This is used to determine if the player is dragging after a certain distance threshold is exceeded in the Update method.
    /// </summary>
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

    /// <summary>
    /// Handles the release of the click. If the click was not a drag, calls DetermineAction.
    /// </summary>
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
        active.Movement.SetActive(!active.Movement.inputEnabled);
    }

    /// <summary>
    /// Determines what action to take based on what the player clicked on. 
    /// If they clicked on a character, it switches to that character. 
    /// Else handles move input to move the active character to the clicked location.
    /// </summary>
    void DetermineAction()
    {
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null)
        {
            if (hit.CompareTag("Player"))
            {
                SwitchCharacter(hit.GetComponent<CharacterRoot>());
            }
            else if (hit.CompareTag("Card"))
            {
                Debug.Log($"[INPUT] Card Clicked");
                active?.Inventory.CardClicked(hit);
            }
            else if (hit.GetComponent<IInteractable>() != null)
            {
                hit.GetComponent<IInteractable>().Interact();
            }

        }
        else
        {
            HandleMoveInput();
        }
    }

    /// <summary>
    /// Determines if the player is clicking on a card in their inventory to hold and drag. If so, it caches the held card and notifies the InventoryManager.
    /// </summary>
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

    /// <summary>
    /// Determines if the player is releasing a held card over a valid cell to drop it or over the hotbar/invalid cell to return it to the inventory. It then notifies the InventoryManager of the release and clears the held card cache.
    /// </summary>
    void ReleaseHold()
    {
        if (heldCard != null)
        {
            Debug.Log("[INPUT] Card released - dropping");
            Vector3Int releaseCellPos = floormap.WorldToCell(mousePos);
            Vector2 hotbarViewportStart = new Vector2(0.1f, 0f);
            Vector2 hotbarViewportEnd = new Vector2(0.9f, 0.3f);
            Vector2 viewportPos = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue());
            if ((viewportPos.x >= hotbarViewportStart.x && viewportPos.x <= hotbarViewportEnd.x &&
                viewportPos.y >= hotbarViewportStart.y && viewportPos.y <= hotbarViewportEnd.y) || releaseCellPos == null)
            {
                Debug.Log("[INPUT] Card released over hotbar or invalid cell, returning to inventory");
                active?.Inventory.CardReleased(heldCard);
            }
            else if (releaseCellPos != null)
            {
                active?.Inventory.CardReleasedAtPosition(heldCard, releaseCellPos);
            }

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
