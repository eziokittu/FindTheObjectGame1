using UnityEngine;

public class ClickObjects : MonoBehaviour
{

    private void OnMouseDown()
    {
        GameManager.Instance.AddScore();
        Destroy(gameObject);
    }
}
