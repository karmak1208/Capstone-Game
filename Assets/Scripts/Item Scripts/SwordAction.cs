using UnityEngine;

public class SwordAction : CardAction
{
    public override void ExecuteAction(Vector3Int cellPos)
    {
        Debug.Log($"[SWORD] Executing sword action at cell {cellPos}");
        transform.parent.GetComponent<CardHandler>()?.DestroyCard();
    }

    public override void PreviewAction()
    {
    }
}
