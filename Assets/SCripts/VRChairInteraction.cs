using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// VR-specific chair interaction script.
/// Removes Cinemachine and FOV scaling (unsupported in VR, causes motion sickness).
/// Handles smooth transition to a seated position, a 30-second auto-stand timer, and quest completion.
/// </summary>
public class VRChairInteraction : MonoBehaviour
{
    [Header("Seat Transforms")]
    public Transform sitPoint;
    public Transform getUpPoint;

    [Header("Player Settings")]
    [Tooltip("Drag your player's root node here (e.g., XR Origin).")]
    public Transform playerRoot;
    public Transform tvScreenTarget;

    [Header("Transition & Timing Settings")]
    public float transitionSpeed = 2f;
    public float autoStandUpTime = 30f; 

    [Header("Interaction & Input Settings")]
    public float interactionDistance = 5f;
    public LayerMask interactableLayer = -1;
    [Tooltip("The Interact action for the VR right controller.")]
    public InputActionReference interactAction;
    [Tooltip("The VR raycast origin (e.g., Right Controller).")]
    public Transform vrRaySource;

    [Header("Trigger Events (Link TV Here)")]
    public UnityEvent onPlayerSatDown;
    public UnityEvent onPlayerStoodUp;

    [Header("Quest Completion Settings")]
    [Tooltip("Check this if finishing the 30s session should complete a quest.")]
    public bool completeQuestOnFinish = true;
    [Tooltip("Drag your QuestData asset here to be completed after the 30s timer.")]
    public QuestData questToComplete; 

    private bool isSitting = false;
    private bool isTransitioning = false;
    private bool completedByTimer = false; 
    
    private Coroutine autoStandRoutine;

    void Update()
    {
        if (isTransitioning) return;

        bool wasPressed = false;
        
        // Check VR trigger or PC left mouse button (for easy testing in the editor)
        if (interactAction != null && interactAction.action != null && interactAction.action.WasPressedThisFrame()) 
            wasPressed = true;
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) 
            wasPressed = true;

        if (wasPressed && !isSitting) TryInteract();
    }

    void LateUpdate()
    {
        if (isSitting && !isTransitioning && playerRoot != null && sitPoint != null)
        {
            // Lock player root position completely while sitting, 
            // but allow the player to freely look around via the VR headset
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
        else if (Camera.main != null) 
        {
            ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        }
        else return;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                InteractWithChair();
            }
        }
    }

    // Can be called directly by the global raycast manager (VRGlobalInteractionBridge)
    public void Interact()
    {
        InteractWithChair();
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

        Vector3 startPos = playerRoot.position;
        Quaternion startRot = playerRoot.rotation;
        
        Quaternion endRot = target.rotation; 
        
        // If sitting down and a TV screen target exists, calculate rotation to face the TV
        if (isSittingDown && tvScreenTarget != null)
        {
            Vector3 directionToScreen = tvScreenTarget.position - target.position;
            directionToScreen.y = 0; // Maintain horizontal rotation, don't interfere with the vertical axis
            
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

        // NOTE: Forced Camera X-axis rotation is removed for VR to prevent severe motion sickness and tracking conflicts.

        isSitting = isSittingDown;
        isTransitioning = false;

        if (isSitting)
        {
            autoStandRoutine = StartCoroutine(AutoStandUpTimer());
            
            // Trigger sat down event (e.g., to turn on the TV or start audio)
            onPlayerSatDown?.Invoke();
        }
        else
        {
            // Trigger stood up event (e.g., to turn off the TV)
            onPlayerStoodUp?.Invoke();

            // If stood up automatically via the 30s timer, complete the quest
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
            Debug.Log($"[VRChair] 30s session complete! Quest marked as done: {questToComplete.name}");
        }
        else
        {
            Debug.LogWarning("[VRChair] QuestManager.Instance or assigned questToComplete reference is missing!");
        }
    }

    private IEnumerator AutoStandUpTimer()
    {
        yield return new WaitForSeconds(autoStandUpTime);
        if (isSitting && !isTransitioning)
        {
            // Mark that this stand-up was triggered by the timer
            completedByTimer = true;
            InteractWithChair();
        }
    }
}