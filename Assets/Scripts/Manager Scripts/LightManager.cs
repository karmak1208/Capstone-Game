using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        sceneLights.AddRange(FindObjectsByType<Light2D>(FindObjectsSortMode.None).Where(l => !l.name.Contains("Global")));
    }

    /// <summary>
    /// List of all Light2D components in the scene, excluding the global light. 
    /// This list is populated in the Awake method by finding all Light2D objects and filtering out the one named "Global Light 2D".
    /// NOTE: Does not currently update newly created lights after awake.
    /// </summary>
    public List<Light2D> sceneLights = new();

    /// <summary>
    /// Determines if the given position is illuminated by any lights in the scene (Excluding the global light). 
    /// It checks if the position is within the outer radius of any light and then performs a raycast to ensure there are no obstacles blocking the light from reaching that position.
    /// </summary>
    /// <param name="position">The position to check for illumination.</param>
    /// <returns>True if the position is illuminated by any light, false otherwise.</returns>
    public bool IsObjectInLight(Vector3 position)
    {
        if (sceneLights.Count == 0) { Debug.Log("[LIGHTMANAGER] No lights in sceneLights"); return false; }

        foreach (var light in sceneLights)
        {
            Vector3 lightPos = light.transform.position;
            float distToLight = Vector3.Distance(position, lightPos);

            if (distToLight <= light.pointLightOuterRadius)
            {
                Debug.Log($"[LIGHTMANAGER] Position {position} is within light {light.name} range.");
                Vector3 dirToLight = (lightPos - position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(position, dirToLight, distToLight, ~LayerMask.GetMask("characters", "players", "lights", "UI"));

                if (hit.collider == null)
                    return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Determines if the given position is within the line of sight of any character in the party.
    /// It performs a raycast from each character to the position to check for obstacles.
    /// </summary>
    /// <param name="position">The position to check for line of sight.</param>
    /// <returns>True if the position is within the line of sight of any character, false otherwise.</returns>
    public bool IsInLOSOfCharacter(Vector3 position)
    {
        List<CharacterRoot> characters = CharacterManager.Instance.PartyMembers;
        if (characters.Count == 0) { Debug.Log("[LIGHTMANAGER] No characters in CharacterManager.Instance.PartyMembers"); return false; }
        foreach (var character in characters)
        {
            Vector3 characterPos = character.Position;
            RaycastHit2D hit = Physics2D.Raycast(characterPos, (position - characterPos).normalized, Vector3.Distance(characterPos, position), ~LayerMask.GetMask("characters", "players", "UI"));
            if (hit.collider == null) // No obstacles between character and position
                return true;

        }
        return false;
    }
}

