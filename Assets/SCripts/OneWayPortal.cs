using UnityEngine;
using System.Collections;

/// <summary>
/// Simplified one-way portal script. Synchronizes capture camera movement via 
/// relative transform calculation and handles safe player teleportation.
/// Attach this script ONLY to Door A.
/// </summary>
public class OneWayPortal : MonoBehaviour
{
    [Header("Portal Target Settings")]
    public Transform doorA;                // Starting door (Door A)
    public Transform doorB;                // Target exit door (Door B)
    public float teleportOffset = 2.0f;    // Forward offset from Door B after teleporting
    
    [Header("Position Fine-Tuning Offsets")]
    public float rightOffset = 0.0f;       // Shift right (+) or left (-)
    public float upOffset = 0.0f;          // Height adjustment

    [Header("Camera & Rendering")]
    public Camera portalCaptureCamera;     // Camera behind Door B capturing the scene
    public RenderTexture portalTexture;    // Render texture applied to Door A
    public MeshRenderer doorAScreenRenderer; // Screen mesh on Door A

    private Transform playerCamTransform;  // Reference to the player's main camera
    private Camera playerCam;
    private static bool isTeleporting = false; // Global lock to prevent multi-triggering

    void Start()
    {
        if (Camera.main != null)
        {
            playerCam = Camera.main;
            playerCamTransform = playerCam.transform;
        }

        // Initialize RenderTexture
        if (portalTexture == null)
        {
            portalTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.DefaultHDR);
            portalTexture.antiAliasing = 4;
        }

        if (portalCaptureCamera != null)
        {
            portalCaptureCamera.targetTexture = portalTexture;
            portalCaptureCamera.enabled = true;
        }

        if (doorAScreenRenderer != null && portalTexture != null)
        {
            doorAScreenRenderer.material.mainTexture = portalTexture;
        }
    }

    void LateUpdate()
    {
        if (playerCamTransform == null || playerCam == null || doorB == null || portalCaptureCamera == null || doorA == null)
            return;

        // --- Simplified Camera Synchronization ---
        // Calculate player's relative position and rotation to Door A, then apply to Door B
        Vector3 localPos = doorA.InverseTransformPoint(playerCamTransform.position);
        Quaternion localRot = Quaternion.Inverse(doorA.rotation) * playerCamTransform.rotation;

        portalCaptureCamera.transform.position = doorB.TransformPoint(localPos);
        portalCaptureCamera.transform.rotation = doorB.rotation * localRot;

        // Sync camera optics for correct perspective
        portalCaptureCamera.fieldOfView = playerCam.fieldOfView;
        portalCaptureCamera.aspect = playerCam.aspect;
        portalCaptureCamera.nearClipPlane = playerCam.nearClipPlane;
        portalCaptureCamera.farClipPlane = playerCam.farClipPlane;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return;

        CharacterController cc = other.GetComponentInParent<CharacterController>();
        if (cc != null)
        {
            isTeleporting = true;
            StartCoroutine(ExecuteTeleport(cc.transform));
        }
    }

    // Safe teleportation coroutine avoiding CharacterController position conflicts
    private IEnumerator ExecuteTeleport(Transform playerTransform)
    {
        MonoBehaviour fpController = (MonoBehaviour)playerTransform.GetComponent("FirstPersonController");
        CharacterController cc = playerTransform.GetComponent<CharacterController>();

        if (fpController != null) fpController.enabled = false;
        if (cc != null) cc.enabled = false;

        Vector3 localPos = doorA.InverseTransformPoint(playerTransform.position);
        Quaternion localRot = Quaternion.Inverse(doorA.rotation) * playerTransform.rotation;

        Vector3 targetPos = doorB.TransformPoint(localPos) 
                            + (doorB.forward * teleportOffset) 
                            + (doorB.right * rightOffset) 
                            + (Vector3.up * upOffset);

        Quaternion targetRot = doorB.rotation * localRot;

        playerTransform.position = targetPos;
        playerTransform.rotation = targetRot;

        yield return null; // Wait 1 frame for physics/controller sync

        if (cc != null) cc.enabled = true;
        if (fpController != null) fpController.enabled = true;

        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
    }
}