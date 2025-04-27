using UnityEngine;

public class ClickObjects : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log($"[ClickObjects] Mouse click detected on object: {gameObject.name}");
        Debug.Log($"[ClickObjects] Object position: {transform.position}, Layer: {gameObject.layer}, Active: {gameObject.activeInHierarchy}");
        
        // Get the GameManager instance
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            Debug.Log("[ClickObjects] GameManager found, attempting to add score and destroy object");
            gameManager.AddScore();
            
            // Store object name before destruction for logging
            string objName = gameObject.name;
            Destroy(gameObject);
            Debug.Log($"[ClickObjects] Destroy called on object: {objName}");
        }
        else
        {
            Debug.LogError("[ClickObjects] GameManager instance not found! Cannot process click.");
        }
    }

    // Add OnDestroy to verify when object is actually destroyed
    private void OnDestroy()
    {
        Debug.Log($"[ClickObjects] Object {gameObject.name} has been destroyed");
    }
}
