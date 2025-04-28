using UnityEngine;

public class SpawnCards : MonoBehaviour
{
    private float screenWidth;
    private float screenHeight;
    private float cardSize;
    private int minSpawnCount;
    private int maxSpawnCount;
    private float minRotation;
    private float maxRotation;
    private bool useRandomColors;
    private Color[] predefinedColors;
    private GameManager.PrefabSettings[] cardPrefabSettings;
    private bool debugMode;
    private void Start()
    {
        // Wait a frame to ensure GameManager has initialized
        Invoke("Initialize", 0.1f);
    }

    private void Initialize()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // Get values from GameManager
        screenWidth = gameManager.GetScreenWidth();
        screenHeight = gameManager.GetScreenHeight();
        cardSize = gameManager.cardSize;
        minSpawnCount = gameManager.minSpawnCount;
        maxSpawnCount = gameManager.maxSpawnCount;
        minRotation = gameManager.minRotation;
        maxRotation = gameManager.maxRotation;
        useRandomColors = gameManager.useRandomColors;
        predefinedColors = gameManager.predefinedColors;
        cardPrefabSettings = gameManager.cardPrefabSettings;
        debugMode = gameManager.debugMode;

        // After initialization, spawn the cards
        SpawnInitialCards();
    }

    private void SpawnInitialCards()
    {
        if (cardPrefabSettings == null || cardPrefabSettings.Length == 0)
        {
            Debug.LogWarning("No card prefabs assigned in GameManager!");
            return;
        }
        
        // Reset spawn counts
        foreach (var settings in cardPrefabSettings)
        {
            settings.currentSpawnCount = 0;
        }
        
        int cardsToSpawn = Random.Range(minSpawnCount, maxSpawnCount + 1);
        for (int i = 0; i < cardsToSpawn; i++)
        {
            SpawnRandomCard();
        }

        // Spawn special card in top left corner
        if (debugMode) {
            SpawnSpecialCard();
        }
    }

    private void SpawnSpecialCard()
    {
        if (cardPrefabSettings == null || cardPrefabSettings.Length == 0)
            return;

        // Get a random card prefab
        GameObject cardPrefab = cardPrefabSettings[0].prefab;
        if (cardPrefab == null) return;

        // Calculate position at the exact top left of spawn area
        // The card should be centered on the top-left corner
        float xPos = -screenWidth/2f;
        float yPos = screenHeight/2f;
        
        SpawnCardAtPosition(cardPrefab, new Vector3(xPos, yPos, -0.5f), 45f);
    }

    public void SpawnRandomCard()
    {
        GameObject prefab = SelectPrefabBasedOnPriority();
        if (prefab == null) return;

        // Card spawn should always stay within the spawn area (not the extended board area)
        // Adjust the spawn area to account for card size to keep cards fully within the spawn area
        float cardHalfSize = cardSize/2;
        float randomX = Random.Range(-screenWidth/2f , screenWidth/2f );
        float randomY = Random.Range(-screenHeight/2f , screenHeight/2f );
        float randomZ = Random.Range(-0.1f, 0f);
        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);
        
        float rotation = Random.Range(minRotation, maxRotation);
        SpawnCardAtPosition(prefab, spawnPosition, rotation);
    }
    
    // Method to spawn a specific card at a specific position
    public GameObject SpawnCardAtPosition(GameObject cardPrefab, Vector3 position, float rotation = 0f)
    {
        if (cardPrefab == null) return null;
        
        GameObject spawnedCard = Instantiate(cardPrefab, position, Quaternion.Euler(0f, 0f, rotation));
        // Scale the card based on cardSize
        spawnedCard.transform.localScale = new Vector3(cardSize, cardSize, 1f);
        
        // Apply random color if enabled
        if (useRandomColors && predefinedColors.Length > 0)
        {
            Color randomColor = predefinedColors[Random.Range(0, predefinedColors.Length)];
            GameManager.Instance.ApplyColorToSprites(spawnedCard, randomColor);
        }
        
        return spawnedCard;
    }
    
    // Method to spawn a card with a specific color
    public GameObject SpawnCardWithColor(GameObject cardPrefab, Vector3 position, Color color, float rotation = 0f)
    {
        if (cardPrefab == null) return null;
        
        GameObject spawnedCard = Instantiate(cardPrefab, position, Quaternion.Euler(0f, 0f, rotation));
        spawnedCard.transform.localScale = new Vector3(cardSize, cardSize, 1f);
        GameManager.Instance.ApplyColorToSprites(spawnedCard, color);
        
        return spawnedCard;
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
    
    // Method to get a random card prefab
    public GameObject GetRandomCardPrefab()
    {
        if (cardPrefabSettings == null || cardPrefabSettings.Length == 0)
            return null;
            
        return cardPrefabSettings[Random.Range(0, cardPrefabSettings.Length)].prefab;
    }
    
    // OnDrawGizmosSelected ensures the gizmos are only drawn when this object is selected in the editor
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        // We now draw in both editor and play mode, but only in editor view
        if (Camera.main == null) return;
        
        GameManager gameManager = GameManager.Instance;
        
        // During play mode, use cached values if available
        float width, height, cSize, extraMargin;
        if (Application.isPlaying && screenWidth > 0 && screenHeight > 0)
        {
            width = screenWidth;
            height = screenHeight;
            cSize = cardSize;
        }
        else
        {
            // In edit mode or if values aren't cached, get from GameManager
            if (gameManager == null) return;
            
            float heightPercentage = gameManager.screenHeightPercentage;
            float widthPercentage = gameManager.screenWidthPercentage;
            
            height = Camera.main.orthographicSize * 2f * heightPercentage;
            width = height * (16f / 9f) * widthPercentage;
            cSize = gameManager.cardSize;
        }
        
        // Extra margin (3/4 of card size) for extended board
        extraMargin = cSize * 0.75f;
        
        // Draw spawn area in a different color
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        Vector3 topLeft = transform.position + new Vector3(-width/2f, height/2f, 0);
        Vector3 topRight = transform.position + new Vector3(width/2f, height/2f, 0);
        Vector3 bottomLeft = transform.position + new Vector3(-width/2f, -height/2f, 0);
        Vector3 bottomRight = transform.position + new Vector3(width/2f, -height/2f, 0);
        
        // Draw a rectangle representing the spawn area
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // Draw the special card at the top left corner (marker for spawn area)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // Orange
        DrawCardGizmo(topLeft, 45f, cSize);
        
        // Draw a few sample cards at random positions
        if (gameManager != null && gameManager.cardPrefabSettings != null && gameManager.cardPrefabSettings.Length > 0)
        {
            // Use a seeded random to ensure consistent visualization
            Random.InitState(42); // Fixed seed for editor visualization
            
            // Draw a few random cards
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            for (int i = 0; i < 5; i++)
            {
                float cardHalfSize = cSize/2;
                float randomX = Random.Range(-width/2f + cardHalfSize, width/2f - cardHalfSize);
                float randomY = Random.Range(-height/2f + cardHalfSize, height/2f - cardHalfSize);
                Vector3 pos = transform.position + new Vector3(randomX, randomY, 0);
                float randomRot = Random.Range(-180f, 180f);
                DrawCardGizmo(pos, randomRot, cSize);
            }
            
            // Reset the random seed
            Random.InitState((int)System.DateTime.Now.Ticks);
        }
#endif
    }
    
    // Helper method to draw a card gizmo
    private void DrawCardGizmo(Vector3 position, float rotation, float size)
    {
#if UNITY_EDITOR
        // Create a square to represent a card
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.Euler(0, 0, rotation), Vector3.one);
        
        float halfSize = size / 2;
        Vector3 p1 = new Vector3(-halfSize, -halfSize, 0);
        Vector3 p2 = new Vector3(halfSize, -halfSize, 0);
        Vector3 p3 = new Vector3(halfSize, halfSize, 0);
        Vector3 p4 = new Vector3(-halfSize, halfSize, 0);
        
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
        
        // Draw an X in the card for visual interest
        Gizmos.DrawLine(p1, p3);
        Gizmos.DrawLine(p2, p4);
        
        Gizmos.matrix = originalMatrix;
#endif
    }
}
