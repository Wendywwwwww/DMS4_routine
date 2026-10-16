using UnityEngine;

/// <summary>
/// Spawns a batch of pool balls procedurally and assigns random Materials to them.
/// </summary>
public class PoolBallSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Drag your base pool ball Prefab here.")]
    public GameObject ballPrefab;

    [Tooltip("Total number of balls to spawn.")]
    public int spawnCount = 50;

    [Tooltip("The spawn area spread size above the pool.")]
    public Vector3 spawnAreaSize = new Vector3(3f, 2f, 3f);

    [Header("Material Palette")]
    [Tooltip("A list of Materials that the balls can randomly use.")]
    public Material[] ballMaterials;

    void Start()
    {
        SpawnBalls();
    }

    void SpawnBalls()
    {
        if (ballPrefab == null)
        {
            Debug.LogError("[PoolBallSpawner] Ball Prefab is not assigned!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            // Calculate a random spawn position within the area
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
                Random.Range(0f, spawnAreaSize.y),
                Random.Range(-spawnAreaSize.z, spawnAreaSize.z)
            );

            // Instantiate the ball
            GameObject newBall = Instantiate(ballPrefab, randomPos, Random.rotation);

            // Assign a random material from the palette
            Renderer ballRenderer = newBall.GetComponent<Renderer>();
            if (ballRenderer != null && ballMaterials != null && ballMaterials.Length > 0)
            {
                Material randomMat = ballMaterials[Random.Range(0, ballMaterials.Length)];
                if (randomMat != null)
                {
                    ballRenderer.material = randomMat;
                }
            }
        }

        Debug.Log($"[PoolBallSpawner] Successfully spawned {spawnCount} material-varied pool balls!");
    }

    // Draw gizmos in the editor to visualize the spawn area
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawCube(transform.position + Vector3.up * (spawnAreaSize.y / 2f), spawnAreaSize);
    }
}