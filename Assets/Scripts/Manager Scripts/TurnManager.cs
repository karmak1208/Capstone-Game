using System.Collections;
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
    public UnityEvent AfterTurnEnd;
    public int CurrentTurn { get; private set; } = 1;

    int _pending = 0;

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

    public void EndTurn() => StartCoroutine(EndTurnRoutine());
    
    private IEnumerator EndTurnRoutine()
    {
        Debug.Log($"[TurnManager] Ending Turn {CurrentTurn}");
        _pending = 0;
        OnTurnEnd?.Invoke();
        yield return new WaitUntil(() => _pending <= 0);
        AfterTurnEnd.Invoke();
        CurrentTurn++;
        StartTurn();
    }
    public void RegisterEndTurnTask() => _pending++;
    public void CompleteEndTurnTask() => _pending--;
}