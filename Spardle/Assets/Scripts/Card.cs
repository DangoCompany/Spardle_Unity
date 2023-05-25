using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerDownHandler
{
    private static readonly string[] ColorsString =
    {
        // 赤
        "<color=#e60033>赤色</color>",
        // 緑
        "<color=#3eb370>緑色</color>",
        // 青
        "<color=#0095d9>青色</color>"
    };

    [SerializeField] private Image _figure;
    [SerializeField] private GameObject _nameSlot;
    [SerializeField] private Text _effectName;
    [SerializeField] private Text _effectDescr;
    [SerializeField] private GameObject _scbLogo;
    private int _shapeNum;
    public int ShapeNum => _shapeNum;
    private int _colorNum;
    public int ColorNum => _colorNum;
    private int[] _colorArgs;
    public int[] ColorArgs => _colorArgs;
    private bool _isClickedCard;
    private Vector2 _clickStartPosition;
    private Subject<Vector2> _onClickCard = new Subject<Vector2>();
    public IObservable<Vector2> OnClickCard => _onClickCard;
    
    // デバッグ用
    public void OnPointerEnter()
    {
        Debug.Log("ShapeNum: " + _shapeNum + "\nColorNum: " + _colorNum + "\nColorArgs: " + _colorArgs[0] + "," + _colorArgs[1]);
    }

    public void Initialize(int shapeNum, int colorNum, int[] colorArgs)
    {
        _shapeNum = shapeNum;
        _colorNum = colorNum;
        _colorArgs = colorArgs;
    }

    private void Start()
    {
        this.OnMouseUpAsObservable()
            .Where(_ => _isClickedCard)
            .Select(_ => new Vector2(Input.mousePosition.x, Input.mousePosition.y))
            .Subscribe(clickEndPosition =>
            {
                _onClickCard.OnNext(clickEndPosition - _clickStartPosition);
                _isClickedCard = false;
            });
    }

    private void OnDestroy()
    {
        _onClickCard.OnCompleted();
        _onClickCard.Dispose();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isClickedCard = true;
        _clickStartPosition = eventData.position;
    }

    public void SetCard(CardData cardData, Sprite shape, Color32 color, int[] cnNums)
    {
        _figure.sprite = shape;
        _figure.color = color;
        _effectName.text = cardData.EffectName;
        _effectDescr.text = cardData.EffectDescr;
        _effectDescr.text = _effectDescr.text.Replace("C0", ColorsString[cnNums[0]]);
        _effectDescr.text = _effectDescr.text.Replace("C1", ColorsString[cnNums[1]]);
    }

    public void PickedUp(Deck deck, HalfDeck topHalfDeck, HalfDeck bottomHalfDeck)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DORotate(new Vector3(0, 90, 0), 0.3f).OnComplete(() => TurnOverAndRotateQuarter()));
        sequence.Join(transform.DOMove(deck.transform.position, 1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            deck.gameObject.SetActive(false);
            topHalfDeck.gameObject.SetActive(true);
            bottomHalfDeck.gameObject.SetActive(true);
            topHalfDeck.Shuffle();
            bottomHalfDeck.Shuffle();
            Destroy(gameObject);
        }));
        sequence.Play();
    }

    public void TurnOverAndRotateQuarter()
    {
        _nameSlot.SetActive(false);
        _effectDescr.gameObject.SetActive(false);
        _figure.gameObject.SetActive(false);
        _scbLogo.SetActive(true);
        transform.DORotate(new Vector3(0, 0, 0), 0.3f);
    }
}