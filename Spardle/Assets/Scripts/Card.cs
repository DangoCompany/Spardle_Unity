using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Card : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image _figure;
    [SerializeField] private GameObject _nameSlot;
    [SerializeField] private Text _effectName;
    [SerializeField] private Text _effectDescr;
    [SerializeField] private GameObject _scbLogo;
    [SerializeField] private GameObject _actionButton;
    private Deck _playerDeck;
    private Deck _enemyDeck;
    private int[] _buttonColors = new[] { 0, 1, 2 };
    private ConfigConstants.CardEffect _cardEffect;
    private int _shapeNum;
    private bool _isMyCard;
    public int ShapeNum => _shapeNum;
    private int _colorNum;
    public int ColorNum => _colorNum;
    private int[] _colorArgs;
    public int[] ColorArgs => _colorArgs;
    private bool _isClickedCard;
    private Vector2 _clickStartPosition;
    private Subject<Vector2> _onClickCard = new Subject<Vector2>();
    private Subject<Unit> _onClicking = new Subject<Unit>();

    public IObservable<Vector2> OnClickCard => _onClickCard;
    public IObservable<Unit> OnClicking => _onClicking;

    // デバッグ用
    public void OnPointerEnter()
    {
        Debug.Log($"ShapeNum: {_shapeNum}\nColorNum: {_colorNum}\nColorArgs: {_colorArgs[0]}, {_colorArgs[1]}\nCardEffect: {_cardEffect}");
    }

    public void Initialize(int shapeNum, int colorNum, int[] colorArgs, bool isMyCard, ConfigConstants.CardEffect cardEffect, Deck playerDeck, Deck enemyDeck)
    {
        _shapeNum = shapeNum;
        _colorNum = colorNum;
        _colorArgs = colorArgs;
        _isMyCard = isMyCard;
        _cardEffect = cardEffect;
        _playerDeck = playerDeck;
        _enemyDeck = enemyDeck;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            this.OnMouseUpAsObservable()
                .Where(_ => _isMyCard)
                .Where(_ => _isClickedCard)
                .Where(_ => !(bool)GameProperties.GetCustomPropertyValue(ConfigConstants.CustomPropertyKey
                    .IsMasterCardPlaying))
                .Where(_ => !(bool)GameProperties.GetCustomPropertyValue(ConfigConstants.CustomPropertyKey
                    .IsNonMasterCardPlaying))
                .Where(_ => !(bool)GameProperties.GetCustomPropertyValue(ConfigConstants.CustomPropertyKey
                    .IsSenderActionInProgress))
                .Where(_ => !(bool)GameProperties.GetCustomPropertyValue(ConfigConstants.CustomPropertyKey
                    .IsReceiverActionInProgress))
                .Select(_ => new Vector2(Input.mousePosition.x, Input.mousePosition.y))
                .Subscribe(clickEndPosition =>
                {
                    GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsSenderActionInProgress, true);
                    GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsReceiverActionInProgress, true);
                    _onClickCard.OnNext(clickEndPosition - _clickStartPosition);
                    _isClickedCard = false;
                    _actionButton.SetActive(false);
                });
        }
        else if (SceneManager.GetActiveScene().name == "StoryMode")
        {
            this.OnMouseUpAsObservable()
                .Where(_ => _isMyCard)
                .Where(_ => _isClickedCard)
                .Select(_ => new Vector2(Input.mousePosition.x, Input.mousePosition.y))
                .Subscribe(clickEndPosition =>
                {
                    _onClickCard.OnNext(clickEndPosition - _clickStartPosition);
                    _isClickedCard = false;
                    _actionButton.SetActive(false);
                });
        }
    }

    private void OnDestroy()
    {
        _onClickCard.OnCompleted();
        _onClickCard.Dispose();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isMyCard)
        {
            _isClickedCard = true;
            _clickStartPosition = eventData.position;
            _actionButton.SetActive(true);
            _actionButton.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(_clickStartPosition).x, Camera.main.ScreenToWorldPoint(_clickStartPosition).y, 0);
            _actionButton.GetComponent<ActionButton>().SetButtonPosition(_buttonColors);
            _onClicking.OnNext(Unit.Default);
        }
    }

    public void SetCard(CardData cardData, Sprite shape, Color32 color, int[] cnNums)
    {
        _figure.sprite = shape;
        _figure.color = color;
        _effectName.text = cardData.EffectName;
        _effectDescr.text = cardData.EffectDescr;
        _effectDescr.text = _effectDescr.text.Replace("C0", DictionaryConstants.ColorsString[cnNums[0]]);
        _effectDescr.text = _effectDescr.text.Replace("C1", DictionaryConstants.ColorsString[cnNums[1]]);
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
        if (!_isMyCard && _cardEffect == ConfigConstants.CardEffect.Exchange)
        {
            CardManager.Instance.ApplyExchangeToMyCards(_colorArgs[0], _colorArgs[1]);
        }
        sequence.Play();
    }

    private void TurnOverAndRotateQuarter()
    {
        _nameSlot.SetActive(false);
        _effectDescr.gameObject.SetActive(false);
        _figure.gameObject.SetActive(false);
        _scbLogo.SetActive(true);
        transform.DORotate(new Vector3(0, 0, 0), 0.3f);
    }

    public void GatherAndDeal()
    {
        Deck deck = _isMyCard ? _playerDeck : _enemyDeck;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DORotate(new Vector3(0, 90, 0), 0.3f).OnComplete(() => TurnOverAndRotateQuarter()));
        sequence.Join(transform.DOMove(new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f), 0), 1f).SetEase(Ease.Linear))
            .Append(transform.DOMove(deck.transform.position, 1f).OnComplete(() =>
            {
                Destroy(gameObject);
            }));
        sequence.Play();
    }
    
    public void ExchangeButton(int color0, int color1)
    {
        (_buttonColors[color0], _buttonColors[color1]) = (_buttonColors[color1], _buttonColors[color0]);
    }
}