using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
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

    public TextMeshProUGUI turnDisplay;

    public UnityEvent<int> OnTurnStart;
    public UnityEvent OnTurnEnd;
    public int CurrentTurn { get; private set; } = 1;

    void Start()
    {
        StartTurn();
    }

    public void StartTurn()
    {
        if (turnDisplay != null)
        {
            turnDisplay.text = $"Current Turn: {CurrentTurn}";
        }

        OnTurnStart?.Invoke(CurrentTurn);
    }

    public void EndTurn()
    {
        Debug.Log($"[TurnManager] Ending Turn {CurrentTurn}");
        OnTurnEnd?.Invoke();
        CurrentTurn++;
        StartTurn();
    }
}