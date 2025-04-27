using UnityEngine;
using System.Linq;

public class SpawnObjects : MonoBehaviour
{
    [System.Serializable]
    public class PrefabSettings
    {
        public GameObject prefab;
        public int priority = 1;
        public int maxSpawnCount = 0;
        [HideInInspector]
        public int currentSpawnCount = 0;
    }
    
    [Header("Spawn Settings")]
    public float cardSize = 1f;
    public PrefabSettings[] cardPrefabSettings;
    
    [Header("Spawn Count Range")]
    [Min(0)]
    public int minSpawnCount = 10;
    [Min(0)]
    public int maxSpawnCount = 30;

    [Header("Debug Settings")]
    public bool debugMode = false;

    [Header("Color Settings")]
    public bool useRandomColors = true;
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
    public float screenWidthPercentage = 0.8f;
    [Range(0f, 1f)]
    public float screenHeightPercentage = 0.8f;
    
    [Header("Rotation Settings")]
    public float minRotation = -180f;
    public float maxRotation = 180f;
    
    [Header("Border Settings")]
    public GameObject borderPrefab;
    public float borderThickness = 0.1f;
    public GameObject boardPrefab;
    
    private float screenWidth;
    private float screenHeight;
    private Camera mainCamera;

    private void Start()
    {
        ValidateSettings();
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene!");
            return;
        }
        
        CalculateSpawnBoundaries();
        ResetSpawnCounts();
        SpawnBorders();
        SpawnInitialObjects();
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        float height = Camera.main.orthographicSize * 2f * screenHeightPercentage;
        float width = height * (16f / 9f) * screenWidthPercentage;

        Gizmos.color = Color.white;
        Vector3 topLeft = transform.position + new Vector3(-width/2f, height/2f, 0);
        Vector3 topRight = transform.position + new Vector3(width/2f, height/2f, 0);
        Vector3 bottomLeft = transform.position + new Vector3(-width/2f, -height/2f, 0);
        Vector3 bottomRight = transform.position + new Vector3(width/2f, -height/2f, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, bottomRight);
        Gizmos.DrawLine(topRight, bottomLeft);
    }

    private void ValidateSettings()
    {
        if (minSpawnCount > maxSpawnCount)
        {
            minSpawnCount = maxSpawnCount;
        }
        
        if (predefinedColors == null || predefinedColors.Length == 0)
        {
            predefinedColors = new Color[] { Color.red };
        }
        
        if (cardPrefabSettings != null)
        {
            foreach (var settings in cardPrefabSettings)
            {
                if (settings.priority <= 0)
                {
                    settings.priority = 1;
                }
            }
        }
    }

    private void CalculateSpawnBoundaries()
    {
        float screenAspect = 16f / 9f;
        screenHeight = mainCamera.orthographicSize * 2f * screenHeightPercentage;
        screenWidth = screenHeight * screenAspect * screenWidthPercentage;
    }

    private void ResetSpawnCounts()
    {
        foreach (var settings in cardPrefabSettings)
        {
            settings.currentSpawnCount = 0;
        }
    }

    private void SpawnInitialObjects()
    {
        int objectsToSpawn = Random.Range(minSpawnCount, maxSpawnCount + 1);
        for (int i = 0; i < objectsToSpawn; i++)
        {
            SpawnRandomObject();
        }

        // Spawn special card in top left corner
        SpawnSpecialCard();
    }

    private void SpawnSpecialCard()
    {
        if (cardPrefabSettings == null || cardPrefabSettings.Length == 0)
            return;

        // Get a random card prefab
        GameObject cardPrefab = cardPrefabSettings[Random.Range(0, cardPrefabSettings.Length)].prefab;
        if (cardPrefab == null) return;

        // Calculate position at top left of spawn area
        float xPos = -screenWidth/2f;
        float yPos = screenHeight/2f;
        
        // Spawn the card
        GameObject specialCard = Instantiate(cardPrefab, new Vector3(xPos, yPos, 0), Quaternion.Euler(0, 0, 45));
        // Scale the special card based on cardSize
        specialCard.transform.localScale = new Vector3(cardSize, cardSize, 1f);
        
        // Apply random color if enabled
        if (useRandomColors && predefinedColors.Length > 0)
        {
            Color randomColor = predefinedColors[Random.Range(0, predefinedColors.Length)];
            ApplyColorToAllSprites(specialCard, randomColor);
        }
    }

    private GameObject SelectPrefabBasedOnPriority()
    {
        if (cardPrefabSettings == null || cardPrefabSettings.Length == 0)
            return null;

        float totalPriority = 0;
        foreach (var settings in cardPrefabSettings)
        {
            if (settings.prefab == null) continue;
            if (settings.maxSpawnCount == 0 || settings.currentSpawnCount < settings.maxSpawnCount)
            {
                totalPriority += settings.priority;
            }
        }

        if (totalPriority <= 0) return null;

        float randomValue = Random.Range(0, totalPriority);
        float currentSum = 0;

        foreach (var settings in cardPrefabSettings)
        {
            if (settings.prefab == null) continue;
            if (settings.maxSpawnCount > 0 && settings.currentSpawnCount >= settings.maxSpawnCount) continue;

            currentSum += settings.priority;
            if (randomValue <= currentSum)
            {
                settings.currentSpawnCount++;
                return settings.prefab;
            }
        }

        foreach (var settings in cardPrefabSettings)
        {
            if (settings.prefab != null && (settings.maxSpawnCount == 0 || settings.currentSpawnCount < settings.maxSpawnCount))
            {
                settings.currentSpawnCount++;
                return settings.prefab;
            }
        }
        
        return null;
    }

    public void SpawnRandomObject()
    {
        GameObject prefab = SelectPrefabBasedOnPriority();
        if (prefab == null) return;

        float randomX = Random.Range(-screenWidth/2f, screenWidth/2f);
        float randomY = Random.Range(-screenHeight/2f, screenHeight/2f);
        float randomZ = Random.Range(-0.1f, 0f);
        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);
        
        GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        // Scale the card based on cardSize
        spawnedObject.transform.localScale = new Vector3(cardSize, cardSize, 1f);
        spawnedObject.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(minRotation, maxRotation));
        
        if (useRandomColors && predefinedColors.Length > 0)
        {
            Color randomColor = predefinedColors[Random.Range(0, predefinedColors.Length)];
            ApplyColorToAllSprites(spawnedObject, randomColor);
        }
    }

    private void ApplyColorToAllSprites(GameObject obj, Color color)
    {
        SpriteRenderer[] spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = color;
        }
    }

    private void SpawnBorders()
    {
        if (borderPrefab == null)
        {
            Debug.LogWarning("No border prefab assigned!");
            return;
        }

        // Spawn the board first (so it's behind the borders)
        if (boardPrefab != null)
        {
            GameObject board = Instantiate(boardPrefab, transform);
            // Board should extend by border thickness and card size
            board.transform.localScale = new Vector3(screenWidth + borderThickness + cardSize * 2, screenHeight + borderThickness + cardSize * 2, 1f);
            board.transform.localPosition = Vector3.zero;
            // Make board non-clickable
            board.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        // Create top border
        GameObject topBorder = Instantiate(borderPrefab, transform);
        // Extend border to meet corners and add card size
        topBorder.transform.localScale = new Vector3(screenWidth + borderThickness * 3 + cardSize * 2, borderThickness, 1f);
        topBorder.transform.localPosition = new Vector3(0, screenHeight/2f + borderThickness + cardSize, 0);

        // Create bottom border
        GameObject bottomBorder = Instantiate(borderPrefab, transform);
        bottomBorder.transform.localScale = new Vector3(screenWidth + borderThickness * 3 + cardSize * 2, borderThickness, 1f);
        bottomBorder.transform.localPosition = new Vector3(0, -screenHeight/2f - borderThickness - cardSize, 0);

        // Create left border
        GameObject leftBorder = Instantiate(borderPrefab, transform);
        leftBorder.transform.localScale = new Vector3(borderThickness, screenHeight + borderThickness * 3 + cardSize * 2, 1f);
        leftBorder.transform.localPosition = new Vector3(-screenWidth/2f - borderThickness - cardSize, 0, 0);

        // Create right border
        GameObject rightBorder = Instantiate(borderPrefab, transform);
        rightBorder.transform.localScale = new Vector3(borderThickness, screenHeight + borderThickness * 3 + cardSize * 2, 1f);
        rightBorder.transform.localPosition = new Vector3(screenWidth/2f + borderThickness + cardSize, 0, 0);

        // Make borders non-clickable
        foreach (Transform child in transform)
        {
            if (child.gameObject.layer != LayerMask.NameToLayer("UI"))
            {
                child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
    }
}
