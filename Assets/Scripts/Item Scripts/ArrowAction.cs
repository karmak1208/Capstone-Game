using System.Collections;
using UnityEngine;

public class ArrowAction : CardAction
{
    public override CardData Data { get; set; }
    [SerializeField] private float speed;
    private float hitDistance;

    protected override IEnumerator OnExecuteAction(Vector3Int cellPos)
    {
        foreach (Transform child in transform.parent)
        {
            if (child.gameObject != gameObject && child.GetComponent<CardAction>() != null)
            {
                Debug.Log("[ARROWACTION] Found CardAction on child: " + child.gameObject.name);
                transform.position = child.position;
                transform.rotation = child.rotation;

                yield return StartCoroutine(ShootRoutine(cellPos));
            }
        }
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            hitDistance = Vector2.Distance(transform.position, parentPos);
            Debug.Log($"[ARROWACTION] Hit obstacle: {collision.name} at distance {hitDistance}");
        }
    }

    private IEnumerator ShootRoutine(Vector3Int cellPos)
    {
        Vector2 dir = (cellPos - parentPos).normalized;
        transform.up = dir;
        float travelTime = Data.range / speed;
        float elapsedTime = 0f;
        while (elapsedTime < travelTime)
        {
            if (hitDistance <= 0)
                transform.position += (Vector3)(dir * speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        RaycastHit2D hit = Physics2D.Raycast(parentPos, dir, hitDistance > 0f ? hitDistance : Data.range, LayerMask.GetMask("characters", "obstacles"));
        Debug.DrawRay(parentPos, dir * (hitDistance > 0f ? hitDistance : Data.range), Color.red, 1f);
        if (hit.collider != null && hit.collider.gameObject != transform.parent.gameObject)
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log($"[ARROWACTION] Hit {hit.collider.name} for {Data.damage} damage!");
                damageable.TakeDamage(Data.damage);
            }
        }
        else { Debug.Log($"[ARROWACTION] Missed!"); }
        hitDistance = 0f;
    }

    public override void PreviewAction()
    {
        throw new System.NotImplementedException();
    }   
}
