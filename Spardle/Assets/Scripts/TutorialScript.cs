using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

public class TutorialScript : MonoBehaviour
{
    [SerializeField] private Sprite[] _cardShapes;
    [SerializeField] private CardData[] _cardData;
    [SerializeField] private Table[] _playerTables;
    [SerializeField] private Table[] _enemyTables;
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private Deck _playerDeck;
    [SerializeField] private Deck _enemyDeck;
    [SerializeField] private HalfDeck _enemyTopHalfDeck;
    [SerializeField] private HalfDeck _enemyBottomHalfDeck;

    private Card[] _playerCards = new Card[ConfigConstants.TableCount];
    private Card[] _enemyCards = new Card[ConfigConstants.TableCount];
    private CardData[] _playerCardData = new CardData[ConfigConstants.TableCount];
    private CardData[] _enemyCardData = new CardData[ConfigConstants.TableCount];

    private int _tableNumberToPlace;
    private int _enemyTableNumberToPlace;

    private CancellationTokenSource _cts;

    private async void Start()
    {
        _cts = new CancellationTokenSource();
        await DeckFlipAwaiter(2, new[] { 0, 0 }, new[] { 0, 0 });
        await EnemyDeckFlipAwaiter(2, new[] { 1, 1 }, new[] { 0, 0 });
        await DeckFlipAwaiter(2, new[] { 2, 2 }, new[] { 0, 0 });
        await EnemyDeckFlipAwaiter(2, new[] { 3, 0 }, new[] { 0, 0 });
        await DeckFlipAwaiter(2, new[] { 1, 1 }, new[] { 0, 0 });
        await CardActionAwaiter(0, 1, 1);
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        await EnemyDeckFlipAwaiter(2, new[] { 4, 2 }, new[] { 0, 0 });
        await DeckFlipAwaiter(2, new[] { 3, 1 }, new[] { 0, 0 });
        await CardActionAwaiter(1, 2, 2);
    }

    private async UniTask DeckFlipAwaiter(int cardDataNum, int[] figureData, int[] cnNums)
    {
        Debug.Log("エンターキーを押してね！");
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        DecideTableNumber(ref _tableNumberToPlace);
        if (_playerCards[_tableNumberToPlace] != null)
        {
            Destroy(_playerCards[_tableNumberToPlace].gameObject);
        }

        (_playerCards[_tableNumberToPlace], _playerCardData[_tableNumberToPlace]) =
            GenerateCard(cardDataNum, figureData, cnNums, true);
        _playerTables[_tableNumberToPlace].SetCardPos(_playerCards[_tableNumberToPlace]);
    }

    private async UniTask EnemyDeckFlipAwaiter(int cardDataNum, int[] figureData, int[] cnNums)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        DecideTableNumber(ref _enemyTableNumberToPlace);
        if (_enemyCards[_enemyTableNumberToPlace] != null)
        {
            Destroy(_enemyCards[_enemyTableNumberToPlace].gameObject);
        }

        (_enemyCards[_enemyTableNumberToPlace], _enemyCardData[_enemyTableNumberToPlace]) =
            GenerateCard(cardDataNum, figureData, cnNums, false);
        _enemyTables[_enemyTableNumberToPlace].SetCardPos(_enemyCards[_enemyTableNumberToPlace]);
    }

    private async UniTask CardActionAwaiter(int _playerTableNum, int _enemyTableNum, int directionNum)
    {
        bool canGoNext = false;
        Debug.Log("今出したカードをクリックしてみよう！");
        _playerCards[_tableNumberToPlace].OnClicking
            .Subscribe(_ =>
            {
                string direction;
                if (directionNum == 0)
                {
                    direction = "上";
                }
                else if (directionNum == 1)
                {
                    direction = "左下";
                }
                else
                {
                    direction = "右下";
                }

                Debug.Log("ドラッグしたまま" + direction + "にマウスを動かして、離してみよう！");
            });
        _playerCards[_tableNumberToPlace].OnClickCard
            .Subscribe(delta =>
            {
                float angle = Mathf.Atan2(delta.y, delta.x);
                if ((directionNum == 0 && (angle >= Mathf.PI / 6 && angle < 5 * Mathf.PI / 6)) ||
                    (directionNum == 1 && (angle >= 5 * Mathf.PI / 6 || angle < -Mathf.PI / 2)) ||
                    (directionNum == 2 && (angle >= -Mathf.PI / 2 && angle < Mathf.PI / 6)))
                {
                    Debug.Log("いい感じ！");
                    canGoNext = true;
                }
                else
                {
                    Debug.Log("ちょっと向きが違うかも！");
                }
            })
            .AddTo(_playerCards[_tableNumberToPlace]);
        await UniTask.WaitUntil(() => canGoNext);
        _playerCards[_playerTableNum].PickedUp(_enemyDeck, _enemyTopHalfDeck, _enemyBottomHalfDeck);
        _playerCards[_playerTableNum] = null;
        _enemyCards[_enemyTableNum].PickedUp(_enemyDeck, _enemyTopHalfDeck, _enemyBottomHalfDeck);
        _enemyCards[_enemyTableNum] = null;
    }

    private (Card, CardData) GenerateCard(int cardDataNum, int[] figureData, int[] cnNums, bool isMyCard)
    {
        Sprite shape = _cardShapes[figureData[0]];
        Color32 color = DictionaryConstants.FigureColors[figureData[1]];
        Card card = Instantiate(_cardPrefab);
        CardData cardData = _cardData[cardDataNum];
        card.Initialize(figureData[0], figureData[1], new int[] { cnNums[0], cnNums[1] }, isMyCard, cardData.Effect,
            _playerDeck, _enemyDeck);
        card.SetCard(cardData, shape, color, cnNums);
        return (card, cardData);
    }

    private void DecideTableNumber(ref int tableNum)
    {
        if (tableNum == ConfigConstants.TableCount - 1)
        {
            tableNum = 0;
        }
        else
        {
            tableNum++;
        }
    }

    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}