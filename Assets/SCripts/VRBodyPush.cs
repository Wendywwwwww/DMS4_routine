using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class VRBodyPush : MonoBehaviour
{
    [Header("Body Push Strength")]
    public float pushStrength = 2.0f;

    // Automatically triggered when the CharacterController hits another collider
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        // Ignore if the hit object doesn't have a Rigidbody or is kinematic
        if (rb == null || rb.isKinematic) return;

        // Prevent the player from launching objects they are standing on (ignore downward force)
        if (hit.moveDirection.y < -0.3f) return;

        // Calculate the push direction along the horizontal plane
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        
        // Apply velocity to the Rigidbody to push it
        rb.linearVelocity = pushDir * pushStrength;
    }
}