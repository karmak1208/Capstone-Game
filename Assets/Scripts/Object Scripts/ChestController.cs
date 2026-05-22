using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour, IInteractable
{
    public bool IsOpen = false;

    public Sprite openSprite;
    public Sprite closedSprite;

    private SpriteRenderer spriteRenderer;

    public GameObject cardPrefab;

    [SerializeField] private List<string> chestInventory = new();
    private List<GameObject> chestItems = new();


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Interact()
    {
        Debug.Log($"[CHEST] Chest {gameObject.name} interacted with");
        if (!IsOpen)
        {
            spriteRenderer.sprite = openSprite;
            CreateCards();
            for (int i = 0; i < chestItems.Count; i++)
            {
                var handler = chestItems[i].GetComponent<CardHandler>();
                handler.transform.position = GetCardHomePostion(handler, i);
            }
        }
        IsOpen = true;
    }

    void CreateCards()
    {
        foreach (string item in chestInventory)
        {
            var card = Instantiate(cardPrefab, transform);
            float scale = 0.3f;
            card.transform.localScale = new Vector3(scale, scale, 1);
            var handler = card.GetComponent<CardHandler>();
            handler.Initialize(item, false);
            chestItems.Add(card);
        }
    }

    Vector3 GetCardHomePostion(CardHandler handler, int index)
    {
        float cardWidth = handler.cardSize.x;
        float spacing = cardWidth + 0.1f;
        float totalWidth = spacing * chestItems.Count - spacing + cardWidth; // span of all cards

        float verticalOffset = 1;
        Vector3 originWorld = new Vector3(transform.position.x, transform.position.y + verticalOffset);
        originWorld.z = 0f;

        float startX = originWorld.x - totalWidth / 2f;
        float cardCenterOffsetX = cardWidth / 2f; // align by card center
        return new Vector3(startX + cardCenterOffsetX + spacing * index, originWorld.y);
    }
}
