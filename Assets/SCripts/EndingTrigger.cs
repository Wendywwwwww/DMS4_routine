using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EndingTrigger : MonoBehaviour
{
    [Header("UI Lookup by Name (For Prefabs)")]
    [Tooltip("Type the exact name of your black screen UI Image GameObject in your scene hierarchy here.")]
    public string uiImageObjectName = "BlackScreenPanel";

    [Header("Timings")]
    public float flickerDuration = 1.5f;
    public float flickerSpeed = 0.08f;
    public float fadeToBlackDuration = 2.0f;

    [Header("Objects to Disable (e.g., Sounds, UI)")]
    public GameObject[] objectsToDisable;

    private Image screenOverlay;
    private bool hasTriggered = false;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        // Automatically find the UI image by name at runtime (works even if it's inactive/hidden)
        Image[] allImages = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var img in allImages)
        {
            if (img.gameObject.name == uiImageObjectName)
            {
                screenOverlay = img;
                break;
            }
        }

        if (screenOverlay != null)
        {
            screenOverlay.gameObject.SetActive(false); // Hide it at start
        }
        else
        {
            Debug.LogError($"[EndingTrigger] Could not find any UI Image named '{uiImageObjectName}' in the scene!");
        }
    }

    private void OnMouseDown()
    {
        if (!hasTriggered && screenOverlay != null)
        {
            hasTriggered = true;
            StartCoroutine(EndingSequenceRoutine());
        }
    }

    private IEnumerator EndingSequenceRoutine()
    {
        screenOverlay.gameObject.SetActive(true);
        float timer = 0f;

        // 1. Black and White Flicker Phase
        while (timer < flickerDuration)
        {
            timer += flickerSpeed;
            screenOverlay.color = Random.value > 0.5f ? Color.white : Color.black;
            yield return new WaitForSeconds(flickerSpeed);
        }

        // 2. Disable specified objects (sounds, player, etc.)
        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // 3. Smooth Fade to Solid Black Phase
        float fadeTimer = 0f;
        Color startColor = screenOverlay.color;
        Color targetColor = Color.black;

        while (fadeTimer < fadeToBlackDuration)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / fadeToBlackDuration;

            screenOverlay.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }

        screenOverlay.color = targetColor;
        Debug.Log("[EndingTrigger] Sequence finished. Screen is solid black.");
    }
}