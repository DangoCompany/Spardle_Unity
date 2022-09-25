using UnityEngine;

public class Deck : MonoBehaviour
{
    public void OnPointerEnter()
    {
        if(transform.position.y >= 0)
        {
            Debug.LogError("EnemyDeckNumber: " + CardManager.Instance.EnemyDeckNum);
        }
        else
        {
            Debug.LogError("PlayerDeckNumber: " + CardManager.Instance.PlayerDeckNum);
        }
    }
    public void OnPointerExit()
    {
        Debug.LogError("Exit");
    }
}
