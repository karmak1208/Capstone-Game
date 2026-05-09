using UnityEngine;
using System.Collections;

public abstract class CardAction : MonoBehaviour
{
    public abstract CardData Data { get; set; }

    /// <summary>
    /// Starts the process of executing the card's action by calling the ExecuteActionRoutine. 
    /// If destroyParent is true, it will also call DestroyCard on the parent CardHandler before executing the action. 
    /// </summary>
    /// <param name="cellPos">The cell position where the card's effect should be applied.</param>
    /// <param name="destroyParent">Whether to destroy the parent CardHandler after executing the action.</param>
    public void ExecuteAction(Vector3Int cellPos, bool destroyParent = false)
    {
        if (destroyParent) transform.parent.GetComponent<CardHandler>()?.DestroyCard();
        StartCoroutine(ExecuteActionRoutine(cellPos, destroyParent));
    }

    /// <summary>
    /// Executes the card's action routine. This coroutine handles the execution of the card's action and optionally resets the card's position and rotation if the parent is not destroyed.
    /// </summary>
    /// <param name="cellPos">The cell position where the card's effect should be applied.</param>
    /// <param name="destroyParent">Whether to destroy the parent CardHandler after executing the action.</param>
    private IEnumerator ExecuteActionRoutine(Vector3Int cellPos, bool destroyParent)
    {
        yield return OnExecuteAction(cellPos); // subclass returns an IEnumerator
        if (!destroyParent) { transform.localPosition = Vector3.zero; transform.rotation = transform.parent.rotation; } // reset position after action completes
    }

    public virtual void Initialize(CardData data)
    {
        Data = data;
    }

    /// <summary>
    /// The routine that subclasses must implement to define the specific behavior of the card's action. 
    /// This method will be called by ExecuteActionRoutine and should contain the logic for applying the card's effect at the given cell position.
    /// The card is only reset after this coroutine completes.
    /// </summary>
    /// <param name="cellPos"></param>
    protected abstract IEnumerator OnExecuteAction(Vector3Int cellPos);

    public abstract void PreviewAction();
}
