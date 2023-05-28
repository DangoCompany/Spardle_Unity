using UnityEngine;

public class Table : MonoBehaviour
{
    public void SetCardPos(Card card)
    {
        Transform cardTransform = card.transform;
        cardTransform.SetParent(transform);
        cardTransform.position = transform.position;
    }
}