using System.Collections.Generic;
using UnityEngine;

public class EnemyVisibility : MonoBehaviour
{
    private EnemyRoot Root;
    private SpriteRenderer spriteRenderer;
    private Vector3 lastPos;
    private bool isCharacterMoving = false;
    private GameObject ghost;

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
    }

    bool IsVisibleToPlayer(Vector3 position)
    {
        bool inLight = LightManager.Instance.IsObjectInLight(position);
        bool inLOS = LightManager.Instance.IsInLOSOfCharacter(position);
        Debug.Log($"[ENEMY] {Root.EnemyName} visibility check: InLight={inLight}, InLOS={inLOS}");
        return inLight && inLOS;
    }

    public void CharacterStartedMoving() => isCharacterMoving = true;
    public void CharacterStoppedMoving() => isCharacterMoving = false;

    bool wasVisible = true;
    Vector2 ghostPos;
    private void Update()
    {
        if (isCharacterMoving || lastPos != transform.position)
        {
            lastPos = transform.position;
            bool visible = IsVisibleToPlayer(transform.position);
            Debug.Log($"[ENEMY] {Root.EnemyName} visibility updated: {(visible ? "Visible" : "Hidden")}");
            spriteRenderer.enabled = visible;
            if (!visible && wasVisible)
            {
                Debug.Log($"[ENEMY] {Root.EnemyName} has become hidden.");
                ghost.SetActive(true);
                ghostPos = transform.position;
            }
            else if (visible && !wasVisible)
            { 
                Debug.Log($"[ENEMY] {Root.EnemyName} has become visible.");
                ghost.SetActive(false);
            }
            if (!visible && ghost.activeSelf)
            {
                ghost.transform.position = ghostPos;
                bool ghostVisible = IsVisibleToPlayer(ghost.transform.position);
                if (ghostVisible) 
                {
                    ghost.SetActive(false);
                }
            }

            wasVisible = visible;
        }
    }
}