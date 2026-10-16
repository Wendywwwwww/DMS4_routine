using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles Scene 2 spawn-in: starts with a white screen fade in, locks player movement until complete,
/// and restores the persistent Quest UI smoothly.
/// </summary>
public class SceneSpawnFadeInController : MonoBehaviour
{
    [Header("White Fade Screen Settings")]
    [Tooltip("A full-screen white UI Image used for the white fade in effect.")]
    public Image whiteFadeImage;
    [Tooltip("Duration for the white screen to fade out (revealing the scene).")]
    public float whiteFadeDuration = 3f;

    [Header("Quest UI Settings (Optional: will auto-find if empty)")]
    [Tooltip("The CanvasGroup of your Quest UI that should reappear.")]
    public CanvasGroup questUICanvasGroup;
    public float uiFadeDuration = 2f;

    [Header("Player Movement Lock")]
    [Tooltip("Drag your player's movement script or character controller component here to lock movement during fade.")]
    public MonoBehaviour playerMovementScript; 

    [Header("Audio (Optional)")]
    public AudioSource spawnAudioSource;
    public AudioClip spawnClip;

    void Start()
    {
        // 1. Lock player movement immediately at spawn
        SetPlayerMovementEnabled(false);

        // 2. Initialize White Fade Image to fully opaque white (Alpha = 1)
        if (whiteFadeImage != null)
        {
            Color c = whiteFadeImage.color;
            c.a = 1f;
            whiteFadeImage.color = c;
            whiteFadeImage.gameObject.SetActive(true);
        }

        // 3. Automatically find the Quest UI if it wasn't assigned in the inspector using FindFirstObjectByType
        if (questUICanvasGroup == null)
        {
            QuestFogTransitionController transitionController = Object.FindFirstObjectByType<QuestFogTransitionController>();
            if (transitionController != null && transitionController.canvasGroupToFade != null)
            {
                questUICanvasGroup = transitionController.canvasGroupToFade;
            }
            else
            {
                questUICanvasGroup = Object.FindFirstObjectByType<CanvasGroup>();
            }
        }

        // 4. Initialize Quest UI as active and ready to fade in (keeping previous progress records)
        if (questUICanvasGroup != null)
        {
            questUICanvasGroup.gameObject.SetActive(true);
            questUICanvasGroup.alpha = 0f; // Start hidden, will fade in
        }

        // Play spawn sound if assigned
        if (spawnAudioSource != null && spawnClip != null)
        {
            spawnAudioSource.PlayOneShot(spawnClip);
        }

        // Start the spawn reveal coroutine
        StartCoroutine(SpawnFadeInRoutine());
    }

    private IEnumerator SpawnFadeInRoutine()
    {
        float elapsedTime = 0f;
        float maxDuration = Mathf.Max(whiteFadeDuration, uiFadeDuration);

        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;

            // 1. White screen fades out (Alpha 1 -> 0), revealing the scene slowly
            if (whiteFadeImage != null && elapsedTime <= whiteFadeDuration)
            {
                float t = elapsedTime / whiteFadeDuration;
                Color c = whiteFadeImage.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                whiteFadeImage.color = c;
            }

            // 2. Quest UI fades in (Alpha 0 -> 1)
            if (questUICanvasGroup != null && elapsedTime <= uiFadeDuration)
            {
                float t = elapsedTime / uiFadeDuration;
                questUICanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        // Ensure final clean states
        if (whiteFadeImage != null)
        {
            Color c = whiteFadeImage.color;
            c.a = 0f;
            whiteFadeImage.color = c;
            whiteFadeImage.gameObject.SetActive(false); // Disable white screen object when done
        }

        if (questUICanvasGroup != null)
        {
            questUICanvasGroup.alpha = 1f;
        }

        // 3. Unlock player movement after fade is complete
        SetPlayerMovementEnabled(true);

        Debug.Log("[SceneSpawn] Scene 2 spawn fade-in complete. Player movement unlocked, Quest UI restored.");
    }

    private void SetPlayerMovementEnabled(bool isEnabled)
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = isEnabled;
        }
    }
}