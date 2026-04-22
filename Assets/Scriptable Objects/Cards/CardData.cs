using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public CardAction cardAction;
}
