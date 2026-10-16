using UnityEngine;

/// <summary>
/// Handles seamless portal rendering and teleportation between two doors within the SAME scene.
/// Attach this script to both Door A and Door B.
/// </summary>
public class SameScenePortal : MonoBehaviour
{
    [Header("Portal Pair Settings")]
    public Transform thisDoor;             // The door you are currently standing at
    public Transform targetDoor;           // The other door you want to look/teleport into
    
    [Header("Camera & Rendering")]
    public Camera portalCaptureCamera;     // The camera that captures the view from the target door
    public RenderTexture portalTexture;    // The render texture applied to this door's screen
    public MeshRenderer doorScreenRenderer;// The quad renderer on this door

    private Transform playerCam;           // Reference to the player's main camera
    private Transform playerTransform;     // Reference to the player's transform

    void Start()
    {
        if (Camera.main != null)
        {
            playerCam = Camera.main.transform;
            playerTransform = playerCam.root;
        }

        // Initialize RenderTexture if not assigned
        if (portalTexture == null)
        {
            portalTexture = new RenderTexture(Screen.width, Screen.height, 24);
            portalTexture.antiAliasing = 4;
        }

        if (portalCaptureCamera != null)
        {
            portalCaptureCamera.targetTexture = portalTexture;
        }

        if (doorScreenRenderer != null)
        {
            doorScreenRenderer.material.mainTexture = portalTexture;
        }
    }

    void LateUpdate()
    {
        if (playerCam == null || targetDoor == null || portalCaptureCamera == null || thisDoor == null)
        {
            if (portalCaptureCamera != null && portalCaptureCamera.gameObject.activeSelf)
                portalCaptureCamera.gameObject.SetActive(false);
            return;
        }

        if (!portalCaptureCamera.gameObject.activeSelf)
            portalCaptureCamera.gameObject.SetActive(true);

        // --- Core Algorithm: Parallax / Off-Axis projection calculation ---
        // Maps the player's camera position relative to "thisDoor", and applies it to "targetDoor" 
        // so the capture camera sees exactly what the player would see if they were looking out of the target door.
        Matrix4x4 matrix = targetDoor.localToWorldMatrix * thisDoor.worldToLocalMatrix * playerCam.localToWorldMatrix;
        portalCaptureCamera.transform.SetPositionAndRotation(matrix.GetColumn(3), matrix.rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            Transform player = other.transform.root; // Get the top-level player object
            
            // Handle CharacterController if present to avoid positioning conflict
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 1. Calculate relative local position and rotation from this door
            Vector3 localPos = thisDoor.InverseTransformPoint(player.position);
            Quaternion localRot = Quaternion.Inverse(thisDoor.rotation) * player.rotation;

            // 2. Teleport player smoothly to the target door's coordinate system
            player.position = targetDoor.TransformPoint(localPos);
            player.rotation = targetDoor.rotation * localRot;

            if (cc != null) cc.enabled = true;
        }
    }
}