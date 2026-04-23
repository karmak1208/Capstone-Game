using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardHandler : MonoBehaviour
{
    bool isFollowingCursor = false;
    public Vector2 cardSize => GetComponent<SpriteRenderer>().bounds.size;
    Vector2 offsetFromCursor => new Vector2(cardSize.x / 2, cardSize.y / 2);
    GameObject CardItem;
    public string ItemName;
    GameObject creator;

    public async void Initialize(string itemName, GameObject _creator)
    {
        creator = _creator;
        CardData cardSO = await CardLoader.Instance?.LoadObject<CardData>(itemName);
        GameObject cardPrefab = await CardLoader.Instance?.LoadObject<GameObject>(cardSO.itemName + "Prefab");
        CardItem = Instantiate(cardPrefab, transform);
        ItemName = cardSO.itemName;
        CardItem.transform.position = transform.position;
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
        ScaleToCamSize();
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
        float scaleFactor = camSize / 5f;
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    }

    public void UseCard(Vector3Int cellPos)
    {
        Debug.Log($"[CARD] Using card: {CardItem.name} at position {cellPos}");
        CardItem.GetComponent<CardAction>()?.ExecuteAction(cellPos);
    }

    public void DestroyCard()
    {
        transform.DetachChildren();
        creator.GetComponent<InventoryManager>()?.RemoveCardFromInventory(gameObject);
        Destroy(gameObject);
    }
} 
