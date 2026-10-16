using UnityEngine;

/// <summary>
/// Spawns a guide object in the player's blind spot (behind them) 
/// and shoots it towards a specific target mesh when the player enters the trigger.
/// </summary>
public class VisionGuideTrigger : MonoBehaviour
{
    [Header("Guide Setup")]
    [Tooltip("The prefab to spawn. It MUST have a Rigidbody component.")]
    public GameObject guidePrefab;
    
    [Tooltip("The target Transform/Mesh the guide will fly towards.")]
    public Transform targetMesh;

    [Header("Spawn Position (Relative to Player)")]
    [Tooltip("Distance behind the player to spawn the guide (ensures it spawns out of sight).")]
    public float spawnBehindDistance = 3f;
    [Tooltip("Height offset relative to the player's feet (e.g., 1.5 for head level).")]
    public float spawnHeightOffset = 1.5f;

    [Header("Flight Settings")]
    [Tooltip("Speed at which the guide flies towards the target.")]
    public float flySpeed = 15f;
    
    [Tooltip("If true, the guide will be destroyed after a set time. If false, it stays in the scene forever.")]
    public bool destroyAfterTime = false; // 默认设为 false，就不会消失了
    
    [Tooltip("Time in seconds before the guide is automatically destroyed (only works if destroyAfterTime is true).")]
    public float destroyAfterSeconds = 5f;

    [Header("Trigger Settings")]
    [Tooltip("If true, the trigger will only fire the first time the player enters.")]
    public bool triggerOnce = true;
    [Tooltip("The tag required to activate the trigger (usually 'Player').")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object has the correct tag and if it hasn't been triggered yet
        if (other.CompareTag(playerTag) && !hasTriggered)
        {
            if (triggerOnce)
            {
                hasTriggered = true;
            }

            SpawnAndShootGuide(other.transform);
        }
    }

    private void SpawnAndShootGuide(Transform playerTransform)
    {
        if (guidePrefab == null || targetMesh == null)
        {
            Debug.LogWarning("[VisionGuide] Missing guidePrefab or targetMesh. Cannot spawn guide!");
            return;
        }

        // 1. Calculate spawn position: Player position - Player forward * distance + Height offset
        Vector3 spawnPosition = playerTransform.position 
                              - playerTransform.forward * spawnBehindDistance 
                              + Vector3.up * spawnHeightOffset;

        // 2. Instantiate the guide object
        GameObject guideObj = Instantiate(guidePrefab, spawnPosition, Quaternion.identity);

        // 3. Force the object to look directly at the target
        guideObj.transform.LookAt(targetMesh);

        // 4. Get the Rigidbody and apply forward velocity
        Rigidbody rb = guideObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Since the object is already looking at the target, transform.forward points directly to it
            rb.linearVelocity = guideObj.transform.forward * flySpeed;
        }
        else
        {
            Debug.LogError("[VisionGuide] The guidePrefab is missing a Rigidbody component! It cannot fly.");
        }

        // 5. Destroy the object conditionally based on the toggle
        if (destroyAfterTime)
        {
            Destroy(guideObj, destroyAfterSeconds);
        }
    }
    
    // Draws a line in the editor for easy visual debugging
    private void OnDrawGizmos()
    {
        if (targetMesh != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetMesh.position);
        }
    }
}