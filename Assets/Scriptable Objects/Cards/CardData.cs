using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public string itemName;
    public CardAction cardAction;

    public float damage;
    public int cooldown;
}
