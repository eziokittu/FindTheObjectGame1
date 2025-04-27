using UnityEngine;
using System.Linq;

public class SpawnObjects : MonoBehaviour
{
    [System.Serializable]
    public class PrefabSettings
    {
        public GameObject prefab;
        [Tooltip("Higher priority means higher chance of spawning")]
        public int priority = 1;
        [Tooltip("Maximum number of times this prefab can spawn (0 = unlimited)")]
        public int maxSpawnCount = 0;
        [HideInInspector]
        public int currentSpawnCount = 0;
    }
    
    [Header("Spawn Settings")]
    public PrefabSettings[] prefabSettings;  // Array of prefabs with their settings
    
    [Header("Spawn Count Range")]
    [Tooltip("Minimum number of objects to spawn")]
    [Min(0)]
    public int minSpawnCount = 10;
    [Tooltip("Maximum number of objects to spawn")]
    [Min(0)]
    public int maxSpawnCount = 30;

    [Header("Color Settings")]
    public bool useRandomColors = true;  // Toggle for random colors
    [Tooltip("List of colors to randomly choose from")]
    public Color[] predefinedColors = new Color[]
    {
        new Color(1f, 0f, 0f),      // Red
        new Color(0f, 1f, 0f),      // Green
        new Color(0f, 0f, 1f),      // Blue
        new Color(1f, 1f, 0f),      // Yellow
        new Color(1f, 0f, 1f),      // Magenta
        new Color(0f, 1f, 1f),      // Cyan
        new Color(1f, 0.5f, 0f),    // Orange
        new Color(0.5f, 0f, 1f),    // Purple
        new Color(0f, 0.5f, 0f),    // Dark Green
        new Color(0.5f, 0.25f, 0f)  // Brown
    };
    
    [Header("Boundary Settings")]
    [Range(0f, 1f)]
    public float screenWidthPercentage = 0.8f;  // Default 80% of screen width
    [Range(0f, 1f)]
    public float screenHeightPercentage = 0.8f;  // Default 80% of screen height
    
    [Header("Rotation Settings")]
    public float minRotation = -180f;
    public float maxRotation = 180f;
    
    private float screenWidth;
    private float screenHeight;
    private Camera mainCamera;

    private void Start()
    {
        // Validate settings
        ValidateSettings();
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene! Please tag a camera as MainCamera.");
            return;
        }
        
        CalculateSpawnBoundaries();
        
        // Reset spawn counts
        foreach (var settings in prefabSettings)
        {
            settings.currentSpawnCount = 0;
        }
        
        // Spawn random number of objects within range
        int objectsToSpawn = Random.Range(minSpawnCount, maxSpawnCount + 1);
        for (int i = 0; i < objectsToSpawn; i++)
        {
            SpawnRandomObject();
        }
    }

    private void OnDrawGizmos()
    {
        // Only draw if we have a main camera
        if (Camera.main == null) return;

        // Calculate boundaries for gizmos
        float height = Camera.main.orthographicSize * 2f * screenHeightPercentage;
        float width = height * (16f / 9f) * screenWidthPercentage;

        // Set gizmo color to white
        Gizmos.color = Color.white;

        // Calculate the four corners of the spawn area
        Vector3 topLeft = transform.position + new Vector3(-width/2f, height/2f, 0);
        Vector3 topRight = transform.position + new Vector3(width/2f, height/2f, 0);
        Vector3 bottomLeft = transform.position + new Vector3(-width/2f, -height/2f, 0);
        Vector3 bottomRight = transform.position + new Vector3(width/2f, -height/2f, 0);

        // Draw the rectangle
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // Draw diagonal crosses for better visibility
        Gizmos.DrawLine(topLeft, bottomRight);
        Gizmos.DrawLine(topRight, bottomLeft);
    }

    private void ValidateSettings()
    {
        // Ensure min doesn't exceed max
        if (minSpawnCount > maxSpawnCount)
        {
            Debug.LogWarning("Min spawn count exceeds max spawn count. Setting min equal to max.");
            minSpawnCount = maxSpawnCount;
        }
        
        // Ensure we have colors
        if (predefinedColors == null || predefinedColors.Length == 0)
        {
            Debug.LogWarning("No colors defined. Adding default red color.");
            predefinedColors = new Color[] { Color.red };
        }
        
        // Ensure all prefabs have positive priority
        if (prefabSettings != null)
        {
            foreach (var settings in prefabSettings)
            {
                if (settings.priority <= 0)
                {
                    Debug.LogWarning("Prefab has zero or negative priority. Setting to 1.");
                    settings.priority = 1;
                }
            }
        }
    }

    private void CalculateSpawnBoundaries()
    {
        // Calculate screen boundaries in world coordinates
        float screenAspect = 16f / 9f;  // Target aspect ratio
        
        // Get screen height first
        screenHeight = mainCamera.orthographicSize * 2f * screenHeightPercentage;
        // Calculate width based on 16:9 ratio
        screenWidth = screenHeight * screenAspect * screenWidthPercentage;
    }

    private GameObject SelectPrefabBasedOnPriority()
    {
        if (prefabSettings == null || prefabSettings.Length == 0)
            return null;

        // Calculate total priority of available prefabs
        float totalPriority = 0;
        foreach (var settings in prefabSettings)
        {
            // Skip null prefabs
            if (settings.prefab == null)
                continue;
                
            // Only include prefabs that haven't reached their spawn limit
            if (settings.maxSpawnCount == 0 || settings.currentSpawnCount < settings.maxSpawnCount)
            {
                totalPriority += settings.priority;
            }
        }

        if (totalPriority <= 0)
            return null;

        // Generate random value between 0 and total priority
        float randomValue = Random.Range(0, totalPriority);
        float currentSum = 0;

        // Select prefab based on priority
        foreach (var settings in prefabSettings)
        {
            // Skip null prefabs
            if (settings.prefab == null)
                continue;
                
            // Skip if max spawn count reached
            if (settings.maxSpawnCount > 0 && settings.currentSpawnCount >= settings.maxSpawnCount)
                continue;

            currentSum += settings.priority;
            if (randomValue <= currentSum)
            {
                settings.currentSpawnCount++;
                return settings.prefab;
            }
        }

        // Find first valid prefab as fallback
        foreach (var settings in prefabSettings)
        {
            if (settings.prefab != null && 
                (settings.maxSpawnCount == 0 || settings.currentSpawnCount < settings.maxSpawnCount))
            {
                settings.currentSpawnCount++;
                return settings.prefab;
            }
        }
        
        return null; // No valid prefabs found
    }

    public void SpawnRandomObject()
    {
        GameObject prefab = SelectPrefabBasedOnPriority();
        if (prefab == null)
        {
            Debug.LogWarning("No prefabs available to spawn!");
            return;
        }

        // Calculate random position within boundaries
        float randomX = Random.Range(-screenWidth/2f, screenWidth/2f);
        float randomY = Random.Range(-screenHeight/2f, screenHeight/2f);
        Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);
        
        // Create the object
        GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Apply random rotation
        float randomRotation = Random.Range(minRotation, maxRotation);
        spawnedObject.transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);
        
        // Apply random color if enabled
        if (useRandomColors && predefinedColors.Length > 0)
        {
            // Get a random color once to apply to all sprites
            Color randomColor = predefinedColors[Random.Range(0, predefinedColors.Length)];
            ApplyColorToAllSprites(spawnedObject, randomColor);
        }
    }

    private void ApplyColorToAllSprites(GameObject obj, Color color)
    {
        // Get all SpriteRenderer components in this object and all its children
        SpriteRenderer[] spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        
        // Apply the color to each sprite renderer found
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = color;
        }
    }
}
