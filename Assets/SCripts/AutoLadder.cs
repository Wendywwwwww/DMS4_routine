using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;

[RequireComponent(typeof(Collider))]
public class AutoLadder : MonoBehaviour
{
    [Header("VR Jump Input Action")]
    public InputActionReference jumpAction;

    [Header("Smooth Climb Duration (Seconds)")]
    public float climbDuration = 1.5f;

    [Header("Landing Offset (X, Y, Z)")]
    public Vector3 landingOffset = new Vector3(0, 0.1f, 0.5f);

    private XROrigin playerOrigin;
    private JumpProvider playerJumpProvider;
    private bool isPlayerNear = false;
    private bool isClimbing = false;
    private Collider ladderCollider;

    private void Start()
    {
        ladderCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isClimbing && other.CompareTag("Player"))
        {
            playerOrigin = other.GetComponentInParent<XROrigin>();
            
            if (playerOrigin != null)
            {
                playerJumpProvider = playerOrigin.GetComponentInChildren<JumpProvider>();
                if (playerJumpProvider != null)
                {
                    playerJumpProvider.enabled = false;
                }
            }
            
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Only trigger exit if we are not currently climbing
        if (!isClimbing && other.CompareTag("Player"))
        {
            ResetLadderState();
        }
    }

    private void Update()
    {
        if (isPlayerNear && !isClimbing && jumpAction != null && jumpAction.action.WasPressedThisFrame())
        {
            if (playerOrigin != null)
            {
                StartCoroutine(SmoothClimbRoutine());
            }
        }
    }

    private IEnumerator SmoothClimbRoutine()
    {
        isClimbing = true;

        CharacterController cc = playerOrigin.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // Disabling this breaks Unity's OnTriggerExit

        Vector3 startPos = playerOrigin.transform.position;
        float topY = ladderCollider.bounds.max.y;
        Vector3 baseEndPos = new Vector3(startPos.x, topY, startPos.z);
        Vector3 endPos = baseEndPos + transform.TransformDirection(landingOffset);

        float elapsedTime = 0f;

        while (elapsedTime < climbDuration)
        {
            float t = elapsedTime / climbDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            playerOrigin.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerOrigin.transform.position = endPos;

        if (cc != null) cc.enabled = true;
        
        // CRITICAL FIX: Forcefully reset the state because disabling CC bypassed OnTriggerExit
        ResetLadderState();
        isClimbing = false;
    }

    // Helper method to clean up references and restore normal jump
    private void ResetLadderState()
    {
        isPlayerNear = false;
        
        if (playerJumpProvider != null)
        {
            playerJumpProvider.enabled = true;
            playerJumpProvider = null;
        }
        
        playerOrigin = null;
    }
}