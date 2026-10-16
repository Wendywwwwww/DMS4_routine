using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Singleton manager that tracks quest states and broadcasts completion events.
/// Decouples the logic so UI and objects don't need to check items directly.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    /// <summary>
    /// Event triggered whenever a quest is completed.
    /// Other scripts (like UI or QuestAudioPlayer) can subscribe to this event.
    /// </summary>
    public event Action<QuestData> OnQuestCompleted;

    // Stores the IDs of all completed quests to prevent repetition.
    private HashSet<string> completedQuestIDs = new HashSet<string>();

    void Awake()
    {
        // Enforce Singleton pattern: Ensure only one QuestManager exists in the scene.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Marks a quest as complete and notifies all listeners.
    /// </summary>
    public void CompleteQuest(QuestData quest)
    {
        if (quest == null || completedQuestIDs.Contains(quest.questID)) return;

        completedQuestIDs.Add(quest.questID);
        Debug.Log($"[QuestManager] Quest Completed: {quest.questID}");

        // Broadcast the event to any subscribed listeners (UI, Audio Players, Doors, etc.)
        OnQuestCompleted?.Invoke(quest);
    }

    /// <summary>
    /// Checks if a specific quest has been completed.
    /// </summary>
    public bool IsQuestCompleted(QuestData quest)
    {
        if (quest == null) return true;
        return completedQuestIDs.Contains(quest.questID);
    }

    /// <summary>
    /// Checks if a specific quest is currently active.
    /// For multi-prerequisite quests, all listed prerequisites must be completed first.
    /// </summary>
    public bool IsQuestActive(QuestData quest)
    {
        if (quest == null) return false;

        // If it's already completed, it's no longer active
        if (IsQuestCompleted(quest)) return false;

        // Check all prerequisites in the array
        if (quest.prerequisiteQuests != null && quest.prerequisiteQuests.Length > 0)
        {
            foreach (var prereq in quest.prerequisiteQuests)
            {
                if (prereq != null && !IsQuestCompleted(prereq))
                {
                    // If even one prerequisite is not completed, this quest is not active yet
                    return false;
                }
            }
        }

        // If all prerequisites are cleared (or there are none), it's active
        return true;
    }
}