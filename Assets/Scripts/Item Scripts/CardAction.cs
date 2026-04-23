using UnityEngine;

public abstract class CardAction : MonoBehaviour
{
    public abstract void ExecuteAction(Vector3Int cellPos);
    public abstract void PreviewAction();
}
