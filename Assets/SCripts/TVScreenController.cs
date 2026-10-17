using UnityEngine;

/// <summary>
/// Controls TV screen visibility, audio playback, and applies CUSTOMIZABLE relative rotation sync.
/// Allows independent inversion of horizontal (left/right) and vertical (up/down) camera movements.
/// </summary>
public class TVScreenController : MonoBehaviour
{
    [Header("Virtual Camera Settings")]
    public Transform virtualCamera;
    public Transform targetQuad; 
    public Transform playerCamera;
    public float cameraMoveSpeed = 0.3f; 

    [Header("Rotation Inversion Settings")]
    [Tooltip("If true, looking left moves the picture left (inverts Yaw).")]
    public bool invertHorizontal = true; 
    [Tooltip("If true, looking up moves the picture up (inverts Pitch). Uncheck this if up/down feels wrong!")]
    public bool invertVertical = false;

    [Header("TV Screen Settings")]
    public AudioSource tvAudioSource;
    public MeshRenderer screenRenderer;

    private bool isTVActive = false;
    
    // Variables to store the initial state
    private Vector3 initialVCamPosition;
    private Quaternion initialVCamRotation;
    private Quaternion initialPlayerRotation;

    void Start()
    {
        if (screenRenderer != null) 
        {
            screenRenderer.enabled = false;
        }

        if (virtualCamera != null)
        {
            initialVCamPosition = virtualCamera.position;
            initialVCamRotation = virtualCamera.rotation;
        }
    }

    void Update()
    {
        if (isTVActive && virtualCamera != null && playerCamera != null && targetQuad != null)
        {
            // 1. Calculate the raw delta rotation since sitting down
            Quaternion playerDeltaRotation = playerCamera.rotation * Quaternion.Inverse(initialPlayerRotation);

            // 2. Convert to Euler angles to separate the axes
            Vector3 deltaEuler = playerDeltaRotation.eulerAngles;

            // 3. Normalize angles from (0 to 360) to (-180 to 180) for correct mathematical inversion
            float pitchX = deltaEuler.x > 180f ? deltaEuler.x - 360f : deltaEuler.x;
            float yawY = deltaEuler.y > 180f ? deltaEuler.y - 360f : deltaEuler.y;
            float rollZ = deltaEuler.z > 180f ? deltaEuler.z - 360f : deltaEuler.z;

            // 4. Apply independent inversion based on your inspector settings
            if (invertVertical) pitchX = -pitchX;
            if (invertHorizontal) yawY = -yawY;

            // 5. Reconstruct the modified delta rotation
            Quaternion modifiedDelta = Quaternion.Euler(pitchX, yawY, rollZ);

            // 6. Apply to the virtual camera
            virtualCamera.rotation = modifiedDelta * initialVCamRotation;

            // 7. Move strictly towards the target
            virtualCamera.position = Vector3.MoveTowards(
                virtualCamera.position, 
                targetQuad.position, 
                cameraMoveSpeed * Time.deltaTime
            );
        }
    }

    public void TurnOnTV()
    {
        if (virtualCamera != null && playerCamera != null)
        {
            virtualCamera.position = initialVCamPosition;
            virtualCamera.rotation = initialVCamRotation;
            initialPlayerRotation = playerCamera.rotation;
        }

        isTVActive = true;
        if (screenRenderer != null) screenRenderer.enabled = true;
        if (tvAudioSource != null) tvAudioSource.Play();
    }

    public void TurnOffTV()
    {
        isTVActive = false;
        if (screenRenderer != null) screenRenderer.enabled = false;
        if (tvAudioSource != null) tvAudioSource.Stop();
    }
}