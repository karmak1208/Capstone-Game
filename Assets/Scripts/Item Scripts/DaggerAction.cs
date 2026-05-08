using UnityEngine;
using System.Collections;

public class DaggerAction : CardAction
{
    public override CardData Data { get; set; }
    public AnimationCurve stabCurve;
    public Vector3 parentPos => transform.parent.parent.position;
    private bool isStabbing = false;
    private float stabTimer = 0f;
    [SerializeField] private float stabSpeed;


    protected override IEnumerator OnExecuteAction(Vector3Int cellPos)
    {
        isStabbing = true; 
        stabTimer = 0f;
        yield return StartCoroutine(StabCoroutine(cellPos));
    }

    private IEnumerator StabCoroutine(Vector3Int cellPos)
    {
        Vector2 dir = (cellPos - parentPos).normalized;
        transform.position = parentPos;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, dir);

        while (isStabbing)
        {
            stabTimer += Time.deltaTime * stabSpeed;
            float t = Mathf.Clamp01(stabTimer / Data.range); ;
            transform.position = Vector3.Lerp(parentPos, parentPos + (Vector3)dir * Data.range, stabCurve.Evaluate(t));
            if (t >= 1f) isStabbing = false;
            yield return null;
        }

        Debug.Log($"[DAGGER] Stabbed towards {cellPos}!");
        RaycastHit2D hit = Physics2D.Raycast(transform.parent.parent.position, dir, Data.range, LayerMask.GetMask("characters"));
        Debug.DrawRay(transform.parent.parent.position, dir * Data.range, Color.red, 1f);
        if (hit.collider != null && hit.collider.gameObject != transform.parent.parent.gameObject)
        {
            Debug.Log($"[DAGGER] Hit {hit.collider.name} for {Data.damage} damage!");
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(Data.damage);
            }
        }
        else { Debug.Log($"[DAGGER] Missed!"); } 

    }

    public override void PreviewAction()
    {

    }
}
