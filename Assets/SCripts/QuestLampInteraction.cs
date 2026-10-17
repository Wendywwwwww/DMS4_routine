using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem; // Required for the new Input System

/// <summary>
/// A desk lamp interaction script that checks a prerequisite quest, enables designated components,
/// swaps the material of element 1 on click, completes an interaction quest, progressively increases fog density, 
/// auto-generates a full-screen fade to black, and loads a new scene.
/// </summary>
[RequireComponent(typeof(Collider))]
public class QuestLampInteraction : MonoBehaviour
{
    [Header("Quest Prerequisite")]
    public QuestData requiredQuest;

    [Header("Components to Enable")]
    public Behaviour[] componentsToEnable;

    [Header("Quest Completion on Interaction")]
    public QuestData questToCompleteOnInteract;

    [Header("Material Swap Settings (Element 1)")]
    public MeshRenderer targetMeshRenderer;
    public Material activeMaterial;
    public int materialElementIndex = 1;

    [Header("Transition Settings (Fog & Scene)")]
    public float transitionDuration = 3f;
    public float targetFogDensity = 0.08f;
    public string nextSceneName = "Scene5";

    [Header("Interaction Settings")]
    public float interactionDistance = 5f;
    public LayerMask interactableLayer = -1;
    [Tooltip("Leave empty to use standard Mouse Left Click.")]
    public InputActionReference interactAction;
    public Transform vrRaySource;

    private Camera playerCamera;
    private bool hasInteracted = false;
    private bool isTransitioning = false;
    
    private Image autoFadeImage;
    private Canvas autoFadeCanvas;

    private void Start()
    {
        playerCamera = Camera.main;
        
        SetComponentsEnabled(false);
        CreateDynamicFadeUI();
    }

    private void Update()
    {
        if (isTransitioning) return;

        CheckQuestStatus();

        // Supports both new and legacy input systems
        bool wasPressed = false;
        if (interactAction != null && interactAction.action.WasPressedThisFrame()) wasPressed = true;
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) wasPressed = true;
        else if (Input.GetMouseButtonDown(0)) wasPressed = true; // Fallback for legacy input system

        if (wasPressed)
        {
            TryInteract();
        }
    }

    private void CreateDynamicFadeUI()
    {
        GameObject canvasObj = new GameObject("DynamicLampFadeCanvas");
        autoFadeCanvas = canvasObj.AddComponent<Canvas>();
        autoFadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        autoFadeCanvas.sortingOrder = 999; 

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        autoFadeImage = imageObj.AddComponent<Image>();
        autoFadeImage.color = new Color(0f, 0f, 0f, 0f); 

        RectTransform rect = autoFadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        canvasObj.SetActive(false);
    }

    private void CheckQuestStatus()
    {
        if (requiredQuest != null && QuestManager.Instance != null)
        {
            if (QuestManager.Instance.IsQuestCompleted(requiredQuest))
            {
                SetComponentsEnabled(true);
            }
        }
    }

    private void SetComponentsEnabled(bool isEnabled)
    {
        if (componentsToEnable == null) return;

        foreach (Behaviour comp in componentsToEnable)
        {
            if (comp != null) comp.enabled = isEnabled;
        }
    }

    private void TryInteract()
    {
        if (hasInteracted) return;

        Ray ray;
        if (vrRaySource != null) ray = new Ray(vrRaySource.position, vrRaySource.forward);
        else if (playerCamera != null) ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        else return;

        RaycastHit hit;
        
        // Debug ray to visualize hit detection in the Scene window
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 2f);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                if (requiredQuest != null && QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted(requiredQuest))
                {
                    StartCoroutine(LampActivationRoutine());
                }
                else
                {
                    Debug.LogWarning("[QuestLamp] Lamp clicked, but prerequisite quest is not yet completed!");
                }
            }
        }
    }

    private IEnumerator LampActivationRoutine()
    {
        hasInteracted = true;
        isTransitioning = true;
        Debug.Log("[QuestLamp] Lamp activated! Starting transition sequence...");

        if (targetMeshRenderer != null && activeMaterial != null)
        {
            Material[] mats = targetMeshRenderer.materials;
            if (materialElementIndex >= 0 && materialElementIndex < mats.Length)
            {
                mats[materialElementIndex] = activeMaterial;
                targetMeshRenderer.materials = mats;
            }
        }

        if (QuestManager.Instance != null && questToCompleteOnInteract != null)
        {
            QuestManager.Instance.CompleteQuest(questToCompleteOnInteract);
        }

        RenderSettings.fog = true;

        if (autoFadeCanvas != null) autoFadeCanvas.gameObject.SetActive(true);

        float elapsedTime = 0f;
        float startFogDensity = RenderSettings.fogDensity;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);

            RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, targetFogDensity, t);

            if (autoFadeImage != null)
            {
                Color c = autoFadeImage.color;
                c.a = t;
                autoFadeImage.color = c;
            }

            yield return null;
        }

        RenderSettings.fogDensity = targetFogDensity;
        if (autoFadeImage != null)
        {
            Color c = autoFadeImage.color;
            c.a = 1f;
            autoFadeImage.color = c;
        }

        Debug.Log($"[QuestLamp] Loading scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
}