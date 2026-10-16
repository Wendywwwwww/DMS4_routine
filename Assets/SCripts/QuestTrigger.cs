using UnityEngine;

/// <summary>
/// Automatically closes, locks designated doors, and removes their interaction scripts 
/// when the player enters the trigger zone. 
/// If a quest is assigned, it also completes the quest (supporting prerequisite checks).
/// If no quest is assigned, it acts purely as a door closure trigger.
/// </summary>
public class QuestTrigger : MonoBehaviour
{
    [Header("Quest Settings (Optional)")]
    [Tooltip("Optional: The quest that will be completed when the player enters this zone. Leave empty for door-only triggers.")]
    public QuestData questToComplete;

    [Header("Door Control")]
    [Tooltip("A list of ProximityDoors to automatically close, lock, and strip interaction scripts when this trigger fires.")]
    public ProximityDoor[] targetDoors; 
    
    // Prevents the trigger from firing multiple times once activated.
    private bool isTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        // Ensure the object entering the trigger zone is the Player.
        if (other.CompareTag("Player"))
        {
            // Case 1: A quest is assigned, so we need to check prerequisites first.
            if (questToComplete != null)
            {
                if (!ArePrerequisitesMet(questToComplete)) return; // Prerequisites not met, exit.

                // Complete the quest
                QuestManager.Instance.CompleteQuest(questToComplete);
            }
            // Case 2: No quest is assigned, so it acts purely as a door trigger (proceeds directly).

            // Execute door control logic for all assigned doors
            if (targetDoors != null && targetDoors.Length > 0)
            {
                foreach (var door in targetDoors)
                {
                    if (door != null)
                    {
                        door.CloseAndLockDoor();

                        // Completely destroy the ObjectInteractionPrompt component so it's no longer interactable
                        ObjectInteractionPrompt doorPrompt = door.GetComponent<ObjectInteractionPrompt>();
                        if (doorPrompt != null)
                        {
                            Destroy(doorPrompt);
                        }
                    }
                }
            }

            isTriggered = true; 
            
            if (questToComplete != null)
            {
                Debug.Log($"[QuestTrigger] Quest '{questToComplete.questID}' completed, {targetDoors.Length} door(s) closed and interaction scripts destroyed.");
            }
            else
            {
                Debug.Log($"[QuestTrigger] Pure door trigger fired: {targetDoors.Length} door(s) closed and interaction scripts destroyed.");
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