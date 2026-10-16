using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A quest-driven interactive item that becomes active when its target quest is current.
/// Allows the player to press 'E' to interact, complete the quest, change materials, and forces prompts to hide.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class SingleInteractionItem : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("The quest associated with this item. Interaction will complete this quest.")]
    public QuestData targetQuest;

    [Header("Interaction Settings")]
    [Tooltip("The key used to interact with this item.")]
    public KeyCode interactionKey = KeyCode.E;
    [Tooltip("Maximum distance from the camera to allow interaction.")]
    public float interactionDistance = 3f;

    [Header("Visual Settings")]
    [Tooltip("The normal material displayed when the quest is not active or before interaction.")]
    public Material defaultMaterial;

    [Tooltip("The material to apply AFTER the interaction is completed.")]
    public Material completedMaterial;

    [Header("Component References")]
    [Tooltip("Drag the 'Outlinable' script component here.")]
    public MonoBehaviour outlinableComponent;

    [Tooltip("Drag the 'ObjectInteractionPrompt' script component here.")]
    public ObjectInteractionPrompt interactionPromptComponent;

    [Header("UI Cleanup Reference")]
    [Tooltip("Drag the [E] Prompt Image here so it can be forcefully hidden upon interaction.")]
    public Image promptImageToHide;

    private MeshRenderer meshRenderer;
    private Camera playerCamera;
    private bool hasInteracted = false;
    private bool isCurrentActiveItem = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        playerCamera = Camera.main;

        // Set initial dormant state
        SetItemState(false, false);
    }

    void Update()
    {
        if (hasInteracted) return;

        // 1. Continuously verify if this item's quest is currently active
        CheckActiveState();

        // 2. If active, check if player is looking at it and pressing 'E'
        if (isCurrentActiveItem)
        {
            CheckPlayerInteraction();
        }
    }

    /// <summary>
    /// Checks if the target quest is currently ready/active in the QuestManager.
    /// </summary>
    private void CheckActiveState()
    {
        if (targetQuest == null || QuestManager.Instance == null) return;

        // Check if this quest is the current active task via QuestManager
        bool isActiveQuest = QuestManager.Instance.IsQuestActive(targetQuest);

        if (isActiveQuest != isCurrentActiveItem)
        {
            isCurrentActiveItem = isActiveQuest;
            SetItemState(isCurrentActiveItem, isCurrentActiveItem);
        }
    }

    /// <summary>
    /// Controls the activation of outlining, interaction prompts, and states.
    /// </summary>
    private void SetItemState(bool canInteract, bool showOutline)
    {
        if (outlinableComponent != null)
        {
            outlinableComponent.enabled = showOutline;
        }

        if (interactionPromptComponent != null)
        {
            interactionPromptComponent.enabled = canInteract;
        }
    }

    /// <summary>
    /// Performs a raycast check from the camera and listens for the 'E' key press.
    /// </summary>
    private void CheckPlayerInteraction()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Check if the player is looking directly at this object and presses the interaction key
            if (hit.collider.gameObject == gameObject && Input.GetKeyDown(interactionKey))
            {
                Interact();
            }
        }
    }

    /// <summary>
    /// Core interaction logic called when the player presses 'E' on the active item.
    /// </summary>
    public void Interact()
    {
        if (hasInteracted) return;

        hasInteracted = true;
        ExecuteInteractionEffects();

        Debug.Log($"[Interaction] Successfully interacted with {gameObject.name} and completed quest.");
    }

    /// <summary>
    /// Applies visual changes, locks further interactions, completes the target quest, and force-hides UI.
    /// </summary>
    private void ExecuteInteractionEffects()
    {
        // A. Complete the target quest via global QuestManager
        if (targetQuest != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(targetQuest);
        }

        // B. Disable outlining and prompts permanently
        SetItemState(false, false);

        // C. Force-hide the prompt image directly to prevent it from sticking on screen
        if (promptImageToHide != null)
        {
            promptImageToHide.gameObject.SetActive(false);
        }

        // D. Switch to the completed material
        if (completedMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = completedMaterial;
        }
    }
}