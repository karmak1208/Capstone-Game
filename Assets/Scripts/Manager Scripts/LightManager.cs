using System.Collections;
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
        StartCoroutine(UpdateLights());

        EnemyManager.Instance.OnEnemyDied.AddListener(QueueLightUpdate);
    }

    public List<Light2D> sceneLights = new();

    void QueueLightUpdate() => StartCoroutine(UpdateLights());

    public IEnumerator UpdateLights()
    {
        // Wait 2 frames to ensure lights have been enabled/disabled 
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        sceneLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None).Where(l => !l.name.Contains("Global") && !l.name.Contains("Sight")).ToList();
    }

    /// <summary>
    /// Determines if the given position is illuminated by any lights in the scene (Excluding the global light). 
    /// It checks if the position is within the outer radius of any light and then performs a raycast to ensure there are no obstacles blocking the light from reaching that position.
    /// </summary>
    /// <param name="position">The position to check for illumination.</param>
    /// <returns>True if the position is illuminated by any light, false otherwise.</returns>
    public bool IsObjectInLight(Vector3 position, string excludeTag = null)
    {
        if (sceneLights.Count == 0) { Debug.Log("[LIGHTMANAGER] No lights in sceneLights"); return false; }

        foreach (var light in sceneLights)
        {
            if (light.transform.root.CompareTag(excludeTag)) continue;
            Vector3 lightPos = light.transform.position;
            float distToLight = Vector3.Distance(position, lightPos);

            if (distToLight <= light.pointLightOuterRadius)
            {
                Vector3 dirToLight = (lightPos - position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(position, dirToLight, distToLight, ~LayerMask.GetMask("characters", "players", "lights", "objects", "UI"));

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
            RaycastHit2D hit = Physics2D.Raycast(characterPos, (position - characterPos).normalized, Vector3.Distance(characterPos, position), ~LayerMask.GetMask("characters", "players", "objects", "UI"));

            if (hit.collider == null) // No obstacles between character and position
                return true;

        }
        return false;
    }
}

