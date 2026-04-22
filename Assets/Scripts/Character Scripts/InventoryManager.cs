using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class InventoryManager : MonoBehaviour
{
    public GameObject cardPrefab;
    private CharacterData characterData;
    private List<GameObject> activeCards = new List<GameObject>();
    private GameObject heldCard;

    private Vector2 hotbarStartPos = new Vector2(0.3f, 0.1f);

    async void Start()
    {
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
        Vector3 baseWorldPos = Camera.main.ViewportToWorldPoint(new Vector3(hotbarStartPos.x, hotbarStartPos.y, 10f));
        baseWorldPos.z = 0;
        float spacing = handler.cardSize.x;
        return baseWorldPos + new Vector3(spacing * index, 0, 0);
    }

    GameObject CreateCard(string itemName)
    {
        GameObject card = Instantiate(cardPrefab);
        card.GetComponent<CardHandler>().Initialize(itemName);
        activeCards.Add(card);
        return card;
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
}