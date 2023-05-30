using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TurnPanel : MonoBehaviour
{
    [SerializeField] private Text _turnText;

    public void FlipTurnPanel(int isMyTurn)
    {
        transform.DORotate(new Vector3(0, 90, 0), 0.3f).OnComplete(() =>
        {
            _turnText.text = DictionaryConstants.TurnsString[isMyTurn];
            transform.DORotate(new Vector3(0, 0, 0), 0.3f);
        });
    }
}
