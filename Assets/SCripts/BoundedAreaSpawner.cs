using UnityEngine;

/// <summary>
/// Spawns a batch of objects procedurally strictly within the defined Gizmo bounds.
/// </summary>
public class BoundedAreaSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Drag your base Prefab here (e.g., rubber duck, pool ball).")]
    public GameObject objectPrefab;

    [Tooltip("Total number of objects to spawn.")]
    public int spawnCount = 50;

    [Tooltip("The total size of the spawn area bounding box.")]
    public Vector3 spawnAreaSize = new Vector3(3f, 2f, 3f);

    [Header("Material Palette")]
    [Tooltip("A list of Materials that the objects can randomly use.")]
    public Material[] randomMaterials;

    void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        if (objectPrefab == null)
        {
            Debug.LogError("[BoundedAreaSpawner] Object Prefab is not assigned!");
            return;
        }

        // Calculate half extents to match the Gizmo bounds strictly
        float halfX = spawnAreaSize.x / 2f;
        float halfZ = spawnAreaSize.z / 2f;

        for (int i = 0; i < spawnCount; i++)
        {
            // Spawn strictly within the bounds
            // X and Z go from -half to +half. Y goes from 0 to full height.
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-halfX, halfX),
                Random.Range(0f, spawnAreaSize.y),
                Random.Range(-halfZ, halfZ)
            );

            // Instantiate the object
            GameObject newObj = Instantiate(objectPrefab, randomPos, Random.rotation);

            // Assign a random material from the palette
            Renderer objRenderer = newObj.GetComponent<Renderer>();
            if (objRenderer != null && randomMaterials != null && randomMaterials.Length > 0)
            {
                Material randomMat = randomMaterials[Random.Range(0, randomMaterials.Length)];
                if (randomMat != null)
                {
                    objRenderer.material = randomMat;
                }
            }
        }

        Debug.Log($"[BoundedAreaSpawner] Successfully spawned {spawnCount} objects inside the restricted area!");
    }

    // Draw gizmos in the editor to visualize the exact spawn area
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        // The center of the cube is moved up by half its height so the bottom rests on the transform position
        Gizmos.DrawCube(transform.position + Vector3.up * (spawnAreaSize.y / 2f), spawnAreaSize);
    }
}