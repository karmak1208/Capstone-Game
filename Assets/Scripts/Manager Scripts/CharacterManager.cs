using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
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
        TurnManager.Instance.OnTurnEnd.AddListener(DeselectCharacter);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnEnd.RemoveListener(DeselectCharacter);
        }
    }

    [SerializeField] private List<CharacterRoot> partyMembers;
    public TextMeshProUGUI characterNameDisplay;
    public TextMeshProUGUI moveRangeText;

    public CharacterRoot ActiveCharacter { get; private set; }

    public void SwitchTo(CharacterRoot newCharacter)
    {
        if (newCharacter == ActiveCharacter)
        {
            DeselectCharacter();
            return;
        }

        // Deactivate the OLD character (stop accepting input, hide selection ring, etc.)
        if (ActiveCharacter != null)
            ActiveCharacter.SetActive(false);

        ActiveCharacter = newCharacter;
        ActiveCharacter.SetActive(true);
        characterNameDisplay.text = ActiveCharacter.CharacterName;
        moveRangeText.text = $"Move Range: {ActiveCharacter.Movement.remainingMovementRange}";

        Debug.Log($"[CharacterManager] Switched to: {ActiveCharacter.CharacterName}");
    }

    public void DeselectCharacter()
    {
        if (ActiveCharacter != null)
        {
            ActiveCharacter.SetActive(false);
        }
        ActiveCharacter = null;
        characterNameDisplay.text = "No Active Character";
        moveRangeText.text = "";
        Debug.Log("[CharacterManager] Deselected character.");
        return;
    }
}
