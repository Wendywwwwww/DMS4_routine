using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for Image
using TMPro;

/// <summary>
/// A static version of the quest list display for non-animated scenes.
/// Directly places the UI in the top-left corner and safely crosses out selected quests
/// using the exact UI Image line logic from QuestUIItem to prevent formatting conflicts.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class StaticQuestListDisplay : MonoBehaviour
{
    [Header("Target State (Top-Left Minimized)")]
    [Tooltip("An empty UI object placed at the top-left corner where the UI should move to.")]
    public RectTransform minimizedTargetMarker;
    public float targetScale = 0.5f;

    [Header("Quest Texts Settings")]
    [Tooltip("Drag all your TextMeshProUGUI components here (or leave empty to automatically find all child texts).")]
    public List<TextMeshProUGUI> allQuestTexts = new List<TextMeshProUGUI>();
    
    [Tooltip("Drag the TextMeshProUGUI components you want crossed out initially here.")]
    public List<TextMeshProUGUI> completedQuests = new List<TextMeshProUGUI>();

    private RectTransform rectTransform;

    private IEnumerator Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (allQuestTexts.Count == 0)
        {
            allQuestTexts.AddRange(GetComponentsInChildren<TextMeshProUGUI>());
        }

        // 1. Instantly snap to the top-left corner without animation
        if (minimizedTargetMarker != null)
        {
            rectTransform.anchoredPosition = minimizedTargetMarker.anchoredPosition;
            rectTransform.localScale = new Vector3(targetScale, targetScale, 1f);
        }

        // 2. Align all texts to Left
        foreach (var text in allQuestTexts)
        {
            if (text != null)
            {
                text.alignment = TextAlignmentOptions.Left;
            }
        }

        // 3. Wait one frame to ensure QuestUIItem's Start() sets the initial text
        yield return new WaitForEndOfFrame();

        // 4. Safely apply the physical Image strikethrough to chosen quests
        foreach (var questText in completedQuests)
        {
            if (questText != null)
            {
                ApplyPhysicalStrikeThrough(questText);
            }
        }
    }

    /// <summary>
    /// Replicates the exact strikethrough logic from QuestUIItem.
    /// Triggers a physical Image line and dims the color.
    /// </summary>
    private void ApplyPhysicalStrikeThrough(TextMeshProUGUI questText)
    {
        // Match the dim color from QuestUIItem
        questText.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);

        // Check if the line was already created by QuestManager to avoid duplicates
        if (questText.transform.Find("StrikeThroughLine") != null) return;

        // Create a child object for the line
        GameObject strikeThroughLine = new GameObject("StrikeThroughLine");
        strikeThroughLine.transform.SetParent(questText.transform, false);

        // Add UI Image component
        Image lineImage = strikeThroughLine.AddComponent<Image>();
        lineImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);

        // Get the RectTransform of the line
        RectTransform lineRect = strikeThroughLine.GetComponent<RectTransform>();

        // Set anchors and pivot to Left-Middle (0, 0.5)
        lineRect.anchorMin = new Vector2(0f, 0.5f);
        lineRect.anchorMax = new Vector2(0f, 0.5f);
        lineRect.pivot = new Vector2(0f, 0.5f);

        // Force TMP to update its geometry and calculate the exact width
        questText.ForceMeshUpdate();
        float actualTextWidth = questText.preferredWidth;

        // Set line size: exact width of the actual text, 3 pixels height (thickness)
        lineRect.sizeDelta = new Vector2(actualTextWidth, 3f);

        // Align it perfectly centered vertically with a zero horizontal offset from the left
        lineRect.anchoredPosition = new Vector2(0f, 0f);
    }
}