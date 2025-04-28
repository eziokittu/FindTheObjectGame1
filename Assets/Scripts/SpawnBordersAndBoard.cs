using UnityEngine;

public class SpawnBordersAndBoard : MonoBehaviour
{
    private float screenWidth;
    private float screenHeight;
    private GameObject borderPrefab;
    private GameObject boardPrefab;
    private float borderThickness;
    private float cardSize;

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
        borderPrefab = gameManager.GetBorderPrefab();
        boardPrefab = gameManager.GetBoardPrefab();
        borderThickness = gameManager.borderThickness;
        cardSize = gameManager.cardSize;
        screenWidth = gameManager.GetScreenWidth();
        screenHeight = gameManager.GetScreenHeight();

        // After initialization, spawn the borders
        SpawnBorders();
    }

    public void SpawnBorders()
    {
        if (borderPrefab == null)
        {
            Debug.LogWarning("No border prefab assigned in GameManager!");
            return;
        }

        // Get GameManager to ensure we have consistent border thickness
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;
        
        // Calculate extra margin (3/4 of card size) for the board
        float extraMargin = cardSize * 0.75f;
        
        // Spawn the board first (so it's behind the borders)
        if (boardPrefab != null)
        {
            GameObject board = Instantiate(boardPrefab, transform);
            // Board includes the spawn area plus the extra margin
            board.transform.localScale = new Vector3(
                screenWidth + extraMargin * 2, 
                screenHeight + extraMargin * 2, 
                1f);
            board.transform.localPosition = Vector3.zero;
            // Make board non-clickable
            board.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        float borderWidth = borderThickness;
        
        // Add extra margin to the border positions beyond the board
        float totalWidthWithMargin = screenWidth + extraMargin * 2;
        float totalHeightWithMargin = screenHeight + extraMargin * 2;

        // Create top border - position after the board with margin
        GameObject topBorder = Instantiate(borderPrefab, transform);
        topBorder.transform.localScale = new Vector3(totalWidthWithMargin + 2*borderWidth, borderWidth, 1f);
        topBorder.transform.localPosition = new Vector3(0, totalHeightWithMargin/2 + borderWidth/2, 0);

        // Create bottom border - position after the board with margin
        GameObject bottomBorder = Instantiate(borderPrefab, transform);
        bottomBorder.transform.localScale = new Vector3(totalWidthWithMargin + 2*borderWidth, borderWidth, 1f);
        bottomBorder.transform.localPosition = new Vector3(0, -totalHeightWithMargin/2 - borderWidth/2, 0);

        // Create left border - position after the board with margin
        GameObject leftBorder = Instantiate(borderPrefab, transform);
        leftBorder.transform.localScale = new Vector3(borderWidth, totalHeightWithMargin + 2*borderWidth, 1f);
        leftBorder.transform.localPosition = new Vector3(-totalWidthWithMargin/2 - borderWidth/2, 0, 0);

        // Create right border - position after the board with margin
        GameObject rightBorder = Instantiate(borderPrefab, transform);
        rightBorder.transform.localScale = new Vector3(borderWidth, totalHeightWithMargin + 2*borderWidth, 1f);
        rightBorder.transform.localPosition = new Vector3(totalWidthWithMargin/2 + borderWidth/2, 0, 0);

        // Make borders non-clickable
        foreach (Transform child in transform)
        {
            if (child.gameObject.layer != LayerMask.NameToLayer("UI"))
            {
                child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
    }
    
    // Method to spawn a single border at a specific position
    public GameObject SpawnSingleBorder(Vector3 position, Vector3 scale)
    {
        if (borderPrefab == null)
        {
            borderPrefab = GameManager.Instance.GetBorderPrefab();
            if (borderPrefab == null)
            {
                Debug.LogWarning("No border prefab assigned in GameManager!");
                return null;
            }
        }
        
        GameObject border = Instantiate(borderPrefab, position, Quaternion.identity, transform);
        border.transform.localScale = scale;
        border.layer = LayerMask.NameToLayer("Ignore Raycast");
        return border;
    }
    
    // Method to spawn a board at a specific position
    public GameObject SpawnBoard(Vector3 position, Vector3 scale)
    {
        if (boardPrefab == null)
        {
            boardPrefab = GameManager.Instance.GetBoardPrefab();
            if (boardPrefab == null)
            {
                Debug.LogWarning("No board prefab assigned in GameManager!");
                return null;
            }
        }
        
        GameObject board = Instantiate(boardPrefab, position, Quaternion.identity, transform);
        board.transform.localScale = scale;
        board.layer = LayerMask.NameToLayer("Ignore Raycast");
        return board;
    }
    
    // OnDrawGizmosSelected ensures the gizmos are only drawn when this object is selected in the editor
    private void OnDrawGizmosSelected()
    {
        // We now draw in both editor and play mode, but only in editor view
#if UNITY_EDITOR
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
        
        // Calculate the extra margin (3/4 of card size)
        extraMargin = cSize * 0.75f;

        // Draw spawn area (actual gameplay area)
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
        
        // Draw the extended board area (including margins)
        Gizmos.color = new Color(0.8f, 0.4f, 0.1f, 0.3f);
        Vector3 extTopLeft = transform.position + new Vector3(-(width + extraMargin * 2)/2f, (height + extraMargin * 2)/2f, 0);
        Vector3 extTopRight = transform.position + new Vector3((width + extraMargin * 2)/2f, (height + extraMargin * 2)/2f, 0);
        Vector3 extBottomLeft = transform.position + new Vector3(-(width + extraMargin * 2)/2f, -(height + extraMargin * 2)/2f, 0);
        Vector3 extBottomRight = transform.position + new Vector3((width + extraMargin * 2)/2f, -(height + extraMargin * 2)/2f, 0);
        
        Gizmos.DrawLine(extTopLeft, extTopRight);
        Gizmos.DrawLine(extTopRight, extBottomRight);
        Gizmos.DrawLine(extBottomRight, extBottomLeft);
        Gizmos.DrawLine(extBottomLeft, extTopLeft);

        // Draw X showing the extended board
        Gizmos.DrawLine(extTopLeft, extBottomRight);
        Gizmos.DrawLine(extTopRight, extBottomLeft);
        
        // Draw the special card at top left of spawn area
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // Orange
        DrawCardGizmo(topLeft, 45f, cSize);
#endif
    }
    
    // This draws when the object is not selected in the editor
    private void OnDrawGizmos()
    {
        // We now draw in both editor and play mode, but only in editor view
#if UNITY_EDITOR
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
        
        // Calculate the extra margin
        extraMargin = cSize * 0.75f;

        // Using a slightly transparent color when not selected
        // Draw spawn area boundary
        Gizmos.color = new Color(0.7f, 0.7f, 0.7f, 0.3f);
        Vector3 topLeft = transform.position + new Vector3(-width/2f, height/2f, 0);
        Vector3 topRight = transform.position + new Vector3(width/2f, height/2f, 0);
        Vector3 bottomLeft = transform.position + new Vector3(-width/2f, -height/2f, 0);
        Vector3 bottomRight = transform.position + new Vector3(width/2f, -height/2f, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // Draw extended board area (more transparent)
        Gizmos.color = new Color(0.7f, 0.5f, 0.2f, 0.15f);
        Vector3 extTopLeft = transform.position + new Vector3(-(width + extraMargin * 2)/2f, (height + extraMargin * 2)/2f, 0);
        Vector3 extTopRight = transform.position + new Vector3((width + extraMargin * 2)/2f, (height + extraMargin * 2)/2f, 0);
        Vector3 extBottomLeft = transform.position + new Vector3(-(width + extraMargin * 2)/2f, -(height + extraMargin * 2)/2f, 0);
        Vector3 extBottomRight = transform.position + new Vector3((width + extraMargin * 2)/2f, -(height + extraMargin * 2)/2f, 0);
        
        Gizmos.DrawLine(extTopLeft, extTopRight);
        Gizmos.DrawLine(extTopRight, extBottomRight);
        Gizmos.DrawLine(extBottomRight, extBottomLeft);
        Gizmos.DrawLine(extBottomLeft, extTopLeft);
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
