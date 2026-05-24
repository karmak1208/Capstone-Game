using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Events;

public class CharacterRoot : MonoBehaviour
{
    public MovementController Movement { get; private set; }
    public TileHighlighter Highlighter { get; private set; }
    public InventoryManager Inventory { get; private set; }
    public CharacterHealth Health { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<MovementController>();
        Highlighter = GetComponentInChildren<TileHighlighter>();
        Inventory = GetComponent<InventoryManager>();
        Health = GetComponent<CharacterHealth>();
        if (Movement == null)
        {
            Debug.LogError($"CharacterRoot requires a MovementController component. GameObject: {gameObject.name}");
        }
        if (Highlighter == null)
        {
            Debug.LogError($"CharacterRoot requires a TileHighlighter component in its children. GameObject: {gameObject.name}");
        }
        if (Inventory == null)
        {
            Debug.LogWarning($"CharacterRoot has no InventoryManager component. GameObject: {gameObject.name}");
        }
        if (Health == null)
        {
            Debug.LogError($"CharacterRoot requires an CharacterHealth component. GameObject: {gameObject.name}");
        }


        TurnManager.Instance.OnTurnStart.AddListener(OnTurnStarted);
        TurnManager.Instance.OnTurnEnd.AddListener(OnTurnEnded);
    }

    public UnityEvent OnFinishedLoading;

    async void Start()
    {
        Data = await CardLoader.Instance?.LoadObject<CharacterData>(CharacterName + "Data");
        CharacterName = Data.CharacterName;
        OnFinishedLoading.Invoke();
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
    public string CharacterName = "Familiar";
    public Sprite CharacterSprite;
    public CharacterData Data;

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

