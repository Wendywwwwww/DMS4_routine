using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controls a quest-completion transition with scene persistence, 
/// and checks initial quest states to prevent re-triggering completed quests in new scenes.
/// </summary>
public class QuestFogTransitionController : MonoBehaviour
{
    [Header("Persistence")]
    [Tooltip("Check this if you want this controller and its UI to persist into the next scene.")]
    public bool persistAcrossScenes = true;

    [Header("Quest Selection")]
    [Tooltip("Drag the specific QuestData ScriptableObject asset that triggers this transition when completed.")]
    public QuestData targetQuest;

    [Header("Transition Target Settings")]
    [Tooltip("The name of the scene you want to transition to.")]
    public string targetSceneName = "Scene1_5";

    [Header("Canvas Fade Out Settings (Initially Hidden)")]
    public CanvasGroup canvasGroupToFade;

    [Header("3D Plane Independent Fade Settings")]
    [Tooltip("Drag the MeshRenderer of the 3D Plane that needs to fade in.")]
    public MeshRenderer fadePlaneRenderer;
    private Material planeMaterial;
    public float meshFadeDuration = 2f; 
    public float meshFadeDelay = 0.5f;

    [Header("Light Dimming Settings")]
    [Tooltip("Drag the Light that you want to turn off gradually.")]
    public Light targetLight;
    public float lightFadeDuration = 4f;
    private float initialLightIntensity;

    [Header("Overall & Fog Transition Duration")]
    public float fogTransitionDuration = 6f;

    [Header("Final White Flash Settings (Before Scene Load)")]
    public float finalWhiteFlashDuration = 1.0f;

    [Header("HDRP Fog Settings")]
    public Volume globalVolume;          
    private Fog hdrpFog;
    
    public float startMeanFreePath = 100f;
    public float targetMeanFreePath = 0.1f;

    [Header("Audio Settings")]
    public AudioSource transitionAudioSource;
    public AudioClip transitionClip;

    private bool isTriggered = false;

    void Awake()
    {
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet<Fog>(out hdrpFog))
        {
            hdrpFog.enabled.Override(false); 
        }

        if (canvasGroupToFade != null)
        {
            canvasGroupToFade.alpha = 0f;
            canvasGroupToFade.gameObject.SetActive(false);
        }

        if (fadePlaneRenderer != null)
        {
            planeMaterial = fadePlaneRenderer.material;
            SetPlaneAlpha(0f);
            fadePlaneRenderer.gameObject.SetActive(false);
        }

        if (targetLight != null)
        {
            initialLightIntensity = targetLight.intensity;
        }

        // Check if the quest was already completed BEFORE this scene started
        CheckInitialQuestState();
    }

    void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    /// <summary>
    /// Checks if the target quest was already completed in a previous scene.
    /// If so, marks it as triggered to prevent infinite loops or accidental re-triggering.
    /// </summary>
    private void CheckInitialQuestState()
    {
        if (QuestManager.Instance != null && targetQuest != null)
        {
            if (QuestManager.Instance.IsQuestCompleted(targetQuest))
            {
                Debug.Log($"[QuestTransition] Target quest '{targetQuest.questID}' was already completed previously. Disabling this transition controller.");
                isTriggered = true; // Prevents it from triggering again
            }
        }
    }

    private void HandleQuestCompleted(QuestData completedQuest)
    {
        if (isTriggered || targetQuest == null || completedQuest == null) return;

        if (completedQuest == targetQuest)
        {
            Debug.Log($"[QuestTransition] Target quest '{targetQuest.questID}' completed! Starting transition sequence...");
            TriggerQuestTransition();
        }
    }

    public void TriggerQuestTransition()
    {
        if (isTriggered) return;
        isTriggered = true;

        StartCoroutine(ExecuteTransitionRoutine());
    }

    private IEnumerator ExecuteTransitionRoutine()
    {
        if (transitionAudioSource != null && transitionClip != null)
        {
            transitionAudioSource.PlayOneShot(transitionClip);
        }

        if (hdrpFog != null)
        {
            hdrpFog.enabled.Override(true);
            hdrpFog.meanFreePath.Override(startMeanFreePath);
        }

        if (canvasGroupToFade != null)
        {
            canvasGroupToFade.gameObject.SetActive(true);
            canvasGroupToFade.alpha = 1f;
        }

        if (fadePlaneRenderer != null)
        {
            fadePlaneRenderer.gameObject.SetActive(true);
        }

        float elapsedTime = 0f;

        while (elapsedTime < fogTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float fogProgress = elapsedTime / fogTransitionDuration;

            if (canvasGroupToFade != null)
            {
                canvasGroupToFade.alpha = Mathf.Lerp(1f, 0f, fogProgress);
            }

            if (hdrpFog != null)
            {
                float currentPath = Mathf.Lerp(startMeanFreePath, targetMeanFreePath, fogProgress);
                hdrpFog.meanFreePath.Override(currentPath);
            }

            if (fadePlaneRenderer != null)
            {
                float meshElapsedTime = elapsedTime - meshFadeDelay;
                if (meshElapsedTime >= 0f && meshFadeDuration > 0f)
                {
                    float meshProgress = Mathf.Clamp01(meshElapsedTime / meshFadeDuration);
                    SetPlaneAlpha(meshProgress);
                }
            }

            if (targetLight != null)
            {
                float lightProgress = Mathf.Clamp01(elapsedTime / lightFadeDuration);
                targetLight.intensity = Mathf.Lerp(initialLightIntensity, 0f, lightProgress);
            }

            yield return null;
        }

        if (canvasGroupToFade != null)
        {
            canvasGroupToFade.alpha = 0f;
            canvasGroupToFade.gameObject.SetActive(false);
        }
        SetPlaneAlpha(1f);
        if (hdrpFog != null)
        {
            hdrpFog.meanFreePath.Override(targetMeanFreePath);
        }
        if (targetLight != null)
        {
            targetLight.intensity = 0f;
        }

        Debug.Log("[QuestTransition] Transition phase 1 completed. Starting final absolute white flash...");
        
        float flashTimer = 0f;
        float preFlashFog = targetMeanFreePath;
        float absoluteWhiteFog = 0.01f;

        while (flashTimer < finalWhiteFlashDuration)
        {
            flashTimer += Time.deltaTime;
            float flashT = flashTimer / finalWhiteFlashDuration;

            if (hdrpFog != null)
            {
                float flashPath = Mathf.Lerp(preFlashFog, absoluteWhiteFog, flashT);
                hdrpFog.meanFreePath.Override(flashPath);
            }

            yield return null;
        }

        if (hdrpFog != null)
        {
            hdrpFog.meanFreePath.Override(absoluteWhiteFog);
        }

        Debug.Log("[QuestTransition] Flash complete. Loading target scene: " + targetSceneName);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private void SetPlaneAlpha(float alpha)
    {
        if (planeMaterial != null)
        {
            if (planeMaterial.HasProperty("_BaseColor"))
            {
                Color color = planeMaterial.GetColor("_BaseColor");
                color.a = alpha;
                planeMaterial.SetColor("_BaseColor", color);
            }
            else if (planeMaterial.HasProperty("_Color"))
            {
                Color color = planeMaterial.color;
                color.a = alpha;
                planeMaterial.color = color;
            }
        }
    }
}