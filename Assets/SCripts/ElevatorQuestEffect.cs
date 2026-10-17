using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Listens for a specific QuestData completion via QuestManager, waits for a delay (e.g., 2s), 
/// then triggers the elevator descent, material flashing, audio, and UI texts.
/// </summary>
public class ElevatorQuestEffect : MonoBehaviour
{
    [Header("Quest Trigger Settings")]
    [Tooltip("The specific QuestData asset to listen for.")]
    public QuestData questToWatch;
    [Tooltip("How many seconds to wait after the quest completes before playing effects.")]
    public float delayBeforeStart = 2f; 

    [Header("Elevator Movement Settings")]
    public Transform elevatorMesh;
    public float descentDistance = 3.5f;
    public float descentSpeed = 2f;

    [Header("Audio Settings")]
    public AudioSource elevatorAudio;

    [Header("Material Flash Settings")]
    public MeshRenderer targetMeshRenderer;
    public Material unlitMaterial;
    public Material litMaterial;
    public int flashCount = 3;
    public float flashInterval = 0.15f;

    [Header("TextMeshPro Settings")]
    [Tooltip("Drag the GameObjects containing the TMP texts here.")]
    public GameObject[] textsToAppear;

    [Header("Scripts to Enable")]
    public MonoBehaviour[] scriptsToEnable;

    private bool hasTriggered = false;

    private void OnEnable()
    {
        // Safely subscribe to your specific QuestManager Action event
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks when this object is destroyed or disabled
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    /// <summary>
    /// Triggered automatically whenever ANY quest is completed by QuestManager.
    /// </summary>
    private void HandleQuestCompleted(QuestData completedQuest)
    {
        // Check if the completed quest is the one we are waiting for
        if (!hasTriggered && completedQuest == questToWatch)
        {
            Debug.Log($"[ElevatorEffect] Target quest '{completedQuest.questID}' completed. Starting {delayBeforeStart}s delay...");
            TriggerElevatorSequence();
        }
    }

    /// <summary>
    /// Starts the delayed routine. Can also be called via UnityEvents if preferred.
    /// </summary>
    public void TriggerElevatorSequence()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        StartCoroutine(DelayedElevatorRoutine());
    }

    private IEnumerator DelayedElevatorRoutine()
    {
        // 1. Wait for the specified delay (2 seconds)
        if (delayBeforeStart > 0)
        {
            yield return new WaitForSeconds(delayBeforeStart);
        }

        Debug.Log("[ElevatorEffect] Delay finished. Starting sequence...");

        // 2. Play audio immediately
        if (elevatorAudio != null) elevatorAudio.Play();

        // 3. Reveal TMP texts
        foreach (GameObject txtObj in textsToAppear)
        {
            if (txtObj != null) txtObj.SetActive(true);
        }

        // 4. Enable specific scripts
        foreach (MonoBehaviour script in scriptsToEnable)
        {
            if (script != null) script.enabled = true;
        }

        // 5. Start elevator descent (runs concurrently)
        if (elevatorMesh != null)
        {
            StartCoroutine(MoveElevatorDownRoutine());
        }

        // 6. Perform the material flickering effect
        if (targetMeshRenderer != null && unlitMaterial != null && litMaterial != null)
        {
            for (int i = 0; i < flashCount; i++)
            {
                targetMeshRenderer.material = litMaterial;
                yield return new WaitForSeconds(flashInterval);
                
                targetMeshRenderer.material = unlitMaterial;
                yield return new WaitForSeconds(flashInterval);
            }

            // Lock on lit material at the end
            targetMeshRenderer.material = litMaterial;
        }
    }

    /// <summary>
    /// Moves the elevator down smoothly.
    /// </summary>
    private IEnumerator MoveElevatorDownRoutine()
    {
        Vector3 startPos = elevatorMesh.position;
        Vector3 targetPos = startPos - new Vector3(0, descentDistance, 0);

        while (Vector3.Distance(elevatorMesh.position, targetPos) > 0.01f)
        {
            elevatorMesh.position = Vector3.MoveTowards(elevatorMesh.position, targetPos, descentSpeed * Time.deltaTime);
            yield return null;
        }

        elevatorMesh.position = targetPos;
        Debug.Log("[ElevatorEffect] Elevator descent complete.");
    }
}