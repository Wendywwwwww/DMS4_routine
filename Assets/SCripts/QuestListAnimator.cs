using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Animates the UI panel to shrink and move to the top-left corner after an initial delay.
/// Handles multiple TextMeshProUGUI items (e.g., a list of multiple quests) and switches their alignment.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class QuestListAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Time in seconds the UI stays in the center before animating.")]
    public float displayTime = 2f;
    
    [Tooltip("Duration of the shrink and move animation in seconds.")]
    public float animationDuration = 1f;

    [Header("Target State (Top-Left Minimized)")]
    [Tooltip("An empty UI object placed at the top-left corner where the UI should move to.")]
    public RectTransform minimizedTargetMarker;
    
    [Tooltip("The final scale size (e.g., 0.5 means half the original size).")]
    public float targetScale = 0.5f;

    [Header("Text Alignment Settings (Multiple Texts Supported)")]
    [Tooltip("Drag all your TextMeshProUGUI components here (or leave empty to automatically find all child texts).")]
    public List<TextMeshProUGUI> questTexts = new List<TextMeshProUGUI>();

    [Header("Player Reference")]
    [Tooltip("Drag your Player object (the one with the PlayerInput component) here.")]
    public PlayerInput playerInput;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // If no texts are manually assigned, automatically fetch all TextMeshProUGUI components in children
        if (questTexts.Count == 0)
        {
            questTexts.AddRange(GetComponentsInChildren<TextMeshProUGUI>());
        }

        // Set all texts to Center alignment at start
        SetAllTextAlignment(isLeftAligned: false);
        
        // Automatically start the animation sequence when the panel becomes active.
        if (minimizedTargetMarker != null)
        {
            StartCoroutine(AnimateUISequence());
        }
        else
        {
            Debug.LogWarning("[QuestListAnimator] Missing Top-Left Target Marker. Animation aborted.");
        }
    }

    /// <summary>
    /// Coroutine that handles player movement lock, delayed display, smooth transition (Lerp), and alignment change.
    /// </summary>
    private IEnumerator AnimateUISequence()
    {
        // 1. Disable player movement input at the very beginning, keeping look input active
        SetPlayerMovementEnabled(false);

        // 2. Wait for the initial display time in the center of the screen
        yield return new WaitForSeconds(displayTime);

        // Switch all text alignments to Left when animating to the corner
        SetAllTextAlignment(isLeftAligned: true);

        // 3. Record starting values and calculate ending values
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;
        
        Vector2 endPos = minimizedTargetMarker.anchoredPosition;
        Vector3 endScale = new Vector3(targetScale, targetScale, 1f);

        float elapsedTime = 0f;

        // 4. Smoothly interpolate (Lerp) position and scale frame by frame
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Use SmoothStep for a natural easing effect (slow start, fast middle, slow end)
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothT);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT);

            yield return null; // Wait for the next frame
        }

        // 5. Ensure exact final values to prevent floating-point inaccuracies
        rectTransform.anchoredPosition = endPos;
        rectTransform.localScale = endScale;

        // 6. Re-enable player movement input once the animation is fully completed
        SetPlayerMovementEnabled(true);
    }

    /// <summary>
    /// Switches alignment for all managed TextMeshProUGUI components between Center and Left.
    /// </summary>
    /// <param name="isLeftAligned">If true, sets alignment to Left; otherwise sets to Center.</param>
    private void SetAllTextAlignment(bool isLeftAligned)
    {
        TextAlignmentOptions targetAlignment = isLeftAligned ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;

        foreach (var text in questTexts)
        {
            if (text != null)
            {
                text.alignment = targetAlignment;
            }
        }
    }

    /// <summary>
    /// Enables or disables the 'Move' action in PlayerInput while keeping 'Look' active.
    /// </summary>
    /// <param name="isEnabled">True to allow movement, false to lock.</param>
    private void SetPlayerMovementEnabled(bool isEnabled)
    {
        if (playerInput != null && playerInput.actions != null)
        {
            var moveAction = playerInput.actions["Move"];
            if (moveAction != null)
            {
                if (isEnabled)
                    moveAction.Enable();
                else
                    moveAction.Disable();
            }
            else
            {
                Debug.LogWarning("[QuestListAnimator] Could not find 'Move' action in PlayerInput actions.");
            }
        }
        else
        {
            Debug.LogWarning("[QuestListAnimator] PlayerInput reference is not assigned!");
        }
    }
}