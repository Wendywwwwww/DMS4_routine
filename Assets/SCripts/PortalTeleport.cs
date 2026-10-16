using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTeleport : MonoBehaviour
{
    [Header("Destination Settings")]
    public string targetSceneName = "Scene3";           // Target scene name
    public string targetDoorObjectName = "TargetDoor";    // Name of the door object in Scene3

    private Transform targetDoor;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Find target door dynamically if not yet found
            if (targetDoor == null)
            {
                FindTargetDoor();
            }

            if (targetDoor == null)
            {
                Debug.LogWarning("Target door not found in Scene3 yet!");
                return;
            }

            Transform player = other.transform;
            
            // Handle CharacterController if present to avoid positioning conflicts
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 1. Calculate relative local position and rotation from the current door
            Vector3 localPos = transform.InverseTransformPoint(player.position);
            Quaternion localRot = Quaternion.Inverse(transform.rotation) * player.rotation;

            // 2. Teleport player smoothly to Scene3's target door coordinate system
            player.position = targetDoor.TransformPoint(localPos);
            player.rotation = targetDoor.rotation * localRot;

            if (cc != null) cc.enabled = true;

            // 3. Set Scene3 as the active scene
            Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
            if (targetScene.IsValid())
            {
                SceneManager.SetActiveScene(targetScene);
            }

            // 4. Unload Scene2 to complete the seamless transition
            string currentSceneName = gameObject.scene.name;
            SceneManager.UnloadSceneAsync(currentSceneName);
        }
    }

    private void FindTargetDoor()
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
                    break;
                }
                Transform found = rootObj.transform.Find(targetDoorObjectName);
                if (found != null)
                {
                    targetDoor = found;
                    break;
                }
            }
        }
    }
}