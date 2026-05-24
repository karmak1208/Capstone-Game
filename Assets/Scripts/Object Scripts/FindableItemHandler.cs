using UnityEngine;

public class FindableItemHandler : MonoBehaviour, IInteractable
{
    public string itemName;

    public void Interact()
    {
        Debug.Log("[FINDABLEITEMHANDLER] Interacting with findable item: " + itemName);

        CharacterRoot active = CharacterManager.Instance.ActiveCharacter;
        active.Inventory.CreateCard(itemName);
    }
}
