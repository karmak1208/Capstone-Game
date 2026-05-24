using System;
using UnityEngine;
using UnityEngine.Events;


public interface IEnemyState
{
    void Enter();

    void Exit();
}

public class IdleState : IEnemyState
{
    EnemyRoot enemy;
    public IdleState(EnemyRoot enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        Debug.Log($"[ENEMY] {enemy.EnemyName} has entered Idle State");
        TurnManager.Instance.OnTurnEnd.AddListener(enemy.Movement.ReturnToIdlePos);
    }

    public void Exit()
    {
        Debug.Log($"[ENEMY] {enemy.EnemyName} is exiting Idle State");
        TurnManager.Instance.OnTurnEnd.RemoveListener(enemy.Movement.ReturnToIdlePos);
    }
}

public class PatrolState : IEnemyState
{
    EnemyRoot enemy;
    public PatrolState(EnemyRoot enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        Debug.Log($"[ENEMY] {enemy.EnemyName} has entered Patrol state.");
        TurnManager.Instance.OnTurnEnd.AddListener(enemy.Movement.StartPatrolMove);
    }
    public void Exit()
    {
        Debug.Log($"[ENEMY] {enemy.EnemyName} is exiting Patrol state.");
        TurnManager.Instance.OnTurnEnd.RemoveListener(enemy.Movement.StartPatrolMove);
    }
}
public class ChaseState: IEnemyState
{
    EnemyRoot enemy;
    public ChaseState(EnemyRoot enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        Debug.Log($"[ENEMY] {enemy.EnemyName} has entered Chase State");
        TurnManager.Instance.OnTurnEnd.AddListener(enemy.Movement.StartChaseMove);
        enemy.Sight.sightLight.color = Color.red;

    }

    public void Exit()
    {
        Debug.Log($"[ENEMY] {enemy.EnemyName} is exiting Chase State");
        TurnManager.Instance.OnTurnEnd.RemoveListener(enemy.Movement.StartChaseMove);
        enemy.Sight.sightLight.color = Color.yellow;
    }
}

public class AlertState : IEnemyState
{
    EnemyRoot enemy;
    public AlertState(EnemyRoot enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        Debug.Log($"[ENEMY] {enemy.EnemyName} has entered Alert State");
        enemy.Sight.sightLight.color = Color.orange;
        enemy.Visibility.SetGhostVisible();
    }

    public void Exit()
    {
        Debug.Log($"[ENEMY] {enemy.EnemyName} is exiting Alert State");
        enemy.Sight.sightLight.color = Color.yellow;
    }
}

public enum startingState { Idle, Patrol, Alert, Chase }

public class EnemyRoot : MonoBehaviour
{
    public EnemyMovement Movement { get; private set; }
    public EnemyHealth Health { get; private set; }
    public EnemyVisibility Visibility { get; private set; }
    public EnemySight Sight { get; private set; }
    public EnemyAttack Attack { get; private set; }
    private void Awake()
    {
        Movement = GetComponent<EnemyMovement>();
        if (Movement == null)
        {
            Debug.LogError("EnemyRoot requires an EnemyMovement component.");
        }

        Health = GetComponent<EnemyHealth>();
        if (Health == null)
        {
            Debug.LogError("EnemyRoot requires an EnemyHealth component.");
        }

        Visibility = GetComponent<EnemyVisibility>();
        if (Visibility == null)
        {
            Debug.LogError("EnemyRoot requires an EnemyVisibility component.");
        }

        Sight = GetComponent<EnemySight>();
        if (Sight == null)
        {
            Debug.LogError("EnemyRoot requires an EnemySight component.");
        }

        Attack = GetComponent<EnemyAttack>();
        if (Attack == null)
        {
            Debug.LogError("EnemyRoot requires an EnemyAttack component.");
        }
    }

    void Start()
    {
        switch (initialState)
        {
            case startingState.Idle:
                TransitionTo(new IdleState(this));
                break;
            case startingState.Patrol:
                TransitionTo(new PatrolState(this));
                break;
            case startingState.Alert:
                TransitionTo(new AlertState(this));
                break;
            case startingState.Chase:
                TransitionTo(new ChaseState(this));
                break;
        }
    }

    [SerializeField] public startingState initialState;

    public IEnemyState State;
    public UnityEvent OnDie;

    public void TransitionTo(IEnemyState newState)
    {
        State?.Exit();
        State = newState;
        State.Enter();
    }

    public Vector3 Position => transform.position;
    public string EnemyName = "Unnamed";
}

