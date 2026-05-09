using UnityEngine;

public class CharacterRoot : MonoBehaviour
{
    public MovementController Movement { get; private set; }
    public TileHighlighter Highlighter { get; private set; }
    public InventoryManager Inventory { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<MovementController>();
        Highlighter = GetComponentInChildren<TileHighlighter>();
        Inventory = GetComponent<InventoryManager>();
        if (Movement == null)
        {
            Debug.LogError("CharacterRoot requires a MovementController component.");
        }
        if (Highlighter == null)
        {
            Debug.LogError("CharacterRoot requires a TileHighlighter component in its children.");
        }
        if (Inventory == null)
        {
            Debug.LogError("CharacterRoot requires an InventoryManager component.");
        }


        TurnManager.Instance.OnTurnStart.AddListener(OnTurnStarted);
        TurnManager.Instance.OnTurnEnd.AddListener(OnTurnEnded);
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnStart.RemoveListener(OnTurnStarted);
            TurnManager.Instance.OnTurnEnd.RemoveListener(OnTurnEnded);
        }
    }

    public Vector3 Position => transform.position;
    public string CharacterName = "Unnamed";
    public Sprite CharacterSprite;

    public void SetActive(bool isActive)
    {
        foreach (var component in GetComponents<IActivatable>())
        {
            component.SetActive(isActive);
        }
    }

    void OnTurnStarted(int turn)
    {
        Movement.StartTurnReset();
    }

    void OnTurnEnded()
    {
        SetActive(false);
        foreach(var component in GetComponents<IResettable>())
        {
            component.StartTurnReset();
        }
        
    }
}

