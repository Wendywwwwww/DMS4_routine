using UnityEngine;

/// <summary>
/// An interactive object tied to a specific quest.
/// Changes highlight color based on multiple prerequisite statuses and completes the quest upon interaction.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class QuestInteractable : MonoBehaviour, IInteractable
{
    [Header("Quest Settings")]
    [Tooltip("The quest this object will complete when interacted with.")]
    public QuestData associatedQuest;

    private MeshRenderer meshRenderer;
    private Color originalColor;
    private bool isAlreadyInteracted = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;
    }

    public void OnHoverEnter()
    {
        if (isAlreadyInteracted) return;

        // Check if any prerequisite quests are still unfinished.
        if (associatedQuest != null && !ArePrerequisitesMet(associatedQuest))
        {
            // Prerequisite not met: Highlight RED to indicate it's locked.
            if (meshRenderer != null) meshRenderer.material.color = Color.red;
            return;
        }

        // Prerequisites met (or none exist): Highlight YELLOW to indicate it's interactable.
        if (meshRenderer != null) meshRenderer.material.color = Color.yellow;
    }

    public void OnHoverExit()
    {
        // Restore original color when not hovered.
        if (meshRenderer != null) meshRenderer.material.color = originalColor;
    }

    public void OnInteract()
    {
        if (isAlreadyInteracted || associatedQuest == null) return;

        // Double-check prerequisites before allowing interaction.
        if (!ArePrerequisitesMet(associatedQuest))
        {
            Debug.Log($"[Quest] Locked. You must complete all prerequisite quests first.");
            return;
        }

        // Complete the quest via the Manager
        QuestManager.Instance.CompleteQuest(associatedQuest);
        isAlreadyInteracted = true;
        
        // Visual feedback for completion (e.g., turns green or hides the object)
        if (meshRenderer != null) meshRenderer.material.color = Color.green;
        OnHoverExit();
    }

    /// <summary>
    /// Helper method to check if all prerequisites in the array are completed.
    /// </summary>
    private bool ArePrerequisitesMet(QuestData quest)
    {
        if (quest.prerequisiteQuests == null || quest.prerequisiteQuests.Length == 0) return true;

        foreach (var prereq in quest.prerequisiteQuests)
        {
            if (prereq != null && !QuestManager.Instance.IsQuestCompleted(prereq))
            {
                return false;
            }
        }
        return true;
    }
}