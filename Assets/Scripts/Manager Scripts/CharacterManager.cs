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
        }
    }

    [Header("Party Setup")]
    [SerializeField] private List<CharacterRoot> _partyMembers;
    public CharacterRoot ActiveCharacter { get; private set; }

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
