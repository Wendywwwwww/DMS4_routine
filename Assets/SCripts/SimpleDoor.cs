using UnityEngine;
using UnityEngine.InputSystem; // Import New Input System
using System.Collections;

/// <summary>
/// Controls a door that can be opened/closed via raycast detection when pressing 'E', Mouse Left Click,
/// or VR Controller Buttons, featuring adjustable rotation axis, direction, permanent locking support, 
/// and an explicit empty object reference to act as the custom hinge pivot.
/// </summary>
public class ProximityDoor : MonoBehaviour
{
    [Header("Door Movement Settings")]
    [Tooltip("The angle to open the door (can be positive or negative, or adjusted via Axis/Invert).")]
    public float openAngle = 90f;
    [Tooltip("The rotation speed of the door swing.")]
    public float openSpeed = 3f;
    [Tooltip("The local axis around which the door rotates (e.g., Y-axis for standard doors).")]
    public Vector3 rotationAxis = Vector3.up;
    [Tooltip("Check this to invert the rotation direction if it swings the wrong way.")]
    public bool invertDirection = false;

    [Header("Custom Hinge Settings (Optional)")]
    [Tooltip("Assign an empty GameObject placed at the door hinge/edge if the door model's origin is in the center. Leave empty for doors with correct origins.")]
    public Transform hingeObject;

    [Header("Player Interaction Settings")]
    [Tooltip("Maximum distance from the camera/hand controller to allow interaction.")]
    public float interactionDistance = 3f;
    [Tooltip("The key used to interact with and open/close the door.")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("VR Input Mappings")]
    [Tooltip("Bind your VR controller action here (e.g., RightHand/UI Press or Trigger).")]
    public InputActionProperty vrInteractionAction;

    private Camera playerCamera;
    private bool isOpen = false;
    private bool isLockedShut = false; 
    
    // Rotation tracking for standard doors
    private Quaternion closedRotation;
    private Quaternion openRotation;

    // Transform tracking for hinge-object rotation
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Quaternion targetHingeRotation;

    private void OnEnable()
    {
        // Enable VR action listening
        if (vrInteractionAction.action != null)
        {
            vrInteractionAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        // Disable VR action listening
        if (vrInteractionAction.action != null)
        {
            vrInteractionAction.action.Disable();
        }
    }

    void Start()
    {
        playerCamera = Camera.main;

        if (hingeObject != null)
        {
            // Record initial state relative to the hinge object
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            float finalAngle = invertDirection ? -openAngle : openAngle;
            targetHingeRotation = initialRotation * Quaternion.AngleAxis(finalAngle, rotationAxis);
        }
        else
        {
            // Standard door logic: record initial local rotation
            closedRotation = transform.localRotation;

            float finalAngle = invertDirection ? -openAngle : openAngle;
            openRotation = closedRotation * Quaternion.AngleAxis(finalAngle, rotationAxis);
        }
    }

    void Update()
    {
        // If the door is permanently locked shut, skip all inputs and checks
        if (isLockedShut) return;

        // Listen for raycast interaction
        CheckPlayerRaycastInteraction();

        // Apply rotation based on whether a hinge object is assigned
        if (hingeObject != null)
        {
            ApplyHingeObjectMovement();
        }
        else
        {
            ApplyStandardDoorMovement();
        }
    }

    /// <summary>
    /// Handles standard door rotation around its own local transform origin.
    /// </summary>
    private void ApplyStandardDoorMovement()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
    }

    /// <summary>
    /// Handles rotation where the door swings around a designated hinge empty object.
    /// </summary>
    private void ApplyHingeObjectMovement()
    {
        Quaternion targetRot = isOpen ? targetHingeRotation : initialRotation;
        
        // Smoothly rotate around the hinge object's position and axis
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * openSpeed);
        
        // Keep the door anchored to the hinge position during rotation
        Vector3 worldAxis = hingeObject.TransformDirection(rotationAxis);
        transform.RotateAround(hingeObject.position, worldAxis, 0f);
    }

    /// <summary>
    /// Performs a raycast check from the center of the camera to see if the player is looking at the door.
    /// Supports PC controls (Keyboard/Mouse) and VR Controller actions.
    /// </summary>
    private void CheckPlayerRaycastInteraction()
    {
        // Check input triggers
        bool isVrButtonPressed = vrInteractionAction.action != null && vrInteractionAction.action.WasPressedThisFrame();
        bool isPcKeyPressed = Input.GetKeyDown(interactionKey) || Input.GetMouseButtonDown(0);

        // If no interaction input happened this frame, return early
        if (!isVrButtonPressed && !isPcKeyPressed) return;

        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Check if the ray hits this door or any of its child colliders
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                isOpen = !isOpen;
            }
        }
    }

    /// <summary>
    /// Public method to allow external VR Raycast events (e.g. Unity Events / XRI Interactables) 
    /// to toggle the door directly when pointed and clicked.
    /// </summary>
    public void ToggleDoor()
    {
        if (!isLockedShut)
        {
            isOpen = !isOpen;
        }
    }

    /// <summary>
    /// Permanently closes and locks the door, and disables interaction components.
    /// </summary>
    public void CloseAndLockDoor()
    {
        isLockedShut = true;
        isOpen = false;
        
        // 1. Disable the interaction prompt script if attached
        ObjectInteractionPrompt interactionPrompt = GetComponent<ObjectInteractionPrompt>();
        if (interactionPrompt != null)
        {
            interactionPrompt.enabled = false;
        }

        // 2. Disable any existing Outlinable component safely
        Behaviour outlinable = GetComponent("Outlinable") as Behaviour;
        if (outlinable != null)
        {
            outlinable.enabled = false;
        }

        // 3. Immediately start swinging back to the closed position
        StopAllCoroutines();
        StartCoroutine(SmoothCloseRoutine());
    }

    private IEnumerator SmoothCloseRoutine()
    {
        if (hingeObject != null)
        {
            while (Quaternion.Angle(transform.rotation, initialRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * openSpeed);
                yield return null;
            }
            transform.rotation = initialRotation;
        }
        else
        {
            while (Quaternion.Angle(transform.localRotation, closedRotation) > 0.1f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, closedRotation, Time.deltaTime * openSpeed);
                yield return null;
            }
            transform.localRotation = closedRotation;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Draw a line connecting the door to its custom hinge object if assigned
        if (hingeObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, hingeObject.position);
            Gizmos.DrawWireSphere(hingeObject.position, 0.08f);
        }
    }
}