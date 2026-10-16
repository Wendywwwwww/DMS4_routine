using UnityEngine;

/// <summary>
/// A testing script to verify the interaction framework.
/// Attach this to a 3D object (e.g., a Cube) with a Collider to test hover and interact events.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class TestInteractableItem : MonoBehaviour, IInteractable
{
    private MeshRenderer meshRenderer;
    private Color originalColor;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Cache the original material color to restore it later.
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void OnHoverEnter()
    {
        // Provide visual feedback (e.g., changing color) when targeted.
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.yellow;
        }
    }

    public void OnHoverExit()
    {
        // Restore the default appearance when the target is lost.
        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }
    }

    public void OnInteract()
    {
        // Execute the main logic for this object.
        // E.g., QuestSystem.CompleteQuest("Unlock_Door");
        Debug.Log($"[Interaction System] Successfully interacted with: {gameObject.name}");
    }
}