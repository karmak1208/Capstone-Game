using System.Collections;
using UnityEngine;

public class SwordAction : CardAction
{
    public AnimationCurve swingCurve;
    public override CardData Data { get; set; }

    private bool isSwinging = false;
    private float swingTimer = 0f;

    [Header("Swing Settings")]
    [SerializeField] private float swingDuration;
    [SerializeField] private float swingOffset;
    [SerializeField] private float startAngle;
    [SerializeField] private float endAngle;

    protected override IEnumerator OnExecuteAction(Vector3Int cellPos)
    {
        Vector2 direction = (cellPos - parentPos).normalized;
        isSwinging = true;
        swingTimer = 0f;

        yield return StartCoroutine(SwingCoroutine(direction, parentPos));
    }
    Vector2 RotateBy(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }

    private IEnumerator SwingCoroutine(Vector2 direction, Vector2 pivot)
    {
        Vector2 startDir = RotateBy(direction, startAngle);
        Vector2 endDir = RotateBy(direction, endAngle);

        while (isSwinging)
        {
            swingTimer += Time.deltaTime;
            float t = Mathf.Clamp01(swingTimer / swingDuration);

            // Lerp the angle from start to end
            float angle = Vector2.SignedAngle(Vector2.right,
                              Vector2.Lerp(startDir, endDir, swingCurve.Evaluate(t)).normalized);
            // Convert angle to a world position around the pivot
            Vector2 offset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * swingOffset;

            transform.position = pivot + offset;

            // Rotate the sword sprite to match the direction
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            if (t >= 1f) isSwinging = false;
            yield return null;
        }

        Transform col = transform.Find("HitCollider");
        MoveCollider(direction, col);

        col.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f); // Keep collider active for a short time to ensure it registers hits
        col.gameObject.SetActive(false);
    }

    void MoveCollider(Vector2 direction, Transform collider)
    {
        
        if (collider != null)
        {
            collider.position = parentPos;
            collider.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null && collision.transform != transform.parent)
        {
            Debug.Log($"[SWORD] Collided with {collision.gameObject.name}");

            damageable.TakeDamage(Data.damage); 
        }
    }

    public override void PreviewAction()
    {
        throw new System.NotImplementedException();
    }
}
