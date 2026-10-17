using UnityEngine;
using System.Collections;

/// <summary>
/// Moves a specific target mesh by a set distance along the GLOBAL Z-axis (Blue line) when clicked.
/// </summary>
public class ClickToMoveGlobalZ : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The mesh or object you want to move. Drag it here.")]
    public Transform targetMesh;
    
    [Tooltip("How far the object should move along the global Z-axis (Blue line).")]
    public float moveDistanceZ = 5f;
    
    [Tooltip("How fast the object moves.")]
    public float moveSpeed = 3f;

    // Prevents spam-clicking while it's already moving
    private bool isMoving = false;
    
    // Prevents it from moving infinitely if clicked multiple times
    private bool hasMoved = false; 

    /// <summary>
    /// Unity's built-in mouse click detection (requires a Collider on this object).
    /// </summary>
    private void OnMouseDown()
    {
        TriggerMove();
    }

    /// <summary>
    /// Call this method from your custom interaction scripts to trigger the movement.
    /// </summary>
    public void TriggerMove()
    {
        if (targetMesh == null)
        {
            Debug.LogWarning("[ClickToMoveGlobalZ] Target Mesh is not assigned!");
            return;
        }

        // Only move if it's not currently moving and hasn't already moved
        if (!isMoving && !hasMoved)
        {
            StartCoroutine(MoveRoutine());
        }
    }

    private IEnumerator MoveRoutine()
    {
        isMoving = true;

        // Calculate the target position along the Global Z-axis
        Vector3 startPos = targetMesh.position;
        Vector3 targetPos = startPos + new Vector3(0, 0, moveDistanceZ);

        // Smoothly move towards the target position
        while (Vector3.Distance(targetMesh.position, targetPos) > 0.01f)
        {
            targetMesh.position = Vector3.MoveTowards(
                targetMesh.position, 
                targetPos, 
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Ensure it snaps perfectly to the exact target position at the end
        targetMesh.position = targetPos;
        
        isMoving = false;
        hasMoved = true; // Mark as moved so it doesn't keep going every time you click
        
        Debug.Log($"[ClickToMoveGlobalZ] {targetMesh.name} has finished moving along Global Z.");
    }
}