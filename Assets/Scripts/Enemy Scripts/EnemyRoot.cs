using UnityEngine;

public class EnemyRoot : MonoBehaviour
{
    public EnemyMovement Movement { get; private set; }
    public EnemyHealth Health { get; private set; }

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
    }
     public Vector2 Position => transform.position;
     public string EnemyName = "Unnamed";
}

