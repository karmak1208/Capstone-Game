using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
using UnityEngine;


public class InventoryManager : MonoBehaviour, IResettable, IActivatable
{
    public GameObject cardPrefab;
    private CharacterData characterData;
    private List<GameObject> activeCards = new List<GameObject>();
    private GameObject heldCard;

    private Vector2 hotbarOrigin = new Vector2(0.5f, 0.1f);
    private Vector2 hotbarStartPos;

    public int TimesAttackedThisTurn { get; set; } = 0;
    public int MaxAttacksPerTurn { get; set; } = 1;

    async void Start()
    {
        hotbarStartPos = hotbarOrigin;
        characterData = await CardLoader.Instance?.LoadObject<CharacterData>((string)(GetComponent<CharacterRoot>().CharacterName + "Data"));
    }

    private void LateUpdate()
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            if (activeCards[i] == heldCard) continue; // Skip updating position for the card currently being held
            activeCards[i].transform.position = GetCardHotbarHome(i);
        }
    }
    public void StartTurnReset()
    {
        TimesAttackedThisTurn = 0;
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            if (characterData == null)
            {
                Debug.LogWarning("[INV] Character data not loaded. Cannot populate inventory.");
                return;
            }
            foreach (var item in characterData.inventory)
            {
                CreateCard(item.Key);
            }

            for (int i = 0; i < activeCards.Count; i++)
            {
                CardHandler handler = activeCards[i].GetComponent<CardHandler>();
                handler.transform.position = GetCardHotbarHome(i);

                Debug.Log($"[INV] Card {activeCards[i].name} positioned at {handler.transform.position}");
            }
        }
        else
        {
            foreach (var card in activeCards)
            {
                Destroy(card);
            }
            activeCards.Clear();
        }
    }

    /// <summary>
    /// Gets the world position for a card in the hotbar based on its index, centering the hotbar on the screen and spacing cards evenly. The spacing scales with camera zoom to maintain visual consistency.
    /// </summary>
    /// <param name="index">The index of the card in the hotbar.</param>
    /// <returns>The world position for the card.</returns>
    Vector3 GetCardHotbarHome(int index)
    {
        CardHandler handler = activeCards[index].GetComponent<CardHandler>();
        float cardWidth = handler.cardSize.x;
        float spacing = cardWidth + 0.1f * (Camera.main.orthographicSize / 5f); // scale gap with zoom too
        float totalWidth = spacing * activeCards.Count - spacing + cardWidth; // span of all cards

        // Convert viewport origin to world
        Vector3 originWorld = Camera.main.ViewportToWorldPoint(new Vector3(hotbarOrigin.x, hotbarOrigin.y, 10f));
        originWorld.z = 0f;

        float startX = originWorld.x - totalWidth / 2f;
        float cardCenterOffsetX = cardWidth / 2f; // align by card center
        return new Vector3(startX + cardCenterOffsetX + spacing * index, originWorld.y, 0f);
    }

    /// <summary>
    /// Instantiate a card prefab, initialize it with the given item name, add it to the active cards list, and return the created card GameObject.
    /// </summary>
    /// <param name="itemName">The name of the item to initialize the card with.</param>
    /// <returns>The created card GameObject.</returns>
    GameObject CreateCard(string itemName)
    {
        GameObject card = Instantiate(cardPrefab, transform);
        card.GetComponent<CardHandler>().Initialize(itemName);
        activeCards.Add(card);
        return card;
    }

    /// <summary>
    /// Lowers card count, destroys the card GameObject if count reaches 0, and removes it from the inventory. If the card is not in the active cards list, it does nothing.
    /// </summary>
    /// <param name="card">The card GameObject to remove from the inventory.</param>
    public void RemoveCardFromInventory(GameObject card)
    {
        if (activeCards.Contains(card))
        {
            string itemName = card.GetComponent<CardHandler>().ItemName;
            int itemCount = characterData.inventory[itemName];
            itemCount--;
            if (itemCount <= 0)
            {
                activeCards.Remove(card);
                characterData.inventory.Remove(itemName);
            }
        }
    }

    public void CardHeld(Collider2D held)
    {
        held.GetComponent<CardHandler>()?.FollowCursor(true);
        heldCard = held.gameObject;
    }

    /// <summary>
    /// Returns the card to its hotbar position if it is released without being used.
    /// </summary>
    /// <param name="released">The collider of the card released</param>
    public void CardReleased(Collider2D released)
    {
        released.GetComponent<CardHandler>()?.FollowCursor(false);
        heldCard = null;
    }

    /// <summary>
    /// Activates the card's effect at the given cell position if it is released over a valid target. 
    /// If the card is a weapon, it checks if the character can still attack this turn before using it. 
    /// After using the card, it stops following the cursor and clears the heldCard reference.
    /// </summary>
    /// <param name="released">The collider of the card released</param>
    /// <param name="cellPos">The cell position where the card is released</param>
    public void CardReleasedAtPosition(Collider2D released, Vector3Int cellPos)
    {
        string cardType = released.GetComponent<CardHandler>()?.ItemType;
        if (cardType == "Weapon")
        {
            if (TimesAttackedThisTurn < MaxAttacksPerTurn)
            {
                released.GetComponent<CardHandler>()?.UseCard(cellPos);
                TimesAttackedThisTurn++;
            }
            else { Debug.Log($"[INV] Cannot use {released.name} - max attacks per turn reached."); }
        }
        else { released.GetComponent<CardHandler>()?.UseCard(cellPos); }

        released.GetComponent<CardHandler>()?.FollowCursor(false);
        heldCard = null;
    }

}