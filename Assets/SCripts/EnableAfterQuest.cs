using UnityEngine;

/// <summary>
/// Controls and enables a list of target scripts only after a specified prerequisite quest is completed.
/// Subscribes to the QuestManager event for instant triggering.
/// </summary>
public class EnableAfterQuest : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("The QuestData asset of the prerequisite quest.")]
    public QuestData requiredQuest;

    [Header("Target Scripts to Control")]
    [Tooltip("Drag the components/scripts you want to enable after the quest is done.")]
    public MonoBehaviour[] scriptsToEnable; 

    private bool isActivated = false;

    void Start()
    {
        // 1. Check if the quest was already completed before this object loaded
        CheckInitialQuestState();
    }

    void OnEnable()
    {
        // 2. Subscribe to the QuestManager completion event for instant response
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    void OnDisable()
    {
        // 3. Unsubscribe from the event to prevent memory leaks
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    /// <summary>
    /// Checks the quest state upon startup in case the quest was completed earlier.
    /// </summary>
    void CheckInitialQuestState()
    {
        if (isActivated || requiredQuest == null) return;

        // Use the singleton instance to call IsQuestCompleted correctly
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted(requiredQuest))
        {
            ActivateTargetScripts();
        }
    }

    /// <summary>
    /// Event listener triggered whenever any quest is completed in the game.
    /// </summary>
    void HandleQuestCompleted(QuestData completedQuest)
    {
        if (isActivated) return;

        // Check if the completed quest matches our required prerequisite
        if (completedQuest == requiredQuest)
        {
            ActivateTargetScripts();
        }
    }

    /// <summary>
    /// Enables all assigned target scripts.
    /// </summary>
    void ActivateTargetScripts()
    {
        if (isActivated) return;

        foreach (var script in scriptsToEnable)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }

        isActivated = true;
        Debug.Log($"[EnableAfterQuest] Prerequisite quest '{requiredQuest.questID}' is completed. Target scripts have been enabled.");
    }
}