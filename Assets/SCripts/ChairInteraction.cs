using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Events;

/// <summary>
/// Handles smooth transition to a seated position, Cinemachine zoom, TV trigger events, 
/// automatic stand-up after 30 seconds, and completing a draggable QuestData asset upon standing up.
/// </summary>
public class ChairInteraction : MonoBehaviour
{
    [Header("Seat Transforms")]
    public Transform sitPoint;
    public Transform getUpPoint;

    [Header("Player Settings")]
    public Transform playerRoot;
    public Transform tvScreenTarget;

    [Header("Transition & Timing Settings")]
    public float transitionSpeed = 2f;
    public float autoStandUpTime = 30f; 

    [Header("Cinemachine Zoom Settings")]
    public CinemachineCamera vCam;
    public bool enableZoom = true;
    public float targetZoomFOV = 25f;
    public float zoomDuration = 15f;

    [Header("Interaction & Input Settings")]
    public float interactionDistance = 5f;
    public LayerMask interactableLayer = -1;
    public InputActionReference interactAction;
    public Transform vrRaySource;

    [Header("Trigger Events (Link TV Here)")]
    public UnityEvent onPlayerSatDown;
    public UnityEvent onPlayerStoodUp;

    [Header("Quest Completion Settings")]
    [Tooltip("Check this if finishing the 30s session should complete a quest.")]
    public bool completeQuestOnFinish = true;
    [Tooltip("Drag your QuestData asset here to be completed via QuestManager when the player stands up after 30s.")]
    public QuestData questToComplete; 

    private Camera playerCamera;
    private bool isSitting = false;
    private bool isTransitioning = false;
    private bool completedByTimer = false; 
    
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
        if (interactAction != null && interactAction.action.WasPressedThisFrame()) wasPressed = true;
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) wasPressed = true;

        if (wasPressed && !isSitting) TryInteract();
    }

    void LateUpdate()
    {
        if (isSitting && !isTransitioning)
        {
            // Lock position completely, but allow camera rotation for looking around
            playerRoot.position = sitPoint.position;
        }
    }

    private void TryInteract()
    {
        Ray ray;
        if (vrRaySource != null) ray = new Ray(vrRaySource.position, vrRaySource.forward);
        else if (playerCamera != null) ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
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
            completedByTimer = false;
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

        // Quickly restore Cinemachine Lens FOV to normal when standing up
        if (!isSittingDown && enableZoom && vCam != null)
        {
            if (zoomRoutine != null) StopCoroutine(zoomRoutine);
            zoomRoutine = StartCoroutine(SmoothZoom(defaultFOV, 1f)); 
        }

        Vector3 startPos = playerRoot.position;
        Quaternion startRot = playerRoot.rotation;
        
        Quaternion endRot = target.rotation; 
        if (isSittingDown && tvScreenTarget != null)
        {
            Vector3 directionToScreen = tvScreenTarget.position - target.position;
            directionToScreen.y = 0; 
            
            if (directionToScreen != Vector3.zero)
            {
                endRot = Quaternion.LookRotation(directionToScreen);
            }
        }

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            playerRoot.position = Vector3.Lerp(startPos, target.position, smoothStep);
            playerRoot.rotation = Quaternion.Lerp(startRot, endRot, smoothStep);
            
            yield return null; 
        }

        playerRoot.position = target.position;
        playerRoot.rotation = endRot;

        if (isSittingDown && playerCamera != null && tvScreenTarget != null)
        {
            Vector3 camLocalEuler = playerCamera.transform.localEulerAngles;
            camLocalEuler.x = 0f; 
            playerCamera.transform.localEulerAngles = camLocalEuler;
        }

        isSitting = isSittingDown;
        isTransitioning = false;

        if (isSitting)
        {
            autoStandRoutine = StartCoroutine(AutoStandUpTimer());
            
            // Start cinematic Cinemachine zoom after sitting down
            if (enableZoom && vCam != null)
            {
                if (zoomRoutine != null) StopCoroutine(zoomRoutine);
                zoomRoutine = StartCoroutine(SmoothZoom(targetZoomFOV, zoomDuration));
            }

            // Trigger Sat Down Event
            onPlayerSatDown?.Invoke();
        }
        else
        {
            // Trigger Stood Up Event
            onPlayerStoodUp?.Invoke();

            // Complete the quest only if stood up automatically after 30s
            if (completedByTimer && completeQuestOnFinish)
            {
                CompleteQuest();
            }
        }
    }

    private void CompleteQuest()
    {
        if (QuestManager.Instance != null && questToComplete != null)
        {
            QuestManager.Instance.CompleteQuest(questToComplete);
            Debug.Log($"[Chair] 30s completed! Quest asset marked as done: {questToComplete.name}");
        }
        else
        {
            Debug.LogWarning("[Chair] QuestManager.Instance or assigned questToComplete reference is missing!");
        }
    }

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
            // Mark that this stand-up was triggered by the 30s timer
            completedByTimer = true;
            InteractWithChair();
        }
    }
}