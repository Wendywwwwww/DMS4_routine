using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Global interaction bridge that casts rays from the VR Controller / Main Camera,
/// allowing ONE central script to trigger interactions for ProximityDoor, 
/// ThreeStateQuestItem, or any custom interactable object without per-object setups.
/// </summary>
public class VRGlobalInteractionBridge : MonoBehaviour
{
    [Header("Raycast Source")]
    [Tooltip("Drag your VR Hand/Ray Interactor Transform here (e.g., Right Controller / Ray Origin). If empty, falls back to Main Camera.")]
    public Transform vrRaySource;

    [Tooltip("Max interaction raycast distance.")]
    public float interactionDistance = 5f;

    [Tooltip("Layer mask for interactable world objects.")]
    public LayerMask interactableLayer = ~0;

    [Header("VR Input Action")]
    [Tooltip("Bind your VR controller action here (e.g., RightHand/UI Press or Trigger).")]
    public InputActionProperty vrInteractionAction;

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

    void Update()
    {
        bool isVrPressed = vrInteractionAction.action != null && vrInteractionAction.action.WasPressedThisFrame();
        bool isPcPressed = Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);

        if (isVrPressed || isPcPressed)
        {
            TryInteractWithTarget();
        }
    }

    private void TryInteractWithTarget()
    {
        Ray ray;

        if (vrRaySource != null)
        {
            ray = new Ray(vrRaySource.position, vrRaySource.forward);
        }
        else if (Camera.main != null)
        {
            ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        }
        else
        {
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            // 1. Try triggering ThreeStateQuestItem
            ThreeStateQuestItem questItem = hit.collider.GetComponentInParent<ThreeStateQuestItem>();
            if (questItem != null)
            {
                questItem.Interact();
                return;
            }

            // 2. Try triggering ProximityDoor
            ProximityDoor door = hit.collider.GetComponentInParent<ProximityDoor>();
            if (door != null)
            {
                door.ToggleDoor();
                return;
            }
        }
    }
}