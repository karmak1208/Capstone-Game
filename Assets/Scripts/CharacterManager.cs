using UnityEngine;
using System.Collections.Generic;

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
            DontDestroyOnLoad(gameObject);
        }
    }

    [Header("Party Setup")]
    [SerializeField] private List<CharacterRoot> _partyMembers;
    public CharacterRoot ActiveCharacter { get; private set; }

    private void Start()
    {
        // Auto-activate the first character in the list on game start,
        // if the list isn't empty.
        if (_partyMembers.Count > 0)
            SwitchTo(_partyMembers[0]);
    }

    public void SwitchTo(CharacterRoot newCharacter)
    {
        // Guard: if this character is already active, do nothing.
        if (newCharacter == ActiveCharacter) return;

        // Deactivate the OLD character (stop accepting input, hide selection ring, etc.)
        if (ActiveCharacter != null)
            ActiveCharacter.SetActive(false);

        // Assign the new active character.
        ActiveCharacter = newCharacter;

        // Activate the NEW character (resume input, show selection ring, etc.)
        ActiveCharacter.SetActive(true);

        Debug.Log($"[CharacterManager] Switched to: {ActiveCharacter.CharacterName}");
    }
}
