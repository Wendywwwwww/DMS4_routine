using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A testing utility script that allows developers to skip forward or backward 
/// through a predefined list of quests during play mode.
/// </summary>
public class QuestTestController : MonoBehaviour
{
    [Header("Test Quests Setup")]
    [Tooltip("Drag your sequence of quests here in chronological order (Index 0 is Quest 1).")]
    public List<QuestData> testQuestSequence = new List<QuestData>();

    [Header("UI Display Settings")]
    [Tooltip("Toggle to show or hide the on-screen debug control panel.")]
    public bool showDebugWindow = true;

    private int currentIndex = 0;

    void Update()
    {
        // Quick keyboard shortcuts for testing (N = Next Quest, P = Previous Quest)
        if (Input.GetKeyDown(KeyCode.N))
        {
            AdvanceToNextQuest();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            RevertToPreviousQuest();
        }
    }

    /// <summary>
    /// Instantly completes the current quest and moves to the next one in the sequence.
    /// </summary>
    public void AdvanceToNextQuest()
    {
        if (testQuestSequence == null || testQuestSequence.Count == 0) return;

        if (currentIndex < testQuestSequence.Count)
        {
            QuestData currentQuest = testQuestSequence[currentIndex];
            if (currentQuest != null && QuestManager.Instance != null)
            {
                // Force complete the current quest using the QuestManager
                QuestManager.Instance.CompleteQuest(currentQuest);
                Debug.Log($"[QuestTest] Skipped/Completed Quest [{currentIndex + 1}]: {currentQuest.questID}");
            }

            currentIndex++;
        }
        else
        {
            Debug.Log("[QuestTest] Already at the end of the quest sequence.");
        }
    }

    /// <summary>
    /// Resets or loops back (Note: QuestManager doesn't natively un-complete quests, 
    /// but this shifts the test tracker pointer).
    /// </summary>
    public void RevertToPreviousQuest()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            Debug.Log($"[QuestTest] Reverted test pointer to Quest [{currentIndex + 1}]: {testQuestSequence[currentIndex]?.questID}");
        }
    }

    // Draw an on-screen IMGUI debug window for easy testing
    void OnGUI()
    {
        if (!showDebugWindow) return;

        // Create a small window at the top-left corner of the screen
        GUILayout.BeginArea(new Rect(10, 10, 300, 120), GUI.skin.box);
        GUILayout.Label("<b>--- Quest Test Controller ---</b>");

        if (testQuestSequence != null && testQuestSequence.Count > 0)
        {
            string currentQuestName = (currentIndex < testQuestSequence.Count && testQuestSequence[currentIndex] != null) 
                ? testQuestSequence[currentIndex].questID 
                : "None / Finished";

            GUILayout.Label($"Current Quest Index: <b>{currentIndex + 1} / {testQuestSequence.Count}</b>");
            GUILayout.Label($"Active Quest ID: <b>{currentQuestName}</b>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<< Prev (P)"))
            {
                RevertToPreviousQuest();
            }
            if (GUILayout.Button("Next (N) >>"))
            {
                AdvanceToNextQuest();
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("No test quests assigned in the list!");
        }

        GUILayout.EndArea();
    }
}