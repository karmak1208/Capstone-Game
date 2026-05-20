using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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

    [SerializeField] private List<CharacterRoot> _partyMembers;
    public List<CharacterRoot> PartyMembers { get => _partyMembers; private set => _partyMembers = value; }
    public TextMeshProUGUI characterNameDisplay;
    public TextMeshProUGUI moveRangeText;

    public CharacterRoot ActiveCharacter { get; private set; }

    public UnityEvent OnCharacterStartedMove = new UnityEvent();
    public UnityEvent OnCharacterEndedMove = new UnityEvent();
    public UnityEvent OnPlayerDied;

    public void AddCharacterToParty(CharacterRoot character)
    {

    }

    public void RemoveCharacterFromParty(CharacterRoot character)
    {
        if (_partyMembers.Contains(character))
        {
            if (ActiveCharacter == character) DeselectCharacter();
            _partyMembers.Remove(character);
        }
    }

    /// <summary>
    /// Switches the active character to the specified one. If the same character is selected again, it will be deselected.
    /// </summary>
    /// <param name="newCharacter">The character to switch to.</param>
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

    /// <summary>
    /// Deselects the currently active character, if any. This will stop accepting input for that character and hide any selection indicators.
    /// </summary>
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
