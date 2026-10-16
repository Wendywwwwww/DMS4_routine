using UnityEngine;

/// <summary>
/// Automatically listens to the QuestManager and plays a 3D spatial audio effect 
/// from this object's AudioSource when its designated quest is completed.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class QuestAudioPlayer : MonoBehaviour
{
    [Header("Target Quest")]
    [Tooltip("The QuestData asset associated with this audio source. Plays sound when this specific quest completes.")]
    public QuestData targetQuest;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Subscribe to the QuestManager completion event
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    /// <summary>
    /// Event handler triggered when any quest is completed.
    /// </summary>
    private void HandleQuestCompleted(QuestData completedQuest)
    {
        // Check if the completed quest matches our target quest and has a valid audio clip
        if (completedQuest == targetQuest && targetQuest != null && targetQuest.completionSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(targetQuest.completionSound);
                Debug.Log($"[QuestAudioPlayer] Quest '{targetQuest.questID}' completed. Playing 3D audio from '{gameObject.name}'.");
            }
        }
    }
}