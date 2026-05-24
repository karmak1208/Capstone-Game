using System.Collections.Generic;
using UnityEngine;

public class EnemyVisibility : MonoBehaviour
{
    private EnemyRoot Root;
    private SpriteRenderer spriteRenderer;
    private Vector3 lastPos;
    private bool isCharacterMoving = false;
    private bool hasBeenSeen = false;
    private bool wasVisible = true;
    private GameObject ghost;
    private Vector2 ghostPos;
    [SerializeField] private bool AlwaysVisible;

    public bool IsVisible => spriteRenderer.enabled;

    private void Awake()
    {
        Root = GetComponent<EnemyRoot>();
        if (Root == null)
        {
            Debug.LogError("EnemyVisibility requires an EnemyRoot component.");
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        ghost = transform.Find("GhostSprite")?.gameObject;
        if (ghost == null)
        {
            Debug.LogWarning("No GhostSprite child found for EnemyVisibility. Ghost feature will be disabled.");
        }
    }
    void Start()
    {
        CharacterManager.Instance.OnCharacterStartedMove.AddListener(CharacterStartedMoving);
        CharacterManager.Instance.OnCharacterEndedMove.AddListener(CharacterStoppedMoving);
        CharacterManager.Instance.OnPlayerDied.AddListener(VisibilityUpdate);

    }

    bool IsVisibleToPlayer(Vector3 position)
    {
        bool inLight = LightManager.Instance.IsObjectInLight(position, "Enemy");
        bool inLOS = LightManager.Instance.IsInLOSOfCharacter(position);
        //Debug.Log($"[ENEMYVISIBILITY] {Root.EnemyName} visibility updated: {(inLight ? "Lit" : "UnLit")} {(inLOS ? "InLOS" : "NoLOS")}");
        return inLight && inLOS;
    }

    public void CharacterStartedMoving() => isCharacterMoving = true;
    public void CharacterStoppedMoving() => isCharacterMoving = false;

    public void SetGhostVisible()
    {
        ghost.transform.position = transform.position;
        ghost.SetActive(true);
    }

    public void VisibilityUpdate()
    {
        bool visible = IsVisibleToPlayer(transform.position);
        if (AlwaysVisible) visible = true;

        spriteRenderer.enabled = visible;
        if (!visible && wasVisible && hasBeenSeen)
        {
            Debug.Log($"[ENEMYVISIBILITY] {Root.EnemyName} has become hidden.");
            ghost.SetActive(true);
            ghostPos = transform.position;
        }
        else if (visible && !wasVisible)
        {
            Debug.Log($"[ENEMYVISIBILITY] {Root.EnemyName} has become visible.");
            ghost.SetActive(false);
            hasBeenSeen = true;
        }
        if (!visible && ghost.activeSelf)
        {
            ghost.transform.position = ghostPos;
            bool ghostVisible = IsVisibleToPlayer(ghost.transform.position);
            if (ghostVisible)
            {
                Debug.Log($"[ENEMYVISIBILITY] {Root.EnemyName}'s ghost is visible to the player.");
                ghost.SetActive(false);
            }
        }

        wasVisible = visible;
    }
    private void Update()
    {
        if ((isCharacterMoving || lastPos != transform.position))
        {
            lastPos = transform.position;
            VisibilityUpdate();
        }
    }
}