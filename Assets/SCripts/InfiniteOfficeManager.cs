using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteOfficeManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform of the player object.")]
    public Transform player;            
    [Tooltip("The main camera of the player.")]
    public Camera playerCamera;      

    [Header("Grid & Loading Settings")]
    [Tooltip("Actual physical side length of a single office chunk (in meters).")]
    public float chunkSize = 10f;    
    [Tooltip("Render distance: 2 means 5x5 grid around player, 3 means 7x7 grid.")]
    public int renderDistance = 3;    

    [Header("Prefab Presets")]
    [Tooltip("Normal cubicle Prefab.")]
    public GameObject normalCubiclePrefab;        
    [Tooltip("List of special cubicle Prefabs with 3D spatial audio.")]
    public List<GameObject> specialCubiclePrefabs; 

    [Header("Special Cubicle Generation Rules")]
    [Tooltip("Minimum number of grid steps the player must move before special cubicles are allowed.")]
    public int stepsBeforeSpecialAllowed = 12; 
    [Tooltip("Probability (0~1) for each new chunk to mutate into a special cubicle after meeting step conditions.")]
    [Range(0f, 1f)] public float specialChance = 0.3f; 
    [Tooltip("Cooldown steps after spawning a special cubicle before another one can spawn.")]
    public int specialCooldownSteps = 25;      

    // Internal state control
    private Vector2Int currentChunkCoord;
    private Dictionary<Vector2Int, GameObject> spawnedChunks = new Dictionary<Vector2Int, GameObject>();
    private int stepsSinceLastSpecial = 0;
    private bool hasSpecialInScene = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        // 1. Calculate starting chunk coordinate based on player's rough position
        Vector2Int startCoord = GetChunkCoordFromPosition(player.position);

        // 2. Precisely snap player to the exact geometric center of the starting chunk
        AlignPlayerToChunkCenter(startCoord);

        // 3. Update and record current chunk coordinate after snapping to avoid mis-refresh on the first frame due to position jump
        currentChunkCoord = startCoord;

        // 4. Generate initial map
        UpdateChunks(true);
    }

    void Update()
    {
        if (player == null) return;

        // Real-time calculation of the player's current grid coordinate
        Vector2Int newChunkCoord = GetChunkCoordFromPosition(player.position);

        // Update map when player crosses chunk boundaries
        if (newChunkCoord != currentChunkCoord)
        {
            currentChunkCoord = newChunkCoord;
            stepsSinceLastSpecial++;
            UpdateChunks(false);
        }
    }

    /// <summary>
    /// Core logic: Refresh chunks around the player (dynamic loading and destruction).
    /// </summary>
    void UpdateChunks(bool isFirstRun)
    {
        HashSet<Vector2Int> currentCoords = new HashSet<Vector2Int>();

        // 1. Collect chunk coordinates within renderDistance around the player
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                currentCoords.Add(new Vector2Int(currentChunkCoord.x + x, currentChunkCoord.y + z));
            }
        }

        // 2. Determine whether a special cubicle should spawn in this update
        Vector2Int candidateSpecialCoord = Vector2Int.zero;
        bool shouldSpawnSpecial = false;

        if (!hasSpecialInScene && stepsSinceLastSpecial >= stepsBeforeSpecialAllowed)
        {
            if (Random.value < specialChance && specialCubiclePrefabs.Count > 0)
            {
                // Pick a position among the edge chunks that is outside the player's view
                shouldSpawnSpecial = TryFindHiddenChunkCoord(currentCoords, out candidateSpecialCoord);
            }
        }

        // 3. Generate new chunks
        foreach (var coord in currentCoords)
        {
            if (!spawnedChunks.ContainsKey(coord))
            {
                GameObject prefabToSpawn = normalCubiclePrefab;

                // If this coordinate is selected as the special cubicle coordinate
                if (shouldSpawnSpecial && coord == candidateSpecialCoord)
                {
                    int randomIndex = Random.Range(0, specialCubiclePrefabs.Count);
                    prefabToSpawn = specialCubiclePrefabs[randomIndex];
                    hasSpecialInScene = true;
                    stepsSinceLastSpecial = -specialCooldownSteps; // Enter cooldown period
                }

                SpawnChunkAt(coord, prefabToSpawn);
            }
        }

        // 4. Recycle and destroy old chunks that are out of the render range
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var item in spawnedChunks)
        {
            if (!currentCoords.Contains(item.Key))
            {
                toRemove.Add(item.Key);
            }
        }

        foreach (var coord in toRemove)
        {
            // If the recycled chunk is a special cubicle, reset the scene flag
            if (spawnedChunks[coord] != null && spawnedChunks[coord].name.Contains("Special"))
            {
                hasSpecialInScene = false;
            }

            Destroy(spawnedChunks[coord]);
            spawnedChunks.Remove(coord);
        }
    }

    /// <summary>
    /// Instantiate Prefab at the specified chunk position.
    /// </summary>
    void SpawnChunkAt(Vector2Int coord, GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 spawnPos = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        GameObject newChunk = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        
        // Differentiate naming for easier debugging and detection
        if (prefab != normalCubiclePrefab)
        {
            newChunk.name = $"SpecialChunk_({coord.x},{coord.y})";
            // ================== Debug Log Notification ==================
            Debug.Log($"Special Cubicle Spawned! Grid Coordinate: {coord}, World Position: {spawnPos}");
            // ============================================================
        }
        else
        {
            newChunk.name = $"Chunk_({coord.x},{coord.y})";
        }

        spawnedChunks.Add(coord, newChunk);
    }

    
    /// <summary>
    /// Find edge new chunk coordinates (fallback mechanism with added view restriction).
    /// </summary>
    bool TryFindHiddenChunkCoord(HashSet<Vector2Int> currentCoords, out Vector2Int hiddenCoord)
    {
        List<Vector2Int> hiddenEmptyCoords = new List<Vector2Int>();
        List<Vector2Int> allEmptyCoords = new List<Vector2Int>();

        foreach (var coord in currentCoords)
        {
            int dx = Mathf.Abs(coord.x - currentChunkCoord.x);
            int dz = Mathf.Abs(coord.y - currentChunkCoord.y);

            // Condition 1: Must be on the outermost ring
            // Condition 2: Must be a brand new chunk (avoid overriding old map)
            if ((dx == renderDistance || dz == renderDistance) && !spawnedChunks.ContainsKey(coord))
            {
                allEmptyCoords.Add(coord); // Record all available new empty spots

                Vector3 worldPos = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
                
                // Check if the point is within the camera's view
                if (!IsInCameraView(worldPos))
                {
                    hiddenEmptyCoords.Add(coord); // Record empty spots outside the view
                }
            }
        }

        // First choice: Prioritize empty spots completely out of view (perfect hiding)
        if (hiddenEmptyCoords.Count > 0)
        {
            hiddenCoord = hiddenEmptyCoords[Random.Range(0, hiddenEmptyCoords.Count)];
            return true;
        }

        // Second choice (fallback fix): If all new empty spots are in the player's view (e.g., player is staring straight ahead and walking)
        // Forcefully pick a random one ahead to spawn! Since the distance is far enough, with environmental fog/walls blocking, the player will barely notice.
        if (allEmptyCoords.Count > 0)
        {
            Debug.Log("Player staring straight ahead, triggering forced fallback spawn.");
            hiddenCoord = allEmptyCoords[Random.Range(0, allEmptyCoords.Count)];
            return true;
        }

        hiddenCoord = Vector2Int.zero;
        return false;
    }
    
    /// <summary>
    /// Determine whether a given world coordinate point is within the current camera view (with a safe buffer).
    /// </summary>
    bool IsInCameraView(Vector3 worldPos)
    {
        if (playerCamera == null) return false;

        Vector3 screenPoint = playerCamera.WorldToViewportPoint(worldPos);

        // Expand the boundary range (-0.4 ~ 1.4) to ensure it's well beyond screen edges to be considered "unseen"
        bool inView = screenPoint.z > 0 && 
                      screenPoint.x >= -0.4f && screenPoint.x <= 1.4f && 
                      screenPoint.y >= -0.4f && screenPoint.y <= 1.4f;

        return inView;
    }

    /// <summary>
    /// Precise chunk coordinate conversion algorithm (using Mathf.RoundToInt to eliminate half-Chunk offset deviations).
    /// </summary>
    Vector2Int GetChunkCoordFromPosition(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(pos.x / chunkSize),
            Mathf.RoundToInt(pos.z / chunkSize)
        );
    }

    /// <summary>
    /// Forcefully snap and precisely align the player to the absolute physical center of the specified chunk.
    /// </summary>
    void AlignPlayerToChunkCenter(Vector2Int coord)
    {
        Vector3 targetPos = new Vector3(coord.x * chunkSize, player.position.y, coord.y * chunkSize);

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        player.position = targetPos;

        if (controller != null) controller.enabled = true;
    }
}