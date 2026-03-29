using UnityEngine;

public class CharacterRoot : MonoBehaviour
{
    public MovementController Movement { get; private set; }
    public TileHighlighter Highlighter { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<MovementController>();
        Highlighter = GetComponentInChildren<TileHighlighter>();
        if (Movement == null)
        {
            Debug.LogError("CharacterRoot requires a MovementController component.");
        }
        if (Highlighter == null)
        {
            Debug.LogError("CharacterRoot requires a TileHighlighter component in its children.");
        }
    }
    public Vector3 Position => transform.position;
    public string CharacterName = "Unnamed";
    public Sprite CharacterSprite;

    public void SetActive(bool isActive)
    {
        // Tell the movement controller whether to accept new destinations.
        // (We don't stop mid-path — the character finishes their current move.)
        if (Movement != null)
            Movement.SetInputEnabled(isActive);
        if (Highlighter != null)
            Highlighter.SetActive(isActive);

        // You could also show/hide a selection ring, outline, etc. here.
    }
}

