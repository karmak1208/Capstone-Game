using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private EnemyRoot Root;

    public GameObject WeaponPrefab;
    public CardData _CardData;
    private GameObject Card;
    public string weaponName;
    public bool HasAttackedThisTurn;

    void Start()
    {
        Root = GetComponent<EnemyRoot>();
        if (Root == null) Debug.LogError($"[EnemyAttack] No CharacterRoot found on {gameObject.name}");

        TurnManager.Instance.OnTurnEnd.AddListener(ResetAttack);
        LoadCard();
    }

    async void LoadCard()
    {
        _CardData = await CardLoader.Instance?.LoadObject<CardData>(weaponName);
        Card = Instantiate(WeaponPrefab, transform.position, Quaternion.identity, transform);
        Card.GetComponent<CardAction>().Initialize(_CardData, gameObject);
        Card.SetActive(false);
    }

    private void ResetAttack() => HasAttackedThisTurn = false;

    public void Attack(Vector3Int targetCell)
    {
        Card.SetActive(true);
        Card.GetComponent<CardAction>()?.EnemyExecuteAction(targetCell);
    }

    void Update()
    {
        if (Root.State is ChaseState)
        {
            if (!HasAttackedThisTurn && !Root.Movement.isMoving && Vector2.Distance(Root.Sight.TargetedPlayer.Position, transform.position) <= Mathf.Sqrt(2))
            {
                Attack(Root.Movement.floormap.WorldToCell(Root.Sight.TargetPos));
                HasAttackedThisTurn = true;
            }
        }
    }
}
