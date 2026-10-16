using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalController : MonoBehaviour
{
    [Header("Scene & Door Settings")]
    public string targetSceneName = "Scene3";             // Target scene to load and look into
    public Transform currentDoor;                         // The door in Scene2
    public float loadTriggerDistance = 10f;               // Distance to start loading Scene3 asynchronously

    [Header("Hierarchy Names in Target Scene")]
    public string targetDoorObjectName = "TargetDoor";     // The exact name of the door object in Scene3
    public string portalCameraObjectName = "PortalCamera"; // The exact name of the camera object in Scene3

    [Header("Rendering")]
    public RenderTexture portalTexture;                   // The render texture used for the door surface
    public MeshRenderer doorScreenRenderer;               // The quad/material renderer on the Scene2 door

    private Transform targetDoor;                         // Auto-found target door
    private Camera portalCamera;                          // Auto-found portal camera
    private Transform playerTransform;                    // Reference to the player's transform
    private Transform playerCam;                          // Reference to the player's main camera
    private bool isSceneLoading = false;
    private bool isSceneLoaded = false;

    void Start()
    {
        if (Camera.main != null)
        {
            playerCam = Camera.main.transform;
            playerTransform = playerCam.root; // Assume player root is nearby
        }

        // Initialize the RenderTexture if not assigned in the inspector
        if (portalTexture == null)
        {
            portalTexture = new RenderTexture(Screen.width, Screen.height, 24);
            portalTexture.antiAliasing = 4;
        }

        if (doorScreenRenderer != null)
        {
            doorScreenRenderer.material.mainTexture = portalTexture;
        }

        // Ensure portal camera is hidden initially
        if (portalCamera != null)
        {
            portalCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Find player if not assigned yet
        if (playerTransform == null && Camera.main != null)
        {
            playerCam = Camera.main.transform;
            playerTransform = playerCam.root;
            return;
        }

        if (playerTransform == null || currentDoor == null) return;

        // Check distance to door. If player gets close, start loading Scene3 asynchronously.
        float distance = Vector3.Distance(playerTransform.position, currentDoor.position);
        if (distance <= loadTriggerDistance && !isSceneLoading && !isSceneLoaded)
        {
            StartCoroutine(LoadTargetSceneAsync());
        }

        // If Scene3 is loaded, handle camera sync in LateUpdate logic
        if (isSceneLoaded)
        {
            UpdatePortalCamera();
        }
    }

    private System.Collections.IEnumerator LoadTargetSceneAsync()
    {
        isSceneLoading = true;
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        
        if (!targetScene.isLoaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        // Find references in Scene3 after loading
        FindTargetSceneReferences();
        isSceneLoaded = true;
    }

    private void FindTargetSceneReferences()
    {
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        if (targetScene.IsValid())
        {
            GameObject[] rootObjects = targetScene.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                if (rootObj.name == targetDoorObjectName)
                {
                    targetDoor = rootObj.transform;
                }
                Transform foundDoor = rootObj.transform.Find(targetDoorObjectName);
                if (foundDoor != null) targetDoor = foundDoor;

                Transform foundCam = rootObj.transform.Find(portalCameraObjectName);
                if (foundCam != null)
                {
                    portalCamera = foundCam.GetComponent<Camera>();
                }
                if (rootObj.name == portalCameraObjectName)
                {
                    portalCamera = rootObj.GetComponent<Camera>();
                }
            }

            if (portalCamera != null && portalTexture != null)
            {
                portalCamera.targetTexture = portalTexture;
                portalCamera.gameObject.SetActive(true);
            }
        }
    }

    void UpdatePortalCamera()
    {
        if (playerCam == null || targetDoor == null || portalCamera == null || currentDoor == null)
        {
            if (portalCamera != null && portalCamera.gameObject.activeSelf)
                portalCamera.gameObject.SetActive(false);
            return;
        }

        if (!portalCamera.gameObject.activeSelf)
            portalCamera.gameObject.SetActive(true);

        // --- Core Algorithm: Map player's relative position from Scene2 door to Scene3 target door ---
        Matrix4x4 matrix = targetDoor.localToWorldMatrix * currentDoor.worldToLocalMatrix * playerCam.localToWorldMatrix;
        portalCamera.transform.SetPositionAndRotation(matrix.GetColumn(3), matrix.rotation);
    }
}