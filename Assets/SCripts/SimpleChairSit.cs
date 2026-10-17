using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// A simple and smooth chair interaction script. 
/// Automatically finds the player at runtime if Player Root is left empty,
/// smoothly transitions them to the seat, and allows free head rotation without standing up.
/// </summary>
public class SimpleChairSit : MonoBehaviour
{
    [Header("Seat & Player Settings")]
    [Tooltip("The target transform where the player will sit.")]
    public Transform sitPoint;
    [Tooltip("Optional. Leave empty to automatically find the object with 'Player' tag at runtime.")]
    public Transform playerRoot;
    [Tooltip("Optional target (like a TV or desk) to face when sitting down.")]
    public Transform lookAtTarget;

    [Header("Transition Settings")]
    [Tooltip("Speed of the smooth transition to the chair.")]
    public float transitionSpeed = 3f;

    [Header("Interaction Settings")]
    public float interactionDistance = 5f;
    public LayerMask interactableLayer = -1;
    [Tooltip("Leave empty to use standard Mouse Left Click.")]
    public InputActionReference interactAction;
    public Transform vrRaySource;

    private Camera playerCamera;
    private bool isSitting = false;
    private bool isTransitioning = false;

    private void Start()
    {
        playerCamera = Camera.main;

        // Automatically find the player if playerRoot was left empty in the Prefab
        if (playerRoot == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerRoot = playerObj.transform;
            }
            else
            {
                Debug.LogWarning($"[SimpleChairSit] '{gameObject.name}' could not find any object with the 'Player' tag in the scene!");
            }
        }
    }

    private void Update()
    {
        // Once seated, disable further interaction
        if (isSitting || isTransitioning) return;

        bool wasPressed = false;
        if (interactAction != null && interactAction.action.WasPressedThisFrame()) wasPressed = true;
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) wasPressed = true;

        if (wasPressed) TryInteract();
    }

    private void LateUpdate()
    {
        // Completely lock player's position while seated, but allow free head/camera look
        if (isSitting && !isTransitioning && playerRoot != null && sitPoint != null)
        {
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
            // Check if this chair or its child was clicked
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                SitDown();
            }
        }
    }

    public void SitDown()
    {
        if (isSitting || isTransitioning || sitPoint == null) return;

        // Fallback check if playerRoot is still missing
        if (playerRoot == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerRoot = playerObj.transform;
            else
            {
                Debug.LogError("[SimpleChairSit] Cannot sit down because Player Root is missing and no 'Player' tag was found!");
                return;
            }
        }

        StartCoroutine(SmoothSitRoutine());
    }

    private IEnumerator SmoothSitRoutine()
    {
        isTransitioning = true;

        Vector3 startPos = playerRoot.position;
        Quaternion startRot = playerRoot.rotation;

        // Calculate target rotation (optionally facing a lookAtTarget)
        Quaternion endRot = sitPoint.rotation;
        if (lookAtTarget != null)
        {
            Vector3 directionToTarget = lookAtTarget.position - sitPoint.position;
            directionToTarget.y = 0; // Keep horizontal alignment

            if (directionToTarget != Vector3.zero)
            {
                endRot = Quaternion.LookRotation(directionToTarget);
            }
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            playerRoot.position = Vector3.Lerp(startPos, sitPoint.position, smoothStep);
            playerRoot.rotation = Quaternion.Lerp(startRot, endRot, smoothStep);

            yield return null;
        }

        // Snap precisely to final position and rotation
        playerRoot.position = sitPoint.position;
        playerRoot.rotation = endRot;

        // Reset camera pitch offset if needed
        if (playerCamera != null)
        {
            Vector3 camLocalEuler = playerCamera.transform.localEulerAngles;
            camLocalEuler.x = 0f;
            playerCamera.transform.localEulerAngles = camLocalEuler;
        }

        isSitting = true;
        isTransitioning = false;

        Debug.Log("[SimpleChairSit] Player is now seated.");
    }
}