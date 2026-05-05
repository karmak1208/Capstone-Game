using UnityEngine;
using System.Collections;

public abstract class CardAction : MonoBehaviour
{
    public abstract CardData Data { get; set; }

    public void ExecuteAction(Vector3Int cellPos, bool destroyParent = true)
    {
        if (destroyParent) transform.parent.GetComponent<CardHandler>()?.DestroyCard();
        StartCoroutine(ExecuteActionRoutine(cellPos, destroyParent));
    }

    private IEnumerator ExecuteActionRoutine(Vector3Int cellPos, bool destroyParent)
    {
        yield return OnExecuteAction(cellPos); // subclass returns an IEnumerator
        if (!destroyParent) { transform.localPosition = Vector3.zero; transform.rotation = transform.parent.rotation; } // reset position after action completes
    }

    public virtual void Initialize(CardData data)
    {
        Data = data;
    }

    protected abstract IEnumerator OnExecuteAction(Vector3Int cellPos);

    public abstract void PreviewAction();
}
