using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A strict sequential testing tool. Strictly follows the Inspector list order 
/// without auto-skipping or guessing. Press 'N' to complete them one by one.
/// </summary>
public class QuestTestController : MonoBehaviour
{
    [Header("Quests to Complete in Order")]
    [Tooltip("Drag your quests here in exact chronological order. Press 'N' to complete the current one.")]
    public List<QuestData> testQuests = new List<QuestData>();

    [Header("Display UI")]
    public bool showDebugWindow = true;

    private int currentIndex = 0;

    void Update()
    {
        // Press 'N' to complete the current index quest and move to the next
        if (Input.GetKeyDown(KeyCode.N))
        {
            CompleteCurrentIndexQuest();
        }
    }

    /// <summary>
    /// Strictly completes the quest at the current index, then increments the pointer.
    /// </summary>
    public void CompleteCurrentIndexQuest()
    {
        if (testQuests == null || testQuests.Count == 0) return;
        if (QuestManager.Instance == null) return;

        if (currentIndex < testQuests.Count)
        {
            QuestData targetQuest = testQuests[currentIndex];
            if (targetQuest != null)
            {
                QuestManager.Instance.CompleteQuest(targetQuest);
                
                string questDisplayName = !string.IsNullOrEmpty(targetQuest.questID) 
                    ? targetQuest.questID 
                    : targetQuest.name;
                    
                Debug.Log($"[Quest Test] Successfully completed quest [{currentIndex + 1}/{testQuests.Count}]: {questDisplayName}");
            }

            // Move to the next strict position
            currentIndex++;
        }
        else
        {
            Debug.Log("[Quest Test] Reached the end of the test quest list.");
        }
    }

    void OnGUI()
    {
        if (!showDebugWindow) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 100), GUI.skin.box);
        GUILayout.Label("<b>Strict Sequential Quest Tester</b>");

        if (testQuests != null && testQuests.Count > 0)
        {
            string currentQuestName = "Finished All";
            if (currentIndex < testQuests.Count && testQuests[currentIndex] != null)
            {
                currentQuestName = !string.IsNullOrEmpty(testQuests[currentIndex].questID) 
                    ? testQuests[currentIndex].questID 
                    : testQuests[currentIndex].name;
            }

            GUILayout.Label($"Progress: <b>{Mathf.Min(currentIndex + 1, testQuests.Count)} / {testQuests.Count}</b>");
            GUILayout.Label($"Current Target: <b>{currentQuestName}</b>");

            if (GUILayout.Button("Complete Current (N)"))
            {
                CompleteCurrentIndexQuest();
            }
        }
        else
        {
            GUILayout.Label("Assign quests in Inspector!");
        }

        GUILayout.EndArea();
    }
}