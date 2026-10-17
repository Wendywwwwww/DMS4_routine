using UnityEngine;

/// <summary>
/// Controls TV screen visibility, audio playback, and synchronizes the render camera's rotation and forward movement.
/// </summary>
public class TVScreenController : MonoBehaviour
{
    [Header("Virtual Camera Settings")]
    [Tooltip("The Virtual Camera used specifically to render to the Render Texture.")]
    public Transform virtualCamera;
    
    [Tooltip("The player's Main Camera. Used to synchronize rotation so the virtual camera looks exactly where the player looks.")]
    public Transform playerCamera;
    
    [Tooltip("Speed at which the virtual camera flies forward (along its local Z-axis/blue arrow).")]
    public float cameraMoveSpeed = 2f;

    [Header("TV Screen Settings")]
    [Tooltip("AudioSource for playing TV sounds (can be attached to the Quad or virtual camera).")]
    public AudioSource tvAudioSource;
    
    [Tooltip("MeshRenderer of the Quad (used to toggle screen visibility).")]
    public MeshRenderer screenRenderer;

    private bool isTVActive = false;

    void Start()
    {
        // Ensure the screen is hidden/black when the game starts
        if (screenRenderer != null) 
        {
            screenRenderer.enabled = false;
        }
    }

    void Update()
    {
        // Execute camera logic only during the 30 seconds when the TV is active (player is seated)
        if (isTVActive && virtualCamera != null)
        {
            // 1. Real-time view sync: The virtual camera rotates exactly as the player's head/camera rotates
            if (playerCamera != null)
            {
                virtualCamera.rotation = playerCamera.rotation;
            }

            // 2. Continuously move forward along the virtual camera's local Z-axis (forward direction)
            virtualCamera.Translate(Vector3.forward * cameraMoveSpeed * Time.deltaTime, Space.Self);
        }
    }

    /// <summary>
    /// Turns on the TV: shows the Quad, plays audio, and starts camera flight.
    /// </summary>
    public void TurnOnTV()
    {
        isTVActive = true;
        if (screenRenderer != null) screenRenderer.enabled = true;
        if (tvAudioSource != null) tvAudioSource.Play();
        Debug.Log("[TVScreen] TV turned on, camera moving!");
    }

    /// <summary>
    /// Turns off the TV: hides the Quad, stops audio, and stops camera flight.
    /// </summary>
    public void TurnOffTV()
    {
        isTVActive = false;
        if (screenRenderer != null) screenRenderer.enabled = false;
        if (tvAudioSource != null) tvAudioSource.Stop();
        Debug.Log("[TVScreen] TV turned off!");
    }
}