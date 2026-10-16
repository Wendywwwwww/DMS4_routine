using UnityEngine;

/// <summary>
/// Applies a force to an existing object in the scene, 
/// shooting it towards a specific target when the player enters the trigger.
/// </summary>
public class TriggerPushObject : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("The existing object in the scene to push (MUST have a Rigidbody component).")]
    public GameObject objectToPush;
    
    [Tooltip("The final target Mesh/Transform you want it to fly towards.")]
    public Transform targetMesh;

    [Header("Physics Settings")]
    [Tooltip("The speed/force applied to the object.")]
    public float flySpeed = 15f;
    [Tooltip("Force the object to instantly look at the target before flying?")]
    public bool lookAtTarget = true;

    [Header("Trigger Settings")]
    [Tooltip("If true, the trigger will only fire the first time the player enters.")]
    public bool triggerOnce = true;
    [Tooltip("The tag required to activate the trigger (usually 'Player').")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !hasTriggered)
        {
            if (triggerOnce)
            {
                hasTriggered = true;
            }

            ShootExistingObject();
        }
    }

    private void ShootExistingObject()
    {
        if (objectToPush == null || targetMesh == null)
        {
            Debug.LogWarning("[TriggerPush] Missing objectToPush or targetMesh!");
            return;
        }

        Rigidbody rb = objectToPush.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 1. Calculate the absolute direction vector from the object's current position to the target
            Vector3 direction = (targetMesh.position - objectToPush.transform.position).normalized;

            // 2. (Optional) Make the object visually face the target
            if (lookAtTarget)
            {
                objectToPush.transform.LookAt(targetMesh);
            }

            // 3. Instantly apply velocity towards the target. 
            // ForceMode.VelocityChange ignores the object's mass and applies the exact speed.
            rb.AddForce(direction * flySpeed, ForceMode.VelocityChange);
            
            Debug.Log($"[TriggerPush] Successfully shot {objectToPush.name} towards {targetMesh.name}");
        }
        else
        {
            Debug.LogError($"[TriggerPush] {objectToPush.name} is missing a Rigidbody! Cannot push.");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (objectToPush != null && targetMesh != null)
        {
            Gizmos.color = Color.green;
            // Draw a green line from the object to the target to preview the trajectory in the scene
            Gizmos.DrawLine(objectToPush.transform.position, targetMesh.position);
        }
    }
}