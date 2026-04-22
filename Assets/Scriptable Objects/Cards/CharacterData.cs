using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    [System.Serializable]
    public struct inventoryItem
    {
        public string itemName;
        public int amount;
    }
    public List<inventoryItem> _inventoryList = new();
    public Dictionary<string, int> inventory = new();

    void OnEnable()
    {
        foreach (inventoryItem item in _inventoryList)
        {
            inventory.Add(item.itemName, item.amount);
        }
    }
}
