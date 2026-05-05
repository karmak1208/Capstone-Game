using UnityEngine;

public interface IDamageable
{
    float MaxHealth { get; }
    float CurrentHealth { get; }
    bool IsDead { get; }

    void TakeDamage(float amount);
    void Heal(float amount);
    void Die();
}
