using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
using UnityEngine;


public class InventoryManager : MonoBehaviour, IResettable
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

    GameObject CreateCard(string itemName)
    {
        GameObject card = Instantiate(cardPrefab, transform);
        card.GetComponent<CardHandler>().Initialize(itemName);
        activeCards.Add(card);
        return card;
    }

    public void RemoveCardFromInventory(GameObject card)
    {
        if (activeCards.Contains(card))
        {
            activeCards.Remove(card);
            string itemName = card.GetComponent<CardHandler>().ItemName;
            int itemCount = characterData.inventory[itemName];
            itemCount--;
            if (itemCount <= 0)
            {
                characterData.inventory.Remove(itemName);
            }
        }
    }

    public void CardHeld(Collider2D held)
    {
        held.GetComponent<CardHandler>()?.FollowCursor(true);
        heldCard = held.gameObject;
    }

    public void CardReleased(Collider2D released)
    {
        released.GetComponent<CardHandler>()?.FollowCursor(false);
        heldCard = null;
    }

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