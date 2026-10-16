using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Import New Input System
using TMPro;

/// <summary>
/// Manages the game flow, featuring a typewriter backspace effect for the Title Screen,
/// player freezing, and displaying the quest checklist upon game start.
/// Supports both PC Keyboard/Mouse and VR Controller button inputs.
/// </summary>
public class GameFlowController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Quest List container panel GameObject.")]
    public GameObject questListPanel;
    
    [Tooltip("The TextMeshPro component displaying the Title Screen text.")]
    public TextMeshProUGUI titleTextComponent;

    [Header("Animation Settings")]
    [Tooltip("Speed of the typewriter delete effect (seconds per character).")]
    public float deleteSpeed = 0.05f;

    [Header("Player Control References")]
    [Tooltip("Drag your Player movement script component here.")]
    public MonoBehaviour playerMovementScript;

    [Header("VR Input Mappings")]
    [Tooltip("Bind your VR controller action here (e.g., RightHand/UI Press or Trigger).")]
    public InputActionProperty vrInteractionAction;

    private bool isGameStarted = false;
    private string originalTitleText = "";

    private void OnEnable()
    {
        // Enable VR action listening
        if (vrInteractionAction.action != null)
        {
            vrInteractionAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        // Disable VR action listening
        if (vrInteractionAction.action != null)
        {
            vrInteractionAction.action.Disable();
        }
    }

    void Start()
    {
        // 1. Hide the quest list initially
        if (questListPanel != null)
        {
            questListPanel.SetActive(false);
        }

        // 2. Cache the original title text if assigned
        if (titleTextComponent != null)
        {
            originalTitleText = titleTextComponent.text;
        }

        // 3. Freeze player movement and interaction at startup
        FreezePlayer(true);
    }

    void Update()
    {
        // If the game has already started, no further input listening is required
        if (isGameStarted) return;

        // Check if VR controller button was pressed (e.g., Trigger / Primary Button)
        bool isVrButtonPressed = vrInteractionAction.action != null && vrInteractionAction.action.WasPressedThisFrame();

        // Check PC input for backward compatibility: Press 'Q' or Left Mouse Click
        bool isPcInput = Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0);

        // Title Screen state: Wait for VR Trigger / Mouse Click / 'Q' to start the game
        if (isVrButtonPressed || isPcInput)
        {
            StartCoroutine(StartGameWithEffect());
        }
    }

    /// <summary>
    /// Coroutine handling the typewriter delete effect for the title screen,
    /// followed by unfreezing the player and permanently displaying the quest list.
    /// </summary>
    private IEnumerator StartGameWithEffect()
    {
        isGameStarted = true;
        Debug.Log("[GameFlow] Game Started. Playing typewriter delete effect.");

        // 1. Typewriter delete effect (backspace character by character)
        if (titleTextComponent != null)
        {
            string currentText = titleTextComponent.text;
            while (currentText.Length > 0)
            {
                currentText = currentText.Substring(0, currentText.Length - 1);
                titleTextComponent.text = currentText;
                yield return new WaitForSeconds(deleteSpeed);
            }
            
            // Hide the title text object completely after deletion
            titleTextComponent.gameObject.SetActive(false);
        }

        // 2. Unfreeze player movement and raycasting
        FreezePlayer(false);

        // 3. Permanently activate the quest list panel
        if (questListPanel != null)
        {
            questListPanel.SetActive(true);
            Debug.Log("[GameFlow] Quest list panel enabled.");
        }
    }

    /// <summary>
    /// Freezes or unfreezes player movement, camera look, and interaction raycaster.
    /// </summary>
    private void FreezePlayer(bool freeze)
    {
        PCInteractionRaycaster raycaster = Camera.main.GetComponent<PCInteractionRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = !freeze;
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = !freeze;
        }
        
        Cursor.lockState = freeze ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = freeze;
    }
}