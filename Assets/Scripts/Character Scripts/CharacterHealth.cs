using UnityEngine;

public class CharacterHealth : MonoBehaviour, IDamageable
{
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private CharacterRoot Root;

    private void Awake()
    {
        Root = GetComponent<CharacterRoot>();
        Root.OnFinishedLoading.AddListener(Initialize);

        IsDead = false;
    }
    void Initialize()
    {
        MaxHealth = Root.Data.Health;
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHealth -= amount;
        Debug.Log($"[CHARACTER HEALTH] {Root.CharacterName} takes {amount} damage, {CurrentHealth}/{MaxHealth} HP remaining.");

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
        CharacterManager.Instance.RemoveCharacterFromParty(Root);
        gameObject.SetActive(false);
        CharacterManager.Instance.OnPlayerDied.Invoke();
        Debug.Log($"[CHARACTER HEALTH] {Root.CharacterName} has died.");
    }
}
