using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro;

/// <summary>
/// Listens for a specific prerequisite quest to complete, then plays a typewriter effect 
/// after a delay and transitions to the next scene.
/// </summary>
public class SceneTransitionText : MonoBehaviour
{
    [Header("Quest Trigger Setup")]
    [Tooltip("Drag the prerequisite Quest Data here. When this quest finishes, the ending sequence starts.")]
    public QuestData prerequisiteQuest;

    [Header("UI Settings")]
    [Tooltip("The TextMeshProUGUI component used to display the typing effect.")]
    public TextMeshProUGUI uiText;
    
    [Tooltip("The text content to display.")]
    [TextArea(3, 5)]
    public string textToDisplay;

    [Header("Timing Settings")]
    [Tooltip("How many seconds to wait after the quest is completed before typing starts.")]
    public float delayBeforeTyping = 3.0f;
    
    [Tooltip("Time delay between each character appearing (typing speed).")]
    public float typingSpeed = 0.05f;
    
    [Tooltip("Delay in seconds after typing finishes before transitioning to the next scene.")]
    public float delayAfterTyping = 1.5f;

    [Header("Scene Settings")]
    [Tooltip("The name of the next scene (must be added in File -> Build Settings).")]
    public string nextSceneName;

    void Start()
    {
        // Clear the text at the start of the scene
        if (uiText != null) uiText.text = "";

        // Subscribe to the quest manager to listen for completion events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
        else
        {
            Debug.LogWarning("[SceneTransitionText] QuestManager Instance is missing in the scene!");
        }
    }

    void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void HandleQuestCompleted(QuestData completedQuest)
    {
        if (prerequisiteQuest == null || completedQuest == null) return;

        // Check if the completed quest matches our prerequisite slot
        if (completedQuest.questID == prerequisiteQuest.questID)
        {
            Debug.Log($"[SceneTransitionText] Prerequisite quest '{prerequisiteQuest.questID}' met. Starting transition sequence.");
            StartCoroutine(PlayTextAndTransition());
        }
    }

    private IEnumerator PlayTextAndTransition()
    {
        // 1. Wait for the specified delay after the quest finishes
        yield return new WaitForSeconds(delayBeforeTyping);

        // 2. Execute the typewriter effect character by character
        for (int i = 0; i < textToDisplay.Length; i++)
        {
            uiText.text += textToDisplay[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        // 3. Wait for a moment after typing finishes
        yield return new WaitForSeconds(delayAfterTyping);

        // 4. Transition to the next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[SceneTransitionText] Next Scene Name is empty! Cannot transition.");
        }
    }
}