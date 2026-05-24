using System.Collections;
using System.Linq;
using UnityEngine;

public class FamiliarAction : CardAction
{
    public override CardData Data { get; set; }
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Sprite familiarSprite;

    protected override IEnumerator OnExecuteAction(Vector3Int cellPos)
    {
        Debug.Log("[FAMILIARACTION] Executing Familiar Action at cell: " + cellPos);

        GameObject familiar = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(obj => obj.name == "Familiar");
        if (familiar != null)
        {
            familiar.SetActive(true);
            familiar.name = "Familiar";
            familiar.transform.position = cellPos;

            SpriteRenderer sr = familiar.GetComponent<SpriteRenderer>();
            sr.sprite = familiarSprite;

            CharacterRoot root = familiar.GetComponent<CharacterRoot>();
            CharacterManager.Instance.AddCharacterToParty(root);
        }

        yield return null;
    }

    public override void PreviewAction()
    {
        throw new System.NotImplementedException();
    }
}
