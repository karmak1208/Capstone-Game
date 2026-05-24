using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public List<EnemyRoot> Enemies = new();
    public UnityEvent OnEnemyDied;

    void Start()
    {
        Enemies = FindObjectsByType<EnemyRoot>(FindObjectsSortMode.None).ToList();
        Debug.Log($"EnemyManager found {Enemies.Count} enemies in the scene.");
    }

    public void AddEnemy(EnemyRoot enemy)
    {
        if (!Enemies.Contains(enemy))
        {
            Enemies.Add(enemy);
        }
    }

    public void RemoveEnemy(EnemyRoot enemy)
    {
        if (Enemies.Contains(enemy))
        {
            Enemies.Remove(enemy);
        }
    }
}
