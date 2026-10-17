using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// Handles smooth transition to a seated position, locks position (but allows looking around),
/// applies a cinematic Cinemachine zoom over time, and automatically stands up after a set time.
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

    [Header("Cinemachine Zoom Settings")]
    [Tooltip("Cinemachine Camera here")]
    public CinemachineCamera vCam;
    
    [Tooltip("Enable FOV zoom effect while sitting?")]
    public bool enableZoom = true;
    [Tooltip("like 40 to 25, Cinemachine Lens FOV will slowly zoom in over time")]
    public float targetZoomFOV = 25f;
    [Tooltip("how many seconds it takes for the Cinemachine Lens FOV to reach the target zoom.")]
    public float zoomDuration = 15f;

    [Header("Interaction & Input Settings")]
    public float interactionDistance = 5f;
    public LayerMask interactableLayer = -1;
    public InputActionReference interactAction;
    public Transform vrRaySource;

    private Camera playerCamera;
    private bool isSitting = false;
    private bool isTransitioning = false;
    
    private Coroutine autoStandRoutine;
    private Coroutine zoomRoutine;
    private float defaultFOV = 40f; 

    void Start()
    {
        playerCamera = Camera.main;
        
        if (vCam != null)
        {
            defaultFOV = vCam.Lens.FieldOfView;
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        bool wasPressed = false;

        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            wasPressed = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            wasPressed = true;
        }

        if (wasPressed && !isSitting)
        {
            TryInteract();
        }
    }

    void LateUpdate()
    {
        if (isSitting && !isTransitioning)
        {
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
            StartCoroutine(TransitionToPosition(sitPoint, true));
        }
        else
        {
            Transform targetPoint = getUpPoint != null ? getUpPoint : playerRoot;
            StartCoroutine(TransitionToPosition(targetPoint, false));
        }
    }

    private IEnumerator TransitionToPosition(Transform target, bool isSittingDown)
    {
        isTransitioning = true;

        if (autoStandRoutine != null)
        {
            StopCoroutine(autoStandRoutine);
            autoStandRoutine = null;
        }

        if (!isSittingDown && enableZoom && vCam != null)
        {
            if (zoomRoutine != null) StopCoroutine(zoomRoutine);
            zoomRoutine = StartCoroutine(SmoothZoom(defaultFOV, 1f)); 
        }

        Vector3 startPos = playerRoot.position;
        Quaternion startRot = playerRoot.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            playerRoot.position = Vector3.Lerp(startPos, target.position, smoothStep);
            playerRoot.rotation = Quaternion.Lerp(startRot, target.rotation, smoothStep);
            
            yield return null; 
        }

        playerRoot.position = target.position;
        playerRoot.rotation = target.rotation;

        isSitting = isSittingDown;
        isTransitioning = false;

        if (isSitting)
        {
            autoStandRoutine = StartCoroutine(AutoStandUpTimer());
            
            if (enableZoom && vCam != null)
            {
                if (zoomRoutine != null) StopCoroutine(zoomRoutine);
                zoomRoutine = StartCoroutine(SmoothZoom(targetZoomFOV, zoomDuration));
            }
        }

        Debug.Log(isSitting ? "[Chair] Player sat down. Timer & Cinemachine Zoom started." : "[Chair] Player stood up.");
    }

    /// <summary>
    /// Smoothly changes the Cinemachine Lens FOV over a specified duration.
    /// </summary>
    private IEnumerator SmoothZoom(float targetFOV, float duration)
    {
        float startFOV = vCam.Lens.FieldOfView;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            
            var lens = vCam.Lens;
            lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, time / duration);
            vCam.Lens = lens;
            
            yield return null;
        }

        var finalLens = vCam.Lens;
        finalLens.FieldOfView = targetFOV;
        vCam.Lens = finalLens;
    }

    private IEnumerator AutoStandUpTimer()
    {
        yield return new WaitForSeconds(autoStandUpTime);

        if (isSitting && !isTransitioning)
        {
            InteractWithChair();
        }
    }
}