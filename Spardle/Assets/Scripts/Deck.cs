using UnityEngine;
using UnityEngine.SceneManagement;

public class Deck : MonoBehaviour
{
    public void OnPointerEnter()
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            if (transform.position.y >= 0)
            {
                Debug.Log($"EnemyDeckNumber: {CardManager.Instance.EnemyDeckNum}");
            }
            else
            {
                Debug.Log($"PlayerDeckNumber: {CardManager.Instance.PlayerDeckNum}");
            }
        }
    }

    public void OnPointerExit()
    {
        Debug.Log("Deck Pointer Exit");
    }
}