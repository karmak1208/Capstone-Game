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
    public Vector3 IdleFacingDir;

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

        if (Root.State is IdleState)
        {
            IdleFacingDir = facingDir;
        }

        CharacterManager.Instance.OnCharacterStartedMove.AddListener(CharacterStartedMoving);
        CharacterManager.Instance.OnCharacterEndedMove.AddListener(CharacterStoppedMoving);
        CharacterManager.Instance.OnPlayerDied.AddListener(SightUpdate);
        TurnManager.Instance.OnTurnStart.AddListener(HandleTurnStarted);
    }
    public void HandleTurnStarted(int turn)
    {
        if (!PlayerSeenThisTurn)
        {
            switch (Root.initialState)
            {
                case startingState.Idle:
                    Debug.Log($"[ENEMY SIGHT] Lost sight of player, returning to idle.");
                    Root.TransitionTo(new IdleState(Root));
                    break;
                case startingState.Patrol:
                    Debug.Log($"[ENEMY SIGHT] Player not found, returning to patrol.");
                    Root.TransitionTo(new PatrolState(Root));
                    break;
            }
            
        }
        PlayerSeenThisTurn = false;
    }

    public void SightUpdate()
    {
        List<CharacterRoot> players = AllPlayersInSight();
        if (players == null) { Debug.LogError("[ENEMY SIGHT] Players list is null."); }
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
            if (IsPlayerInSight(TargetedPlayer.Position))
            {
                Vector3 toTarget = TargetedPlayer.Position - transform.position;
                SetFacingDirection(toTarget);
                TargetPos = TargetedPlayer.Position;
            }
        }

        PlayerWasInSight = PlayerInSight;

        List<EnemyRoot> enemiesInSight = AllEnemiesInSight();
        if (enemiesInSight.Count <= 0) { Debug.Log("[ENEMY SIGHT] No Enemies in sight."); }

        foreach (EnemyRoot enemy in enemiesInSight)
        {
            Debug.Log($"[ENEMY SIGHT] Enemy {enemy.EnemyName} is in sight.");
            enemy.OnDie.RemoveListener(WitnessedEnemyDie);
            enemy.OnDie.AddListener(WitnessedEnemyDie);
        }
    }

    private void WitnessedEnemyDie()
    {
        Debug.Log($"[ENEMY SIGHT] Witnessed an enemy die, updating sight.");
        Root.TransitionTo(new AlertState(Root));
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

    List<EnemyRoot> AllEnemiesInSight()
    {
        List<EnemyRoot> enemiesInSight = new();
        List<EnemyRoot> enemies = EnemyManager.Instance.Enemies;
        Debug.Log($"[ENEMY SIGHT] Checking sight against {enemies.Count} enemies.");
        foreach (EnemyRoot enemy in enemies)
        {
            if (enemy == Root) continue;
            Debug.Log($"[ENEMY SIGHT] {Root.EnemyName} Checking if enemy {enemy.EnemyName} is in sight.");
            if (IsPlayerInSight(enemy.Position))
            {
                enemiesInSight.Add(enemy);
                Debug.Log($"[ENEMY SIGHT] {Root.EnemyName} Enemy {enemy.EnemyName} is in sight.");
            }
        }
        return enemiesInSight;
    }

    public bool IsPlayerInSight(Vector3 pos)
    {

        Vector3 toPlayer = (pos - transform.position);
        if (toPlayer.magnitude > sightRange) return false;

        float toPlayerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;
        float delta = Mathf.Abs(Mathf.DeltaAngle(sightDirAngle, toPlayerAngle));
        if (delta <= sightAngle / 2f)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (pos - transform.position).normalized, Vector3.Distance(pos, transform.position), ~LayerMask.GetMask("characters", "players", "UI"));
            if (hit.collider == null) return true;
        }
        
        return false;
    }
    List<CharacterRoot> AllPlayersInSight()
    {
        List<CharacterRoot> playersInSight = new();
        if (!Root.Visibility.IsVisible) return playersInSight;

        List<CharacterRoot> players = CharacterManager.Instance.PartyMembers;
        foreach (CharacterRoot player in players)
        {
            if (IsPlayerInSight(player.Position)) playersInSight.Add(player);
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
