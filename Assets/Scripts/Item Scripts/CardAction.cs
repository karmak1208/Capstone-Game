using UnityEngine;
using System.Collections;
using System.Linq;

public abstract class CardAction : MonoBehaviour
{
    public abstract CardData Data { get; set; }
    public GameObject creator = null;
    public Vector3 parentPos => transform.parent.position;

    /// <summary>
    /// Starts the process of executing the card's action by calling the ExecuteActionRoutine. 
    /// If destroyParent is true, it will also call DestroyCard on the parent CardHandler before executing the action. 
    /// </summary>
    /// <param name="cellPos">The cell position where the card's effect should be applied.</param>
    /// <param name="removeCard">Whether to destroy the parent CardHandler after executing the action.</param>
    public void ExecuteAction(Vector3Int cellPos, bool removeCard = false)
    {
        StartCoroutine(ExecuteActionRoutine(cellPos, removeCard));
    }

    /// <summary>
    /// Executes the card's action routine. This coroutine handles the execution of the card's action and optionally resets the card's position and rotation if the parent is not destroyed.
    /// </summary>
    /// <param name="cellPos">The cell position where the card's effect should be applied.</param>
    /// <param name="removeCard">Whether to destroy the parent CardHandler after executing the action.</param>
    private IEnumerator ExecuteActionRoutine(Vector3Int cellPos, bool removeCard)
    {
        transform.SetParent(transform.parent.parent); // reparent to character root for action execution
        transform.localScale = Vector3.one; // reset scale to prevent distortion during action execution
        yield return OnExecuteAction(cellPos); // subclass returns an IEnumerator
        if (creator != null) 
        {
            transform.SetParent(creator.transform); // reparent to creator
            transform.localPosition = Vector3.zero; transform.rotation = transform.parent.rotation; // reset position
            transform.localScale = Vector3.one; // reset scale
        }
        else
        {
            Debug.LogWarning($"[CARD ACTION] No creator assigned for {gameObject.name}, cannot reset.");
            gameObject.SetActive(false); // hide card if no creator to reset to
        }

        var inv = transform.parent.parent.GetComponent<InventoryManager>();
        if (removeCard)
        {
            if (inv != null)
            {
                inv.RemoveCardFromInventory(Data.itemName);
                Debug.Log($"[CARD ACTION] RemoveCard is true, removing {gameObject.name} from inventory.");
            }
            else Debug.LogWarning($"[CARD ACTION] RemoveCard is true but no InventoryManager found on parent of {gameObject.name}");
        }

    }


    public void EnemyExecuteAction(Vector3Int cellPos)
    {
        StartCoroutine(EnemyExecuteActionRoutine(cellPos));
    }

    private IEnumerator EnemyExecuteActionRoutine(Vector3Int cellPos)
    {
        yield return OnExecuteAction(cellPos);
        gameObject.SetActive(false);
    }

    public virtual void Initialize(CardData data, GameObject _creator)
    {
        Data = data;
        creator = _creator;
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
