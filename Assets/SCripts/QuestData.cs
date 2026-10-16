using UnityEngine;

/// <summary>
/// A data container for quests using ScriptableObject.
/// Allows designers to create and edit quests directly in the Project window without touching code.
/// Supports multiple prerequisite quests.
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Systems/Quest Data")]
public class QuestData : ScriptableObject
{
    [Tooltip("A unique identifier for the quest (e.g., 'master_cook_egg').")]
    public string questID;

    [Tooltip("The text description that will appear on the UI Quest List.")]
    [TextArea]
    public string description;

    [Header("Prerequisites")]
    [Tooltip("All of these quests MUST be completed before this quest becomes active/available.")]
    public QuestData[] prerequisiteQuests;

    [Header("Audio Settings")]
    [Tooltip("Optional sound effect to play when this quest is completed. Leave empty for no sound.")]
    public AudioClip completionSound;
}