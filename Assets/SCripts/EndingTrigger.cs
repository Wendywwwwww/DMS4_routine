using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Triggers an ending sequence when left-clicked on a Box Collider:
/// Rapid Black/White screen flicker -> Smooth fade to solid black -> Scene transition.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EndingTrigger : MonoBehaviour
{
    [Header("UI Screen Overlay")]
    [Tooltip("Drag a full-screen UI Image here (used for the flicker and fade to black).")]
    public Image screenOverlay;

    [Header("Sequence Timings")]
    [Tooltip("How long the black and white flicker lasts before fading.")]
    public float flickerDuration = 1.5f;
    [Tooltip("How fast the flicker alternates between black and white.")]
    public float flickerSpeed = 0.08f;
    [Tooltip("How long the final fade to solid black takes.")]
    public float fadeToBlackDuration = 2.0f;

    [Header("Scene Transition")]
    [Tooltip("Name of the credits or ending scene to load when complete.")]
    public string endingSceneName = "CreditsScene";

    private bool hasTriggered = false;

    private void Start()
    {
        // Ensure the collider is set up
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false; // Set to false so raycasts/clicks can hit it easily
        }

        // Hide and reset the overlay at start
        if (screenOverlay != null)
        {
            Color c = screenOverlay.color;
            c.a = 0f;
            screenOverlay.color = c;
            screenOverlay.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[EndingTrigger] Screen Overlay Image is not assigned!");
        }
    }

    /// <summary>
    /// Unity's built-in method that fires when you left-click on this object's collider.
    /// </summary>
    private void OnMouseDown()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("[EndingTrigger] Object clicked! Initiating ending sequence.");
            StartCoroutine(EndingSequenceRoutine());
        }
    }

    /// <summary>
    /// Coroutine handling the flicker, fade to black, and scene load.
    /// </summary>
    private IEnumerator EndingSequenceRoutine()
    {
        if (screenOverlay == null) yield break;

        screenOverlay.gameObject.SetActive(true);
        float timer = 0f;

        // 1. Black and White Flicker Phase
        while (timer < flickerDuration)
        {
            timer += flickerSpeed;

            // Randomly flash between pure white and pure black at full opacity
            screenOverlay.color = Random.value > 0.5f ? Color.white : Color.black;

            yield return new WaitForSeconds(flickerSpeed);
        }

        // 2. Fade to Solid Black Phase
        float fadeTimer = 0f;
        Color startColor = screenOverlay.color;
        Color targetColor = Color.black;
        targetColor.a = 1f; // Solid black

        while (fadeTimer < fadeToBlackDuration)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / fadeToBlackDuration;

            screenOverlay.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }

        // Ensure it's 100% solid black at the end
        screenOverlay.color = targetColor;

        // 3. Load Ending / Credits Scene
        Debug.Log($"[EndingTrigger] Loading ending scene: {endingSceneName}");
        SceneManager.LoadScene(endingSceneName);
    }
}