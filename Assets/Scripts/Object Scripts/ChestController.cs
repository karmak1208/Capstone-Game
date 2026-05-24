using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour, IInteractable
{
    public bool IsOpen = false;
    [SerializeField] private float scale;
    [SerializeField] private float spacing;


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

    void Update()
    {
        if (IsOpen)
        {
            for (int i = 0; i < chestItems.Count; i++)
            {
                var handler = chestItems[i].GetComponent<CardHandler>();
                handler.transform.position = GetCardHomePostion(handler, i);
                handler.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
    }

    void CreateCards()
    {
        foreach (string item in chestInventory)
        {
            var card = Instantiate(cardPrefab, transform);
            var handler = card.GetComponent<CardHandler>();
            handler.Initialize(item, false);
            card.transform.localScale = new Vector3(scale, scale, 1);
            chestItems.Add(card);
        }
    }

    Vector3 GetCardHomePostion(CardHandler handler, int index)
    {
        float cardWidth = handler.cardSize.x;
        float _spacing = cardWidth + spacing;
        float totalWidth = _spacing * chestItems.Count - _spacing + cardWidth; // span of all cards

        float verticalOffset = 1;
        Vector3 originWorld = new Vector3(transform.position.x, transform.position.y + verticalOffset);
        originWorld.z = 0f;

        float startX = originWorld.x - totalWidth / 2f;
        float cardCenterOffsetX = cardWidth / 2f; // align by card center
        return new Vector3(startX + cardCenterOffsetX + _spacing * index, originWorld.y);
    }
}
