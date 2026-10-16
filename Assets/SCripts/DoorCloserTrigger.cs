using UnityEngine;

/// <summary>
/// Triggers the door to permanently close and lock when the player enters the zone.
/// </summary>
public class DoorCloserTrigger : MonoBehaviour
{
    [Tooltip("Drag the ProximityDoor object here.")]
    public ProximityDoor targetDoor;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player") && targetDoor != null)
        {
            targetDoor.CloseAndLockDoor();
            hasTriggered = true; // Ensures it only fires once
        }
    }
}