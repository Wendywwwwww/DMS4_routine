using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the interaction prompt UI using head/camera gaze (VR HMD center view).
/// Automatically ignores the player's own colliders and handles UI visibility.
/// </summary>
public class HeadGazeInteractionPromptManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Maximum interaction distance.")]
    public float interactionDistance = 3f;
    
    [Tooltip("Assign your global interaction prompt image or GameObject here. (Will auto-find if left empty!)")]
    public GameObject promptImageUI;

    [Tooltip("Layer mask for interactable world objects. (Make sure Player layer is excluded or handled).")]
    public LayerMask interactableLayer = ~0;

    private Camera playerCamera;

    void Start()
    {
        // Bind to the main camera (VR Headset camera)
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;

        TryFindPromptUI();
    }

    void Update()
    {
        if (promptImageUI == null)
        {
            TryFindPromptUI();
            if (promptImageUI == null) return; 
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        // Shoot a ray directly from the center of the VR headset view (head gaze)
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        bool hitInteractable = false;

        // Draw a debug ray in the Scene view to see where your head is looking
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Ignore if the ray hits the player itself
            if (hit.collider.transform.root == playerCamera.transform.root)
            {
                promptImageUI.SetActive(false);
                return;
            }

            // Optional debug to check what your head is currently looking at
            // Debug.Log($"[Head Gaze] Looking at: {hit.collider.name}");

            // 1. Check general interactive prompt component
            ObjectInteractionPrompt generalPrompt = hit.collider.GetComponentInParent<ObjectInteractionPrompt>();
            if (generalPrompt != null && generalPrompt.enabled)
            {
                hitInteractable = true;
            }
            
            // 2. Check 3-state quest items
            ThreeStateQuestItem questItem = hit.collider.GetComponentInParent<ThreeStateQuestItem>();
            if (questItem != null && questItem.enabled)
            {
                if (questItem.outlinableComponent != null && questItem.outlinableComponent.enabled)
                {
                    hitInteractable = true;
                }
            }

            // 3. Check VR Chair or global movement items
            if (hit.collider.GetComponentInParent<VRChairInteraction>() != null || 
                hit.collider.GetComponentInParent<VRClickToMoveGlobalZ>() != null)
            {
                hitInteractable = true;
            }
        }

        // Instantly show or hide based on gaze result
        promptImageUI.SetActive(hitInteractable);
    }

    private void TryFindPromptUI()
    {
        if (PromptReference.Instance != null)
        {
            promptImageUI = PromptReference.Instance.gameObject;
        }
        else
        {
            Image foundImage = Object.FindFirstObjectByType<Image>(FindObjectsInactive.Include);
            if (foundImage != null)
            {
                promptImageUI = foundImage.gameObject;
            }
        }
    }
}