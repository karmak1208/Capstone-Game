using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private void Start()
    {
        TurnManager.Instance.OnTurnEnd.AddListener(TempMove);
       
    }

    void TempMove()
    {
        StartCoroutine(MoveToTarget(transform.position + new Vector3(1, 0, 0)));
    }

    IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 10 * Time.deltaTime);
            yield return null;
        }
    }
}
