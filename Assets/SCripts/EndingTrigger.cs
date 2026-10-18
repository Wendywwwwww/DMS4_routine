using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Triggers an ending sequence when left-clicked on a Box Collider:
/// Auto-finds the correct UI image (starts hidden) -> Turns on -> Rapid Black/White screen flicker -> 
/// Disables specified objects -> Smooth fade to solid black and stays black.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EndingTrigger : MonoBehaviour
{
    [Header("UI Matching")]
    [Tooltip("Type a keyword found in your screen overlay image's name (e.g., 'Overlay', 'Fade', 'Black', or 'Panel').")]
    public string imageNameKeyword = "Overlay";

    [Header("Sequence Timings")]
    [Tooltip("How long the black and white flicker lasts before fading.")]
    public float flickerDuration = 1.5f;
    [Tooltip("How fast the flicker alternates between black and white.")]
    public float flickerSpeed = 0.08f;
    [Tooltip("How long the final fade to solid black takes.")]
    public float fadeToBlackDuration = 2.0f;

    [Header("Objects to Disable (e.g. Sounds, Player, UI)")]
    [Tooltip("Drag any objects you want to turn off right before the fade to black (e.g., ambient sound objects).")]
    public GameObject[] objectsToDisable;

    private Image screenOverlay;
    private bool hasTriggered = false;

    private void Start()
    {
        // Ensure the collider is set up
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        // Search all UI Images in the scene (including inactive ones) for the keyword
        Image[] foundImages = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Image img in foundImages)
        {
            if (img.gameObject.name.Contains(imageNameKeyword))
            {
                screenOverlay = img;
                break;
            }
        }

        // Fallback: if no name matched, grab the first image found as a backup
        if (screenOverlay == null && foundImages.Length > 0)
        {
            screenOverlay = foundImages[0];
            Debug.LogWarning($"[EndingTrigger] Couldn't find an image matching '{imageNameKeyword}', so defaulting to '{screenOverlay.name}'.");
        }

        if (screenOverlay != null)
        {
            // Turn it OFF at the start so it doesn't show a black square on your screen
            screenOverlay.gameObject.SetActive(false);
            Debug.Log($"[EndingTrigger] Successfully auto-linked UI Image: '{screenOverlay.name}'");
        }
        else
        {
            Debug.LogError("[EndingTrigger] Could not find any UI Image in the scene! Make sure you have a UI Canvas with an Image component.");
        }
    }

    private void OnMouseDown()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("[EndingTrigger] Object clicked! Initiating ending sequence.");
            StartCoroutine(EndingSequenceRoutine());
        }
    }

    private IEnumerator EndingSequenceRoutine()
    {
        if (screenOverlay == null) yield break;

        // Turn the black screen object ON right when the sequence starts
        screenOverlay.gameObject.SetActive(true);
        float timer = 0f;

        // 1. Black and White Flicker Phase
        while (timer < flickerDuration)
        {
            timer += flickerSpeed;
            screenOverlay.color = Random.value > 0.5f ? Color.white : Color.black;
            yield return new WaitForSeconds(flickerSpeed);
        }

        // 2. Disable specified objects right before fading out
        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // 3. Fade to Solid Black Phase
        float fadeTimer = 0f;
        Color startColor = screenOverlay.color;
        Color targetColor = Color.black;
        targetColor.a = 1f;

        while (fadeTimer < fadeToBlackDuration)
        {
            fadeTimer += Time.deltaTime;

            screenOverlay.color = Color.Lerp(startColor, targetColor, fadeTimer / fadeToBlackDuration);
            yield return null;
        }

        screenOverlay.color = targetColor;
        Debug.Log("[EndingTrigger] Ending sequence complete. Screen is now solid black.");
    }
}