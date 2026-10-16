using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// When the player enters the trigger, hides the specified list of target assets/gameObjects.
/// </summary>
public class TriggerHideObjects : MonoBehaviour
{
    [Header("Assets to Hide")]
    [Tooltip("Drag the GameObjects you want to hide when entering this trigger.")]
    public List<GameObject> targetObjectsToHide = new List<GameObject>();

    [Header("Settings")]
    [Tooltip("If true, destroys the objects instead of just hiding them.")]
    public bool destroyInsteadOfHide = false;

    [Tooltip("Optional: Only trigger if the entering object has this specific tag (e.g., 'Player').")]
    public string requiredTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        // Check tag if specified
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            return;
        }

        HideTargets();
    }

    /// <summary>
    /// Hides or destroys all objects in the list.
    /// </summary>
    public void HideTargets()
    {
        if (targetObjectsToHide == null) return;

        foreach (GameObject obj in targetObjectsToHide)
        {
            if (obj != null)
            {
                if (destroyInsteadOfHide)
                {
                    Destroy(obj);
                }
                else
                {
                    obj.SetActive(false);
                }
            }
        }
        
        Debug.Log("[TriggerHide] Target assets hidden successfully.");
    }
}