using UnityEngine;
using UnityEngine.UI;
using TMPro; 

/// <summary>
/// Listens to the QuestManager, dims the text, and programmatically generates 
/// a precise red strikethrough line matching the exact rendered text width.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class QuestUIItem : MonoBehaviour
{
    [Tooltip("The quest data this UI text represents.")]
    public QuestData associatedQuest;

    private TextMeshProUGUI questText;
    private GameObject strikeThroughLine; // Dynamically generated strikethrough line object

    void Start()
    {
        questText = GetComponent<TextMeshProUGUI>();
        
        if (associatedQuest != null)
        {
            questText.text = associatedQuest.description;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    /// <label>
    /// Handles the quest completion event, dims the text, and triggers the strikethrough effect.
    /// </summary>
    private void HandleQuestCompleted(QuestData completedQuest)
    {
        if (completedQuest == associatedQuest)
        {
            // 1. Keep clean text and dim its color
            questText.text = associatedQuest.description;
            questText.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);

            // 2. Dynamically generate a precise red strikethrough line matching the actual text width
            CreateStrikeThroughLine();
        }
    }

    /// <summary>
    /// Dynamically creates a UI image line that matches the exact preferred width of the text content,
    /// aligned to the left and vertically centered.
    /// </summary>
    private void CreateStrikeThroughLine()
    {
        if (strikeThroughLine != null) return;

        // Create a child object for the line
        strikeThroughLine = new GameObject("StrikeThroughLine");
        strikeThroughLine.transform.SetParent(transform, false);

        // Add UI Image component
        Image lineImage = strikeThroughLine.AddComponent<Image>();
        lineImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Distinct red color for the strikethrough

        // Get the RectTransform of the line
        RectTransform lineRect = strikeThroughLine.GetComponent<RectTransform>();

        // Set anchors and pivot to Left-Middle (0, 0.5)
        lineRect.anchorMin = new Vector2(0f, 0.5f);
        lineRect.anchorMax = new Vector2(0f, 0.5f);
        lineRect.pivot = new Vector2(0f, 0.5f);

        // Force TMP to update its geometry and calculate the exact width of the actual text string
        questText.ForceMeshUpdate();
        float actualTextWidth = questText.preferredWidth;

        // Set line size: exact width of the actual text, 3 pixels height (thickness)
        lineRect.sizeDelta = new Vector2(actualTextWidth, 3f);

        // Align it perfectly centered vertically with a zero horizontal offset from the left
        lineRect.anchoredPosition = new Vector2(0f, 0f);
    }
}