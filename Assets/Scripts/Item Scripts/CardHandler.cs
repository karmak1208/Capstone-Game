using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardHandler : MonoBehaviour
{
    private TextMeshProUGUI itemAmountText;
    public int itemAmount = 1;
    private int lastItemAmount;

    private bool isFollowingCursor = false;
    public bool resizeForCam;
    [SerializeField] float scaleFactor;
    private Vector3 originalScale;
    public Vector2 cardSize => GetComponent<SpriteRenderer>().bounds.size;
    Vector2 offsetFromCursor => new Vector2(cardSize.x / 2, cardSize.y / 2);
    public GameObject CardItem { get; private set; }
    public string ItemName { get; private set; }
    public string ItemType { get; private set; }

    /// <summary>
    /// Initializes the card by loading the corresponding CardData and prefab based on the provided itemName.
    /// </summary>
    /// <param name="itemName">The name of the item to initialize the card with.</param>
    public async void Initialize(string itemName, bool resize = true)
    {
        lastItemAmount = itemAmount;
        resizeForCam = resize;
        originalScale = new Vector3(scaleFactor, scaleFactor, 1);
        transform.localScale = originalScale;

        CardData cardSO = await CardLoader.Instance?.LoadObject<CardData>(itemName);
        if (cardSO == null) Debug.LogError($"[CardHandler] Failed to load CardData for item: {itemName}");

        GameObject cardPrefab = await CardLoader.Instance?.LoadObject<GameObject>(cardSO.itemName + "Prefab");
        if (cardPrefab == null) Debug.LogError($"[CardHandler] Failed to load card prefab for item: {cardSO.itemName}");

        CardItem = Instantiate(cardPrefab, transform);
        CardItem.GetComponent<CardAction>()?.Initialize(cardSO, gameObject);
        ItemName = cardSO.itemName;
        ItemType = cardSO.itemType;
        CardItem.transform.position = transform.position;

        gameObject.name = $"{cardSO.itemName} Card";

        itemAmountText = GetComponentInChildren<TextMeshProUGUI>();
        if (itemAmountText == null) Debug.LogError($"[CardHandler] Failed to find TextMeshProUGUI component in children of {gameObject.name}");
        itemAmountText.text = itemAmount.ToString();
    }

    public void IncreaseItemAmountBy(int amount)
    {
        itemAmount += amount;
    }
    public void DecreaseItemAmountBy(int amount)
    {
        itemAmount = Mathf.Max(0, itemAmount - amount);
    }

    public void FollowCursor(bool active)
    {
        isFollowingCursor = active;
    }

    private void Update()
    {
        if (isFollowingCursor)
        {
           GoToCursor();
        }
        if (resizeForCam)
            ScaleToCamSize();

        if (itemAmount != lastItemAmount && itemAmountText != null)
        {
            itemAmountText.text = itemAmount.ToString();
            lastItemAmount = itemAmount;
        }
    }

    void GoToCursor()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        transform.position = worldPos + new Vector3(offsetFromCursor.x, -offsetFromCursor.y, 0);
    }

    void ScaleToCamSize()
    {
        float camSize = Camera.main.orthographicSize;
        float scaleFactor = 2 * (camSize / 5f);
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    }

    /// <summary>
    /// Calls the ExecuteAction method on the CardAction component of the card's item, passing in the given cell position. 
    /// </summary>
    /// <param name="cellPos">The cell position where the card's effect should be applied.</param>
    /// <param name="removeCard">Whether to remove the card from the inventory after executing the action.</param>
    public void UseCard(Vector3Int cellPos, bool removeCard = false)
    {
        Debug.Log($"[CARD] Using card: {CardItem.name} at position {cellPos}");
        CardItem.GetComponent<CardAction>()?.ExecuteAction(cellPos, removeCard);
    }
} 
