using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BowAction : CardAction
{
    public override CardData Data { get; set; }

    [SerializeField] private float offset;

    protected override IEnumerator OnExecuteAction(Vector3Int cellPos)
    {
        List<GameObject> activeCards = transform.parent.GetComponentInParent<InventoryManager>().ActiveCards;
        var ammoCard = activeCards.FirstOrDefault(c => c.GetComponent<CardHandler>().ItemType == "Ammo");

        if (ammoCard == null) { Debug.Log("[BOWACTION] No ammo card found in inventory!"); yield break; }
        else Debug.Log("[BOWACTION] Ammo card found: " + ammoCard.name);

        Vector2 dir = (cellPos - parentPos).normalized;
        transform.position = parentPos + (Vector3)(dir * offset);
        transform.right = dir;
        ammoCard.GetComponent<CardHandler>().UseCard(cellPos, removeCard: true);

        yield return new WaitForSeconds(0.5f);
    }

    public override void PreviewAction()
    {
        throw new System.NotImplementedException();
    }
}
