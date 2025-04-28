using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // References to component scripts
    [SerializeField] private SpawnCards spawnCardsComponent;
    [SerializeField] private SpawnBordersAndBoard spawnBordersComponent;

    // Game state variables
    private int currentScore = 0;

    [System.Serializable]
    public class PrefabSettings
    {
        public GameObject prefab;
        public int priority = 1;
        public int maxSpawnCount = 0;
        [HideInInspector]
        public int currentSpawnCount = 0;
    }
    
    [Header("Prefabs")]
    [Tooltip("Border prefab used for game boundaries")]
    public GameObject borderPrefab;
    [Tooltip("Board prefab used as the background")]
    public GameObject boardPrefab;
    
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
    [Tooltip("Show editor-only visualization of the game area")]
    public bool showEditorVisualizations = true;

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
    public float borderThickness = 0.1f;
    
    [Header("Rotation Settings")]
    public float minRotation = -180f;
    public float maxRotation = 180f;
    
    // Screen dimensions calculated at runtime
    private float screenWidth;
    private float screenHeight;
    private Camera mainCamera;

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Make sure we have references to our component scripts
        if (spawnCardsComponent == null)
        {
            spawnCardsComponent = gameObject.AddComponent<SpawnCards>();
        }

        if (spawnBordersComponent == null)
        {
            spawnBordersComponent = gameObject.AddComponent<SpawnBordersAndBoard>();
        }
    }

    private void Start()
    {
        ValidateSettings();
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene!");
            return;
        }
        
        CalculateScreenDimensions();
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
                
                // Reset spawn counts
                settings.currentSpawnCount = 0;
            }
        }
        
        if (borderPrefab == null)
        {
            Debug.LogWarning("Border prefab is not assigned in GameManager!");
        }
        
        if (boardPrefab == null)
        {
            Debug.LogWarning("Board prefab is not assigned in GameManager!");
        }
    }

    private void CalculateScreenDimensions()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found in the scene!");
                return;
            }
        }
        
        float screenAspect = 16f / 9f;
        screenHeight = mainCamera.orthographicSize * 2f * screenHeightPercentage;
        screenWidth = screenHeight * screenAspect * screenWidthPercentage;
        
        // Log dimensions for debugging
        Debug.Log($"Game area dimensions: Width={screenWidth}, Height={screenHeight}");
    }

    public void AddScore(int points = 1)
    {
        currentScore += points;
        Debug.Log($"Score: {currentScore}");
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    // Getters for private variables needed by other scripts
    public float GetScreenWidth() => screenWidth;
    public float GetScreenHeight() => screenHeight;
    public Camera GetMainCamera() => mainCamera;
    
    // Prefab access methods
    public GameObject GetBorderPrefab() => borderPrefab;
    public GameObject GetBoardPrefab() => boardPrefab;
    
    // Color helper method
    public void ApplyColorToSprites(GameObject obj, Color color)
    {
        SpriteRenderer[] spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = color;
        }
    }
    
    // Spawn game elements
    public void SpawnBordersAndBoard()
    {
        if (spawnBordersComponent != null)
        {
            spawnBordersComponent.SpawnBorders();
        }
    }
    
    public void SpawnCard()
    {
        if (spawnCardsComponent != null)
        {
            spawnCardsComponent.SpawnRandomCard();
        }
    }
    
#if UNITY_EDITOR
    // This will draw the editor visualization when the GameManager is selected in the hierarchy
    private void OnDrawGizmosSelected()
    {
        // Only hide if editor visualizations are disabled
        if (!showEditorVisualizations) return;
        
        if (Camera.main == null) return;
        
        // During play mode, use cached values if available
        float width, height, borderWidth, extraMargin;
        if (Application.isPlaying && screenWidth > 0 && screenHeight > 0)
        {
            height = screenHeight;
            width = screenWidth;
            borderWidth = borderThickness;
        }
        else
        {
            height = Camera.main.orthographicSize * 2f * screenHeightPercentage;
            width = height * (16f / 9f) * screenWidthPercentage;
            borderWidth = borderThickness;
        }
        
        // Calculate the extended board dimensions (with 3/4 card size margin)
        extraMargin = cardSize * 0.75f;
        float extendedWidth = width + extraMargin * 2;
        float extendedHeight = height + extraMargin * 2;
        
        // Draw a title for the visualization
        UnityEditor.Handles.BeginGUI();
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperCenter;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, height/2f + 0.5f, 0));
        screenPos.y = Screen.height - screenPos.y;
        UnityEditor.Handles.Label(screenPos, "Game Manager - Board Area", style);
        UnityEditor.Handles.EndGUI();
        
        // Draw the spawn area boundary (green)
        Gizmos.color = new Color(0.1f, 0.8f, 0.1f, 0.5f);
        Vector3 center = transform.position;
        Vector3 size = new Vector3(width, height, 0.1f);
        
        // Draw the spawn area (where cards appear)
        Gizmos.DrawWireCube(center, size);
        
        // Draw the extended board area (orange)
        Gizmos.color = new Color(0.8f, 0.4f, 0.1f, 0.3f);
        Vector3 extendedSize = new Vector3(extendedWidth, extendedHeight, 0.1f);
        Gizmos.DrawWireCube(center, extendedSize);
        
        // Draw the borders
        Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
        
        // Top border - positioned outside the extended board
        Gizmos.DrawCube(
            center + new Vector3(0, extendedHeight/2f + borderWidth/2, 0),
            new Vector3(extendedWidth + 2*borderWidth, borderWidth, 0.1f)
        );
        
        // Bottom border
        Gizmos.DrawCube(
            center + new Vector3(0, -extendedHeight/2f - borderWidth/2, 0),
            new Vector3(extendedWidth + 2*borderWidth, borderWidth, 0.1f)
        );
        
        // Left border
        Gizmos.DrawCube(
            center + new Vector3(-extendedWidth/2f - borderWidth/2, 0, 0),
            new Vector3(borderWidth, extendedHeight + 2*borderWidth, 0.1f)
        );
        
        // Right border
        Gizmos.DrawCube(
            center + new Vector3(extendedWidth/2f + borderWidth/2, 0, 0),
            new Vector3(borderWidth, extendedHeight + 2*borderWidth, 0.1f)
        );
        
        // Draw the special card at top left of spawn area
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // Orange
        Vector3 topLeft = center + new Vector3(-width/2f, height/2f, 0);
        DrawCardGizmo(topLeft, 45f, cardSize);
    }
    
    // Helper method to draw a card gizmo
    private void DrawCardGizmo(Vector3 position, float rotation, float size)
    {
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
    }
#endif
}
