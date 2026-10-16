using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A centralized manager that takes a list of GameObjects and makes them persist across scenes using DontDestroyOnLoad.
/// </summary>
public class PersistentManager : MonoBehaviour
{
    [Header("Unique Key for This Manager Group")]
    [Tooltip("Prevents duplicate persistent masters if you accidentally return to the first scene.")]
    public string persistentID = "GlobalPersistentMaster";

    [Header("Objects to Keep Alive Across Scenes")]
    [Tooltip("Drag all the GameObjects you want to persist (QuestManager, Volume, UI Canvas, etc.) into this list.")]
    public List<GameObject> objectsToPersist = new List<GameObject>();

    // Static dictionary to track already existing persistent master IDs
    private static HashSet<string> instantiatedIDs = new HashSet<string>();

    void Awake()
    {
        // 1. Prevent duplicate Master objects across scene loads
        if (!string.IsNullOrEmpty(persistentID))
        {
            if (instantiatedIDs.Contains(persistentID))
            {
                Debug.Log($"[PersistentManager] Duplicate persistent master found for ID '{persistentID}'. Destroying this new instance.");
                Destroy(gameObject);
                return;
            }
            else
            {
                instantiatedIDs.Add(persistentID);
            }
        }

        // 2. Make this main GameObject persistent
        DontDestroyOnLoad(gameObject);

        // 3. Make all listed objects persistent as well, and unparent them if needed 
        // (Unity requires DontDestroyOnLoad objects to be at the root of the hierarchy)
        foreach (GameObject obj in objectsToPersist)
        {
            if (obj != null)
            {
                // Unparenting is optional, but recommended so they don't get destroyed if the parent transforms weirdly
                obj.transform.SetParent(null); 
                DontDestroyOnLoad(obj);
                Debug.Log($"[PersistentManager] Object '{obj.name}' is now persistent across scenes.");
            }
        }

        Debug.Log($"[PersistentManager] Persistent Master '{gameObject.name}' initialized successfully.");
    }
}