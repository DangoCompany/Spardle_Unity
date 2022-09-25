using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Card : MonoBehaviour
{
    private static readonly string[] ColorsString =
    {
        //赤
        "<color=#e60033>赤色</color>",
        //緑
        "<color=#3eb370>緑色</color>",
        //青
        "<color=#0095d9>青色</color>"
    };
    [SerializeField] private Image _figure;
    [SerializeField] private GameObject _nameSlot;
    [SerializeField] private Text _effectName;
    [SerializeField] private Text _effectDescr;
    [SerializeField] private GameObject _SCBLogo;
    private int[] _colorArgs;
    public int[] ColorArgs { get => _colorArgs; set => _colorArgs = value; }

    public void SetCard(CardData cardData, Sprite figure, Color32 color, int[] CnNums)
    {
        _figure.sprite = figure;
        _figure.color = color;
        _effectName.text = cardData.EffectName;
        _effectDescr.text = cardData.EffectDescr;
        _effectDescr.text = _effectDescr.text.Replace("C0", ColorsString[CnNums[0]]);
        _effectDescr.text = _effectDescr.text.Replace("C1", ColorsString[CnNums[1]]);
    }
    public void PickedUp(Deck deck, HalfDeck topHalfDeck, HalfDeck bottomHalfDeck)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DORotate(new Vector3(0, 90, 0), 0.3f).OnComplete(() => TurnOverAndRotateQuarter()));
        sequence.Join(transform.DOMove(deck.transform.position, 1f).SetEase(Ease.Linear).OnComplete(() => {
            deck.gameObject.SetActive(false);
            topHalfDeck.gameObject.SetActive(true);
            bottomHalfDeck.gameObject.SetActive(true);
            topHalfDeck.Shuffle(2);
            bottomHalfDeck.Shuffle(2);
            Destroy(gameObject);
        }));
        sequence.Play();
    }
    public void TurnOverAndRotateQuarter()
    {
        _nameSlot.SetActive(false);
        _effectDescr.gameObject.SetActive(false);
        _figure.gameObject.SetActive(false);
        _SCBLogo.SetActive(true);
        transform.DORotate(new Vector3(0, 0, 0), 0.3f);
    }
}
