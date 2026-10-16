using UnityEngine;

/// <summary>
/// Completes a specific quest automatically when the player enters a defined trigger zone,
/// and optionally closes/locks a designated door and completely removes its interaction script.
/// Supports multiple prerequisite quests.
/// </summary>
public class QuestTrigger : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("The quest that will be completed when the player enters this zone.")]
    public QuestData questToComplete;

    [Header("Optional Door Control")]
    [Tooltip("Optional: A ProximityDoor to automatically close and lock when this quest trigger fires.")]
    public ProximityDoor targetDoor;
    
    // Prevents the trigger from firing multiple times once the quest is completed.
    private bool isTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered || questToComplete == null) return;

        // Ensure the object entering the trigger zone is the Player.
        if (other.CompareTag("Player"))
        {
            // Check if all prerequisites are met (ensures it only fires during the correct quest stage).
            if (ArePrerequisitesMet(questToComplete))
            {
                // 1. Complete the quest
                QuestManager.Instance.CompleteQuest(questToComplete);

                // 2. Automatically close, lock the door and completely remove its interaction prompt script
                if (targetDoor != null)
                {
                    targetDoor.CloseAndLockDoor();

                    // Completely destroy the ObjectInteractionPrompt component so it's no longer interactable
                    ObjectInteractionPrompt doorPrompt = targetDoor.GetComponent<ObjectInteractionPrompt>();
                    if (doorPrompt != null)
                    {
                        Destroy(doorPrompt);
                    }
                }

                isTriggered = true; 
                Debug.Log($"[QuestTrigger] Quest '{questToComplete.questID}' completed, door closed and interaction script destroyed.");
            }
        }
    }

    /// <summary>
    /// Checks if all prerequisite quests in the array have been completed.
    /// </summary>
    private bool ArePrerequisitesMet(QuestData quest)
    {
        if (quest.prerequisiteQuests == null || quest.prerequisiteQuests.Length == 0) return true;

        foreach (var prereq in quest.prerequisiteQuests)
        {
            if (prereq != null && !QuestManager.Instance.IsQuestCompleted(prereq))
            {
                return false; // Found an uncompleted prerequisite
            }
        }
        return true;
    }
}