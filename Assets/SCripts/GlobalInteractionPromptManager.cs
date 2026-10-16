using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A centralized manager attached to the camera. 
/// Automatically finds the prompt UI using global static reference or scene search, 
/// and instantly shows/hides it based on raycasts.
/// </summary>
public class GlobalInteractionPromptManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Maximum interaction distance.")]
    public float interactionDistance = 3f;
    
    [Tooltip("Assign your global '[E] Interact' prompt image or GameObject here. (Will auto-find if left empty!)")]
    public GameObject promptImageUI;

    private Camera playerCamera;

    void Start()
    {
        // Get the camera reference from this GameObject or fallback to main camera
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;

        // Try to fetch the UI immediately on start
        TryFindPromptUI();
    }

    void Update()
    {
        // If the reference is somehow lost or wasn't ready during Start, keep trying in Update until we get it
        if (promptImageUI == null)
        {
            TryFindPromptUI();
            if (promptImageUI == null) return; // Still not found, skip this frame
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        // Shoot a ray from the center of the screen
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        bool hitInteractable = false;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Check if the hit object has a general interactable component and ensure it is enabled (ticked)
            ObjectInteractionPrompt generalPrompt = hit.collider.GetComponentInParent<ObjectInteractionPrompt>();
            if (generalPrompt != null && generalPrompt.enabled)
            {
                hitInteractable = true;
            }
            
            // Check if the hit object is an active quest item and ensure it is enabled (ticked)
            ThreeStateQuestItem questItem = hit.collider.GetComponentInParent<ThreeStateQuestItem>();
            if (questItem != null && questItem.enabled)
            {
                if (questItem.outlinableComponent != null && questItem.outlinableComponent.enabled)
                {
                    hitInteractable = true;
                }
            }
        }

        // Instantly show or hide based on whether a valid collider is hit this exact frame
        promptImageUI.SetActive(hitInteractable);
    }

    private void TryFindPromptUI()
    {
        // 1. Try finding via our global persistent reference (Most reliable across scenes!)
        if (PromptReference.Instance != null)
        {
            promptImageUI = PromptReference.Instance.gameObject;
        }
        else
        {
            // 2. Fallback: Search across the scene using FindFirstByType (including inactive objects)
            Image foundImage = Object.FindFirstObjectByType<Image>(FindObjectsInactive.Include);
            if (foundImage != null)
            {
                promptImageUI = foundImage.gameObject;
            }
        }
    }
}