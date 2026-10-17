using UnityEngine;
using UnityEngine.InputSystem; 
using System.Collections;

/// <summary>
/// Handles smooth transition to a seated position, locks position (but allows looking around),
/// and automatically stands the player up after a set time.
/// </summary>
public class ChairInteraction : MonoBehaviour
{
    [Header("Seat Transforms")]
    [Tooltip("The exact position and rotation the player's head/body should snap to.")]
    public Transform sitPoint;
    
    [Tooltip("Where the player stands up (optional). If left empty, they stand up where they sat down.")]
    public Transform getUpPoint;

    [Header("Player Settings")]
    [Tooltip("Drag the Player's root object here (e.g., XR Origin or Player Capsule).")]
    public Transform playerRoot;

    [Header("Transition & Timing Settings")]
    [Tooltip("How fast the camera moves to the chair (lower is slower).")]
    public float transitionSpeed = 2f;
    
    [Tooltip("How many seconds the player stays seated before automatically standing up.")]
    public float autoStandUpTime = 30f; 

    [Header("Interaction & Input Settings")]
    [Tooltip("Maximum distance from the camera or controller to detect interaction.")]
    public float interactionDistance = 5f;
    [Tooltip("Optional layer mask for raycasting.")]
    public LayerMask interactableLayer = -1;
    [Tooltip("Optional: Assign your New Input System Action here (e.g., VR Trigger). If empty, uses Mouse Left Click.")]
    public InputActionReference interactAction;
    [Tooltip("Drag your VR Hand Ray Interactor Transform here. If empty, falls back to Main Camera.")]
    public Transform vrRaySource;

    private Camera playerCamera;
    private bool isSitting = false;
    private bool isTransitioning = false;
    
    // Tracks the timer so we can cancel it if the player stands up manually early
    private Coroutine autoStandRoutine;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        // Ignore new input if the player is currently transitioning
        if (isTransitioning) return;

        bool wasPressed = false;

        // 1. Check for custom Input Action
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            wasPressed = true;
        }
        // 2. Fallback: Check for PC Mouse Left Click
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            wasPressed = true;
        }

        // If an interaction button was pressed, shoot a raycast to see if looking at the chair
        // NOTE: We only allow clicking to sit down. If they are already sitting, they must wait 30s.
        if (wasPressed && !isSitting)
        {
            TryInteract();
        }
    }

    /// <summary>
    /// Locks the player's position to the chair every frame after they sit down.
    /// This prevents walking away but keeps the camera/head rotation completely free!
    /// </summary>
    void LateUpdate()
    {
        if (isSitting && !isTransitioning)
        {
            // Force the position to stay at the sit point
            playerRoot.position = sitPoint.position;
        }
    }

    private void TryInteract()
    {
        Ray ray;
        if (vrRaySource != null)
        {
            ray = new Ray(vrRaySource.position, vrRaySource.forward);
        }
        else if (playerCamera != null)
        {
            ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        }
        else return;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                InteractWithChair();
            }
        }
    }

    public void InteractWithChair()
    {
        if (isTransitioning) return; 

        if (!isSitting)
        {
            // Start sitting down
            StartCoroutine(TransitionToPosition(sitPoint, true));
        }
        else
        {
            // Start standing up
            Transform targetPoint = getUpPoint != null ? getUpPoint : playerRoot;
            StartCoroutine(TransitionToPosition(targetPoint, false));
        }
    }

    private IEnumerator TransitionToPosition(Transform target, bool isSittingDown)
    {
        isTransitioning = true;

        // Stop any existing 30-second timer just in case
        if (autoStandRoutine != null)
        {
            StopCoroutine(autoStandRoutine);
            autoStandRoutine = null;
        }

        Vector3 startPos = playerRoot.position;
        Quaternion startRot = playerRoot.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            playerRoot.position = Vector3.Lerp(startPos, target.position, smoothStep);
            
            // Lerp the rotation so the player faces the table when they first sit down
            playerRoot.rotation = Quaternion.Lerp(startRot, target.rotation, smoothStep);
            
            yield return null; 
        }

        playerRoot.position = target.position;
        playerRoot.rotation = target.rotation;

        isSitting = isSittingDown;
        isTransitioning = false;

        // If the player just finished sitting down, start the 30-second auto stand-up timer
        if (isSitting)
        {
            autoStandRoutine = StartCoroutine(AutoStandUpTimer());
        }

        Debug.Log(isSitting ? "[Chair] Player sat down. Timer started." : "[Chair] Player stood up.");
    }

    /// <summary>
    /// Waits for a specified time, then forces the player to stand up.
    /// </summary>
    private IEnumerator AutoStandUpTimer()
    {
        // Wait for exactly 30 seconds
        yield return new WaitForSeconds(autoStandUpTime);

        // If the player is still sitting, automatically trigger the stand-up process
        if (isSitting && !isTransitioning)
        {
            Debug.Log("[Chair] 30 seconds reached! Automatically standing up...");
            InteractWithChair();
        }
    }
}