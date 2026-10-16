using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Causes a group of renderers to randomly flash different materials 
/// until a specific quest is completed, then assigns a fixed final material.
/// </summary>
public class FlashingMaterialQuestSync : MonoBehaviour
{
    [Header("Quest Trigger")]
    [Tooltip("The quest that needs to be completed to stop the flashing.")]
    public QuestData requiredQuest;

    [Header("Target Objects")]
    [Tooltip("Drag the renderers (MeshRenderer) you want to affect into this list.")]
    public List<Renderer> targetRenderers = new List<Renderer>();

    [Header("Material Settings")]
    [Tooltip("The palette of materials to randomly pick from during the flashing phase.")]
    public Material[] flashMaterials;
    
    [Tooltip("The final material all objects will change to once the quest is done.")]
    public Material finalMaterial;

    [Tooltip("How fast the materials change (in seconds).")]
    public float flashInterval = 0.1f;

    private bool isQuestCompleted = false;

    void Start()
    {
        if (QuestManager.Instance == null || requiredQuest == null)
        {
            Debug.LogWarning("[FlashingMaterial] QuestManager or Required Quest is missing.");
            return;
        }

        // 1. Check if the quest is ALREADY completed when the scene loads
        if (QuestManager.Instance.IsQuestCompleted(requiredQuest))
        {
            ApplyFinalMaterial();
        }
        else
        {
            // 2. If not completed, subscribe to the completion event and start flashing
            QuestManager.Instance.OnQuestCompleted += OnQuestCompletedEvent;
            StartCoroutine(FlashMaterialsRoutine());
        }
    }

    void OnDestroy()
    {
        // Always unsubscribe from events to prevent memory leaks!
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompletedEvent;
        }
    }

    /// <summary>
    /// Listens for any quest completion and checks if it's the one we are waiting for.
    /// </summary>
    private void OnQuestCompletedEvent(QuestData completedQuest)
    {
        if (completedQuest == requiredQuest && !isQuestCompleted)
        {
            ApplyFinalMaterial();
        }
    }

    /// <summary>
    /// Coroutine that continuously randomizes materials until the quest is finished.
    /// </summary>
    private IEnumerator FlashMaterialsRoutine()
    {
        while (!isQuestCompleted)
        {
            if (flashMaterials != null && flashMaterials.Length > 0)
            {
                foreach (Renderer r in targetRenderers)
                {
                    if (r != null)
                    {
                        // Assign a random material from the array
                        r.material = flashMaterials[Random.Range(0, flashMaterials.Length)];
                    }
                }
            }

            // Wait for the specified interval before changing again
            yield return new WaitForSeconds(flashInterval);
        }
    }

    /// <summary>
    /// Stops the flashing and unifies all renderers to the final fixed material.
    /// </summary>
    private void ApplyFinalMaterial()
    {
        isQuestCompleted = true; // This breaks the while-loop in the Coroutine
        StopAllCoroutines();     // Instantly halt the flashing process

        // Apply the solid, non-changing material
        if (finalMaterial != null)
        {
            foreach (Renderer r in targetRenderers)
            {
                if (r != null)
                {
                    r.material = finalMaterial;
                }
            }
            Debug.Log($"[FlashingMaterial] Quest '{requiredQuest.questID}' completed. All meshes set to final material.");
        }
    }
}