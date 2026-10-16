using UnityEngine;
using UnityEngine.InputSystem; // Import New Input System

/// <summary>
/// Detects when the player looks/points at an object via PC Camera or VR Controller Ray,
/// enabling its Outlinable component and supporting dual PC/VR interactions.
/// Completely decoupled from UI prompts.
/// </summary>
public class ObjectInteractionPrompt : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("Drag the 'Outlinable' script/component found on this object here.")]
    public MonoBehaviour outlinableComponent;

    [Header("Interaction Settings")]
    [Tooltip("Maximum distance from the camera or controller to detect interaction.")]
    public float interactionDistance = 5f;
    [Tooltip("Optional layer mask for raycasting.")]
    public LayerMask interactableLayer = -1;

    [Header("VR Hand Ray Reference")]
    [Tooltip("Drag your VR Hand Ray Interactor Transform here (e.g., Right Controller). If empty, falls back to Main Camera.")]
    public Transform vrRaySource;

    private Camera playerCamera;
    private bool isLookingAtObject = false;

    void Start()
    {
        playerCamera = Camera.main;

        // Ensure outline is disabled at start
        if (outlinableComponent != null)
        {
            outlinableComponent.enabled = false;
        }
    }

    void Update()
    {
        Ray ray;

        // Prioritize VR Hand Ray if assigned; otherwise fallback to PC Center Camera Ray
        if (vrRaySource != null)
        {
            ray = new Ray(vrRaySource.position, vrRaySource.forward);
        }
        else if (playerCamera != null)
        {
            ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        }
        else
        {
            return;
        }

        RaycastHit hit;
        bool lookingAtThisObject = false;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                lookingAtThisObject = true;
            }
        }

        if (lookingAtThisObject && !isLookingAtObject)
        {
            OnLookEnter();
        }
        else if (!lookingAtThisObject && isLookingAtObject)
        {
            OnLookExit();
        }

        isLookingAtObject = lookingAtThisObject;
    }

    private void OnLookEnter()
    {
        if (outlinableComponent != null)
        {
            outlinableComponent.enabled = true;
        }
    }

    private void OnLookExit()
    {
        if (outlinableComponent != null)
        {
            outlinableComponent.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}