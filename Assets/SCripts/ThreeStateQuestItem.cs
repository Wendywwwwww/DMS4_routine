using UnityEngine;
using UnityEngine.InputSystem; // Import New Input System

/// <summary>
/// Manages a 3-state quest item. 
/// Outline is persistently enabled as long as the target quest is Active.
/// Completely decoupled from UI prompts. Supports PC Mouse/Keyboard & VR Controller inputs.
/// </summary>
public class ThreeStateQuestItem : MonoBehaviour
{
    public enum ItemBehaviorType
    {
        SpawnObject,   
        RemoveObject,  
        InteractOnly   
    }

    [Header("Quest Settings")]
    [Tooltip("The quest associated with this item. Interaction will complete this quest.")]
    public QuestData targetQuest;

    [Header("Behavior Settings")]
    [Tooltip("Choose item behavior: Spawn, Remove, or InteractOnly.")]
    public ItemBehaviorType behaviorType = ItemBehaviorType.SpawnObject;

    [Header("Interaction Settings")]
    [Tooltip("The key used to interact with this item.")]
    public KeyCode interactionKey = KeyCode.E;
    [Tooltip("Maximum distance from the camera to allow interaction.")]
    public float interactionDistance = 3f;

    [Header("VR Input Mappings")]
    [Tooltip("Optional: Bind VR action here if you want local input handling on this object.")]
    public InputActionProperty vrInteractionAction;

    [Header("Visual Parts (e.g., Meshes and Materials)")]
    public QuestItemPart[] itemParts;

    [Header("Audio Reference")]
    [Tooltip("Directly drag the AudioSource component from this object here.")]
    public AudioSource itemAudioSource;

    [Header("Component References")]
    public MonoBehaviour outlinableComponent;
    
    [Header("Scripts Control List")]
    [Tooltip("Drag all custom interaction or movement scripts here to automatically enable/disable them based on quest state.")]
    public MonoBehaviour[] targetInteractionScripts;

    private Camera playerCamera;
    private bool hasInteracted = false;
    private ItemState currentState = ItemState.Invisible;

    public enum ItemState
    {
        Invisible,
        Active,
        Completed
    }

    [System.Serializable]
    public struct QuestItemPart
    {
        public MeshRenderer renderer;
        public Material defaultMaterial;
        public Material completedMaterial;
    }

    private void OnEnable()
    {
        if (vrInteractionAction.action != null)
        {
            vrInteractionAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (vrInteractionAction.action != null)
        {
            vrInteractionAction.action.Disable();
        }
    }

    void Start()
    {
        playerCamera = Camera.main;

        if (itemAudioSource == null)
        {
            itemAudioSource = GetComponent<AudioSource>();
        }

        currentState = ItemState.Invisible;
        ApplyStateVisuals();
    }

    void Update()
    {
        if (hasInteracted) return;

        EvaluateQuestState();
        CheckPlayerInteraction();
    }

    private void EvaluateQuestState()
    {
        if (targetQuest == null || QuestManager.Instance == null) return;

        bool isCompleted = QuestManager.Instance.IsQuestCompleted(targetQuest);
        if (isCompleted)
        {
            if (currentState != ItemState.Completed)
            {
                currentState = ItemState.Completed;
                ApplyStateVisuals();
            }
            return;
        }

        bool isActive = QuestManager.Instance.IsQuestActive(targetQuest);
        ItemState desiredState = isActive ? ItemState.Active : ItemState.Invisible;

        if (desiredState != currentState)
        {
            currentState = desiredState;
            ApplyStateVisuals();
        }
    }

    private void ApplyStateVisuals()
    {
        switch (behaviorType)
        {
            case ItemBehaviorType.SpawnObject:
                ApplySpawnVisuals();
                break;
            case ItemBehaviorType.RemoveObject:
                ApplyRemoveVisuals();
                break;
            case ItemBehaviorType.InteractOnly:
                ApplyInteractOnlyVisuals();
                break;
        }
    }

    private void ApplySpawnVisuals()
    {
        switch (currentState)
        {
            case ItemState.Invisible:
                SetRenderersVisible(false);
                SetCustomScriptsEnabled(false);
                break;
            case ItemState.Active:
                SetRenderersVisible(true);
                ApplyMaterialsForState(false);
                SetCustomScriptsEnabled(true);
                break;
            case ItemState.Completed:
                SetRenderersVisible(true);
                ApplyMaterialsForState(true);
                SetCustomScriptsEnabled(true); 
                break;
        }
    }

    private void ApplyRemoveVisuals()
    {
        switch (currentState)
        {
            case ItemState.Invisible:
                SetRenderersVisible(true);
                SetCustomScriptsEnabled(false);
                break;
            case ItemState.Active:
                SetRenderersVisible(true);
                ApplyMaterialsForState(false);
                SetCustomScriptsEnabled(true);
                break;
            case ItemState.Completed:
                SetRenderersVisible(false); 
                SetCustomScriptsEnabled(false);
                break;
        }
    }

    private void ApplyInteractOnlyVisuals()
    {
        SetRenderersVisible(true); 

        switch (currentState)
        {
            case ItemState.Invisible:
                SetCustomScriptsEnabled(false);
                break;
            case ItemState.Active:
                ApplyMaterialsForState(false);
                SetCustomScriptsEnabled(true);
                break;
            case ItemState.Completed:
                ApplyMaterialsForState(true);
                SetCustomScriptsEnabled(true); 
                break;
        }
    }

    private void SetRenderersVisible(bool isVisible)
    {
        foreach (var part in itemParts)
        {
            if (part.renderer != null) part.renderer.enabled = isVisible;
        }
    }

    private void ApplyMaterialsForState(bool isCompleted)
    {
        foreach (var part in itemParts)
        {
            if (part.renderer != null)
            {
                Material targetMat = isCompleted ? part.completedMaterial : part.defaultMaterial;
                if (targetMat != null) part.renderer.material = targetMat;
            }
        }
    }

    private void SetCustomScriptsEnabled(bool enable)
    {
        if (targetInteractionScripts != null)
        {
            foreach (var script in targetInteractionScripts)
            {
                if (script != null) script.enabled = enable;
            }
        }
    }

    private void CheckPlayerInteraction()
    {
        if (targetQuest == null || QuestManager.Instance == null) return;

        bool isQuestActive = QuestManager.Instance.IsQuestActive(targetQuest);

        // Manage Outline: Enabled when quest is Active and not completed
        if (outlinableComponent != null)
        {
            outlinableComponent.enabled = isQuestActive && (currentState != ItemState.Completed);
        }

        if (isQuestActive && currentState == ItemState.Active)
        {
            // Check inputs (VR action OR PC Keyboard/Mouse)
            bool isVrPressed = vrInteractionAction.action != null && vrInteractionAction.action.WasPressedThisFrame();
            bool isPcPressed = Input.GetKeyDown(interactionKey) || Input.GetMouseButtonDown(0);

            if (isVrPressed || isPcPressed)
            {
                if (playerCamera == null) playerCamera = Camera.main;
                if (playerCamera == null) return;

                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, interactionDistance))
                {
                    if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                    {
                        Interact();
                    }
                }
            }
        }
    }

    public void Interact()
    {
        if (hasInteracted || currentState != ItemState.Active) return;

        hasInteracted = true;

        if (itemAudioSource != null && itemAudioSource.clip != null)
        {
            itemAudioSource.Play();
        }

        if (targetQuest != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(targetQuest);
        }

        currentState = ItemState.Completed;
        ApplyStateVisuals();

        if (outlinableComponent != null) outlinableComponent.enabled = false;

        Debug.Log($"[Interaction] Item interacted, quest completed (Behavior: {behaviorType}).");
    }
}