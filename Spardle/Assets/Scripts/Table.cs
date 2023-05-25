using UnityEngine;

public class Table : MonoBehaviour
{
    public void SetCardPos(Card card)
    {
        card.transform.SetParent(transform);
        card.transform.position = transform.position;
    }
}