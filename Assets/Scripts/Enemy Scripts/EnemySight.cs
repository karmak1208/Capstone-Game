using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class EnemySight : MonoBehaviour
{
    private EnemyRoot Root;

    public Light2D sightLight;
    private float sightDirAngle;
    private float sightAngle;
    private float sightRange;
    private Vector3 facingDir = Vector3.right;

    public CharacterRoot TargetedPlayer;
    public Vector3 TargetPos;

    private bool isCharacterMoving = false;
    private Vector3 lastPos;

    public bool PlayerInSight;
    public bool PlayerWasInSight;
    public bool PlayerSeenThisTurn;
    public UnityEvent OnPlayerSpotted;

    private void Start()
    {
        Root = GetComponent<EnemyRoot>();
        if (Root == null) Debug.LogError("[ENEMYSIGHT] EnemySight requires an EnemyRoot component.");

        sightLight = GetComponentInChildren<Light2D>();
        if (sightLight == null) Debug.LogError("[ENEMYSIGHT] EnemySight requires a Light2D component.");
        else
        {
            sightDirAngle = sightLight.transform.rotation.eulerAngles.z;
            sightAngle = sightLight.pointLightOuterAngle;
            sightRange = sightLight.pointLightOuterRadius;
        }

        CharacterManager.Instance.OnCharacterStartedMove.AddListener(CharacterStartedMoving);
        CharacterManager.Instance.OnCharacterEndedMove.AddListener(CharacterStoppedMoving);
        CharacterManager.Instance.OnPlayerDied.AddListener(SightUpdate);
        TurnManager.Instance.OnTurnStart.AddListener(HandleTurnStarted);

        //TurnManager.Instance.AfterTurnEnd.AddListener(SightUpdate);
    }
    public void HandleTurnStarted(int turn)
    {
        if (!PlayerSeenThisTurn)
        {
            Root.TransitionTo(new PatrolState(Root));
        }
        PlayerSeenThisTurn = false;
    }

    public void SightUpdate()
    {
        List<CharacterRoot> players = AllPlayersInSight();
        if (players.Count == 0) { PlayerInSight = false; }

        if (players.Count > 0)
        {
            PlayerInSight = true;
            PlayerSeenThisTurn = true;
            if (!PlayerWasInSight)
            {
                Debug.Log($"[ENEMY SIGHT] New Player Spotted");
                OnPlayerSpotted.Invoke();
                TargetedPlayer = players[0]; // For now, just target the first player in sight.
                TargetPos = TargetedPlayer.Position;
                if (Root.State is not ChaseState)
                    Root.TransitionTo(new ChaseState(Root));
            }
        }

        if (Root.State is ChaseState)
        {
            if (IsPlayerInSight(TargetedPlayer))
            {
                Vector3 toTarget = TargetedPlayer.Position - transform.position;
                SetFacingDirection(toTarget);
                TargetPos = TargetedPlayer.Position;
            }
        }

        PlayerWasInSight = PlayerInSight;
    }

    public void CharacterStartedMoving() => isCharacterMoving = true;
    public void CharacterStoppedMoving() => isCharacterMoving = false;

    void Update()
    {
        if (!Root.Visibility.IsVisible)
        {
            sightLight.enabled = false;
            return;
        }
        else
        {
            sightLight.enabled = true;
        }

        if ((isCharacterMoving || lastPos != transform.position)) 
        {
            lastPos = transform.position;
            SightUpdate();
        }
    }

    public bool IsPlayerInSight(CharacterRoot player)
    {

        Vector3 toPlayer = (player.Position - transform.position);
        if (toPlayer.magnitude > sightRange) return false;

        float toPlayerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;
        float delta = Mathf.Abs(Mathf.DeltaAngle(sightDirAngle, toPlayerAngle));
        if (delta <= sightAngle / 2f)
        {
            RaycastHit2D hit = Physics2D.Raycast(player.Position, (transform.position - player.Position).normalized, Vector3.Distance(player.Position, transform.position), ~LayerMask.GetMask("characters", "players", "UI"));
            if (hit.collider == null) return true;
        }
        
        return false;
    }
    List<CharacterRoot> AllPlayersInSight()
    {
        if (!Root.Visibility.IsVisible) return null;
        List<CharacterRoot> playersInSight = new();
        List<CharacterRoot> players = CharacterManager.Instance.PartyMembers;
        foreach (CharacterRoot player in players)
        {
            if (IsPlayerInSight(player)) playersInSight.Add(player);
        }
        return playersInSight;
    }

    public void SetFacingDirection(Vector3 direction)
    {
        facingDir = direction.normalized;
        sightDirAngle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg - 90f;
        sightLight.transform.rotation = Quaternion.Euler(0, 0, sightDirAngle);
    }

    public void LookAt(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        SetFacingDirection(dir);
    }
}
