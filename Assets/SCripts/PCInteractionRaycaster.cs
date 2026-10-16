using UnityEngine;

/// <summary>
/// A temporary raycaster designed for PC testing and gray-boxing.
/// It simulates VR ray interactions by casting a ray from the center of the camera.
/// NOTE: This script should be removed or disabled when transitioning to the XR Interaction Toolkit.
/// </summary>
public class PCInteractionRaycaster : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The maximum distance (in meters) the player can interact with objects.")]
    public float interactRange = 3f;

    // Keeps track of the object currently being targeted by the raycast.
    private IInteractable currentTarget;

    void Update()
    {
        PerformRaycast();
        HandleInput();
    }

    /// <summary>
    /// Casts a ray forward from the camera to detect objects implementing IInteractable.
    /// </summary>
    private void PerformRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            IInteractable interactableObj = hit.collider.GetComponent<IInteractable>();

            if (interactableObj != null)
            {
                // If a new interactive object is detected, switch the highlight target.
                if (currentTarget != interactableObj)
                {
                    ClearTarget();
                    
                    currentTarget = interactableObj;
                    currentTarget.OnHoverEnter();
                }
            }
            else
            {
                // The ray hit a standard object (e.g., a wall), so clear the current target.
                ClearTarget();
            }
        }
        else
        {
            // The ray is pointing at empty space.
            ClearTarget();
        }
    }

    /// <summary>
    /// Listens for the interaction input.
    /// </summary>
    private void HandleInput()
    {
        // For early PC builds. Consider migrating to the New Input System for better VR mapping later.
        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            currentTarget.OnInteract();
        }
    }

    /// <summary>
    /// Safely clears the current target and triggers its exit behavior.
    /// </summary>
    private void ClearTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.OnHoverExit();
            currentTarget = null;
        }
    }
}