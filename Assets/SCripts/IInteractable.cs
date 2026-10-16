using UnityEngine;

/// <summary>
/// A standardized interface for all interactive objects in the game.
/// Ensures seamless transition between PC (Raycast) and VR (XR Interaction Toolkit) inputs.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Triggered when the player's gaze or VR raycast points at the object.
    /// Primarily used for visual feedback, such as highlighting or outlining.
    /// </summary>
    void OnHoverEnter();

    /// <summary>
    /// Triggered when the player's gaze or VR raycast moves away from the object.
    /// Used to remove visual feedback and restore the object to its default state.
    /// </summary>
    void OnHoverExit();

    /// <summary>
    /// Triggered when the player confirms an interaction (e.g., pressing 'E' on PC or pulling the VR trigger).
    /// Used to execute core logic, such as updating quest states, opening doors, or picking up items.
    /// </summary>
    void OnInteract();
}