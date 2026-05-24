using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;


public class InventoryManager : MonoBehaviour, IResettable, IActivatable
{
    private CharacterRoot Root;

    public GameObject cardPrefab;
    private CharacterData characterData;
    public List<GameObject> ActiveCards = new List<GameObject>();
    private Dictionary<string, GameObject> _cardLookup = new();
    private GameObject heldCard;

    private Vector2 hotbarOrigin = new Vector2(0.5f, 0.2f);
    private Vector2 hotbarStartPos;
    [SerializeField] private float cardSpacing;

    public int TimesAttackedThisTurn = 0;
    public int MaxAttacksPerTurn = 1;

    void Start()
    {
        hotbarStartPos = hotbarOrigin;
        Root = GetComponent<CharacterRoot>();
        Root.OnFinishedLoading.AddListener(Initialize);
    }

    void Initialize()
    {
        foreach (var item in Root.Data.inventory)
            for (int i = 0; i < item.Value; i++)
            {
                GameObject card = CreateCard(item.Key);
                card.SetActive(false);
            }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < ActiveCards.Count; i++)
        {
            if (ActiveCards[i] == heldCard) continue; // Skip updating position for the card currently being held
            ActiveCards[i].transform.position = GetCardHotbarHome(i);
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
            for (int i = 0; i < ActiveCards.Count; i++)
            {
                ActiveCards[i].SetActive(true);
                CardHandler handler = ActiveCards[i].GetComponent<CardHandler>();
                handler.transform.position = GetCardHotbarHome(i);
            }
        }
        else
        {
            foreach (var card in ActiveCards)
            {
                card.SetActive(false);
            }
            _cardLookup.Clear();
        }
    }

    /// <summary>
    /// Gets the world position for a card in the hotbar based on its index, centering the hotbar on the screen and spacing cards evenly. The spacing scales with camera zoom to maintain visual consistency.
    /// </summary>
    /// <param name="index">The index of the card in the hotbar.</param>
    /// <returns>The world position for the card.</returns>
    Vector3 GetCardHotbarHome(int index)
    {
        CardHandler handler = ActiveCards[index].GetComponent<CardHandler>();
        float cardWidth = handler.cardSize.x;
        float spacing = cardWidth + cardSpacing * (Camera.main.orthographicSize / 5f); // scale gap with zoom too
        float totalWidth = spacing * ActiveCards.Count - spacing + cardWidth; // span of all cards

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
    public GameObject CreateCard(string itemName)
    {
        Debug.Log("[INVENTORYMANAGER] Creating card for item: " + itemName);
        if (_cardLookup.TryGetValue(itemName, out GameObject existing))
        {
            existing.GetComponent<CardHandler>().IncreaseItemAmountBy(1);
            return existing;
        }

        var card = Instantiate(cardPrefab, transform);
        _cardLookup[itemName] = card;
        ActiveCards.Add(card);
        card.GetComponent<CardHandler>().Initialize(itemName);
        return card;
    }

    /// <summary>
    /// Lowers card count, destroys the card GameObject if count reaches 0, and removes it from the inventory. If the card is not in the active cards list, it does nothing.
    /// </summary>
    /// <param name="card">The card GameObject to remove from the inventory.</param>
    public void RemoveCardFromInventory(string itemName)
    {
        var card = ActiveCards.FirstOrDefault(c => c.GetComponent<CardHandler>().ItemName == itemName);
        if (card != null)
        {
            CardHandler handler = card.GetComponent<CardHandler>();
            handler.DecreaseItemAmountBy(1);
            if (handler.itemAmount <= 0)
            {
                ActiveCards.Remove(card);
                characterData.inventory.Remove(itemName);
                Destroy(card);
            }
        }
        else { Debug.LogWarning($"[INV] Attempted to remove card not in inventory: {card.name}"); }
    }

    public void CardClicked(Collider2D clicked)
    {
        if (!ActiveCards.Contains(clicked.gameObject))
        {
            Debug.Log($"[INV] Adding clicked card to inventory {clicked.gameObject.name}");
            clicked.transform.SetParent(transform);
            ActiveCards.Add(clicked.gameObject);
            var handler = clicked.GetComponent<CardHandler>();
            handler.resizeForCam = true;
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