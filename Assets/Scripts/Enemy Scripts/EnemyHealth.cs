using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private EnemyRoot Root;

    private void Awake()
    {
        Root = GetComponent<EnemyRoot>();
        if (Root == null)
        {
            Debug.LogError("EnemyHealth requires an EnemyRoot component.");
        }

        MaxHealth = 1;
        CurrentHealth = MaxHealth;
        IsDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHealth -= amount;
        Debug.Log($"[ENEMY] {Root.EnemyName} takes {amount} damage, {CurrentHealth}/{MaxHealth} HP remaining.");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Root.OnDie.Invoke();
        EnemyManager.Instance.OnEnemyDied.Invoke();
        EnemyManager.Instance.RemoveEnemy(Root);
        Destroy(gameObject);
        Debug.Log($"[ENEMY] {Root.EnemyName} has died.");
    }
}
