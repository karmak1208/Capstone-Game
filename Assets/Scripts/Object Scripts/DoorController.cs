using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoorController : MonoBehaviour, IInteractable
{
    public bool IsOpen = false;

    public Sprite openSprite;
    public Sprite closedSprite;

    private SpriteRenderer spriteRenderer;
    private ShadowCaster2D shadowCaster;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        shadowCaster = GetComponent<ShadowCaster2D>();
    }

    public void Interact()
    {
        foreach (var player in CharacterManager.Instance.PartyMembers)
        {
            if (Vector3.Distance((Vector3)player.Position, transform.position) <= Mathf.Sqrt(2))
            {
                Debug.Log($"[DOOR] Door {gameObject.name} interacted with.");
                IsOpen = !IsOpen;
                spriteRenderer.sprite = IsOpen ? openSprite : closedSprite;
                shadowCaster.enabled = !IsOpen;
            }
        }
        Debug.Log($"[DOOR] No Player Close Enough To Interact.");
        return;

    }
}
