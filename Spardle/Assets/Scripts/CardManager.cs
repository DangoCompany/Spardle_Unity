using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using UniRx;
using UniRx.Triggers;

public class CardManager : MonoBehaviourPunCallbacks
{
    public static CardManager Instance;
    [SerializeField] private Sprite[] _cardShapes;
    [SerializeField] private CardData[] _cardData;
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private Table[] _playerTables;
    [SerializeField] private GameObject _playerRemovePoint;
    [SerializeField] private Table[] _enemyTables;
    [SerializeField] private GameObject _enemyRemovePoint;
    [SerializeField] private Deck _playerDeck;
    [SerializeField] private Deck _enemyDeck;
    [SerializeField] private HalfDeck _playerTopHalfDeck;
    [SerializeField] private HalfDeck _playerBottomHalfDeck;
    [SerializeField] private HalfDeck _enemyTopHalfDeck;
    [SerializeField] private HalfDeck _enemyBottomHalfDeck;
    [SerializeField] private MatchOverPanel _matchOverPanel;
    public int PlayerDeckNum { get; private set; }
    public int EnemyDeckNum { get; private set; }

    // {Shape, Color}
    private List<int[]> _playerDeckData = new List<int[]>(ConfigConstants.TotalCardsNum);
    private List<int[]> _enemyDeckData = new List<int[]>(ConfigConstants.TotalCardsNum);
    private CardData[] _playerCardData = new CardData[ConfigConstants.TableCount];
    private CardData[] _enemyCardData = new CardData[ConfigConstants.TableCount];

    // {Index, Shape, Color}
    private List<int[]> _playerPlayedData = new List<int[]>(ConfigConstants.TotalCardsNum);
    private List<int[]> _enemyPlayedData = new List<int[]>(ConfigConstants.TotalCardsNum);
    private Card[] _playerCards = new Card[ConfigConstants.TableCount];
    private Card[] _enemyCards = new Card[ConfigConstants.TableCount];
    private List<int> _exceptedCardDataNums = new List<int>(ConfigConstants.CardDataNum);

    private int _tableNumberToPlace;

    // {ShapeNum, IsMyCard, Index}
    private int[] _removeIndex;

    public int PointedTableNumber;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerDeckNum += ConfigConstants.TotalCardsNum / 2;
            EnemyDeckNum += ConfigConstants.TotalCardsNum / 2;
            DealCardsToPlayers();
            Dictionary<int, int[]> playerDeckDataDic = new Dictionary<int, int[]>(PlayerDeckNum);
            Dictionary<int, int[]> enemyDeckDataDic = new Dictionary<int, int[]>(EnemyDeckNum);
            for (int i = 0; i < PlayerDeckNum; i++)
            {
                playerDeckDataDic.Add(i, _playerDeckData[i]);
                enemyDeckDataDic.Add(i, _enemyDeckData[i]);
            }

            photonView.RPC(nameof(ReceiveDecksData), RpcTarget.Others, enemyDeckDataDic, playerDeckDataDic, false);
        }

        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyDown(KeyCode.Return))
            .Where(_ => TurnManager.Instance.IsMyTurn)
            .Subscribe(async _ =>
            {
                if (CanPutActionButton())
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(
                        new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsWrongPlayingKey], PhotonNetwork.LocalPlayer.ActorNumber } },
                        new Hashtable()
                        {
                            { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsCardPlayingKey], 0 },
                            { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsWrongPlayingKey], 0 },
                            { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsActionInProgressKey], 0 }
                        }
                    );
                    return;
                }

                PhotonNetwork.CurrentRoom.SetCustomProperties(
                    new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsCardPlayingKey], PhotonNetwork.LocalPlayer.ActorNumber } },
                    new Hashtable()
                    {
                        { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsCardPlayingKey], 0 },
                        { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsWrongPlayingKey], 0 },
                        { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsActionInProgressKey], 0 }
                    }
                );
            });
    }

    public void PlayCard()
    {
        int cardDataNum;
                int figureNum = UnityEngine.Random.Range(0, PlayerDeckNum);
                int[] figureData = _playerDeckData[figureNum];
                int[] cnNums = { 0, 0 };
                DecideTableNumber();
                if (_playerCards[_tableNumberToPlace] != null)
                {
                    if (_playerCardData[_tableNumberToPlace].Effect == ConfigConstants.CardEffect.Exchange)
                    {
                        photonView.RPC(nameof(ApplyExchangeToMyCards), RpcTarget.Others,
                            _playerCards[_tableNumberToPlace].ColorArgs[0],
                            _playerCards[_tableNumberToPlace].ColorArgs[1]);
                    }
                    else if (_playerCardData[_tableNumberToPlace].Effect == ConfigConstants.CardEffect.Remove)
                    {
                        _removeIndex = null;
                    }

                    Destroy(_playerCards[_tableNumberToPlace].gameObject);
                }

                ExceptCardData(ConfigConstants.CardEffect.Exchange, false);
                ExceptCardData(ConfigConstants.CardEffect.Substitute, true);
                ExceptCardData(ConfigConstants.CardEffect.Remove, true);
                cardDataNum = DecideCardDataNumber();
                if (_cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Exchange ||
                    _cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Substitute)
                {
                    List<int> colorIndexes = new List<int>(3) { 0, 1, 2 };
                    cnNums[0] = UnityEngine.Random.Range(0, 3);
                    colorIndexes.RemoveAt(cnNums[0]);
                    cnNums[1] = colorIndexes[UnityEngine.Random.Range(0, 2)];
                }
                else if (_cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Illusion)
                {
                    cnNums[0] = UnityEngine.Random.Range(0, 3);
                }

                (_playerCards[_tableNumberToPlace], _playerCardData[_tableNumberToPlace]) =
                    GenerateCard(cardDataNum, figureData, cnNums, _tableNumberToPlace, true);
                Card card = _playerCards[_tableNumberToPlace];
                Card[] enemyExchangeCards = _enemyCards
                    .Select((enemyCard, index) => new { Index = index, EnemyCard = enemyCard })
                    .Where(item => item.EnemyCard != null)
                    .Where(item => _enemyCardData[item.Index].Effect == ConfigConstants.CardEffect.Exchange)
                    .Select(item => item.EnemyCard)
                    .ToArray();
                foreach (var enemyExchangeCard in enemyExchangeCards)
                {
                    card.ExchangeButton(enemyExchangeCard.ColorArgs[0], enemyExchangeCard.ColorArgs[1]);
                }

                if (_playerCardData[_tableNumberToPlace].Effect == ConfigConstants.CardEffect.Remove)
                {
                    _removeIndex = new[]
                    {
                        _playerCards[_tableNumberToPlace].ShapeNum,
                        1,
                        _tableNumberToPlace
                    };
                }

                _playerTables[_tableNumberToPlace].SetCardPos(card);
                _playerPlayedData.Add(new int[] { _tableNumberToPlace, figureData[0], figureData[1] });
                if (PlayerDeckNum > 0)
                {
                    PlayerDeckNum--;
                }
                else
                {
                    throw new Exception("Deck Empty");
                }

                _playerDeckData.Remove(figureData);
                photonView.RPC(nameof(PlayEnemyCard), RpcTarget.Others, cardDataNum, figureData, cnNums,
                    _tableNumberToPlace);
                TurnManager.Instance.SetIsMyTurn(false);
    }

    public async void PlayWrongly()
    {
        PushWrongButton();
        // 美しくない
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsWrongPlayingKey], 0 } }
        );
    }

    private void DealCardsToPlayers()
    {
        int deckNum = ConfigConstants.TotalCardsNum / 2;
        int leftCardsNum = ConfigConstants.TotalCardsNum;
        List<int> playerCardIndexes = new List<int>(ConfigConstants.TotalCardsNum);
        for (int i = 0; i < leftCardsNum; i++)
        {
            playerCardIndexes.Add(i);
        }

        while (leftCardsNum-- > deckNum)
        {
            int index = UnityEngine.Random.Range(0, leftCardsNum);
            playerCardIndexes.RemoveAt(index);
        }

        for (int i = 0; i < ConfigConstants.FigureShapeNum; i++)
        {
            for (int j = 0; j < DictionaryConstants.FigureColors.Length; j++)
            {
                int[] data = { i, j };
                if (playerCardIndexes.Contains(i * DictionaryConstants.FigureColors.Length + j))
                {
                    _playerDeckData.Add(data);
                }
                else
                {
                    _enemyDeckData.Add(data);
                }
            }
        }
    }

    private void DecideTableNumber()
    {
        if (_tableNumberToPlace == ConfigConstants.TableCount - 1)
        {
            _tableNumberToPlace = 0;
        }
        else
        {
            _tableNumberToPlace++;
        }
    }

    private int DecideCardDataNumber()
    {
        List<int> availableNums = new List<int>(ConfigConstants.CardDataNum);
        for (int i = 0; i < ConfigConstants.CardDataNum; i++)
        {
            availableNums.Add(i);
        }

        availableNums = availableNums.Except(_exceptedCardDataNums).ToList();
        _exceptedCardDataNums.Clear();
        return availableNums[UnityEngine.Random.Range(0, availableNums.Count())];
    }

    private void ExceptCardData(ConfigConstants.CardEffect cardEffect, bool dependsOnBoth)
    {
        if (_playerCardData
                .Where(data => data != null)
                .Any(data => data.Effect == cardEffect) ||
            (_enemyCardData
                 .Where(data => data != null)
                 .Any(data => data.Effect == cardEffect) &&
             dependsOnBoth))
        {
            _exceptedCardDataNums.Add((int)cardEffect);
        }
    }

    private bool CanPutActionButton()
    {
        var playerCards = _playerCards
            .Where(playerCard => playerCard != null);
        foreach (var playerCard in playerCards)
        {
            if (_enemyCards.Where(enemyCard => enemyCard != null)
                .Any(enemyCard => enemyCard.ShapeNum == playerCard.ShapeNum))
            {
                return true;
            }
        }

        return false;
    }

    private (Card, CardData) GenerateCard(int cardDataNum, int[] figureData, int[] cnNums, int tableNumber, bool isMyCard)
    {
        Sprite shape = _cardShapes[figureData[0]];
        Color32 color = DictionaryConstants.FigureColors[figureData[1]];
        Card card = Instantiate(_cardPrefab);
        CardData cardData = _cardData[cardDataNum];
        card.Initialize(figureData[0], figureData[1], new int[] { cnNums[0], cnNums[1] }, tableNumber, isMyCard, cardData.Effect,
            _playerDeck, _enemyDeck);
        card.SetCard(cardData, shape, color, cnNums);
        return (card, cardData);
    }

    public void OnReceiveCardAction()
    {
        int index = PointedTableNumber;
        Card card = _playerCards[index];
        Vector2 delta = card.Delta;
        Debug.Log($"Clicked Card Index: {index}");
        Dictionary<int, Card> enemyCorrespondingCardsDict = _enemyCards
            .Select((enemyCard, cardIndex) => new { Index = cardIndex, EnemyCard = enemyCard })
            .Where(item => item.EnemyCard != null)
            .Where(item => item.EnemyCard.ShapeNum == card.ShapeNum)
            .ToDictionary(item => item.Index, item => item.EnemyCard);
        if (enemyCorrespondingCardsDict.Count > 0)
        {
            int selectedColorNum = GetSelectedColorNum(Mathf.Atan2(delta.y, delta.x));
            Debug.Log($"Selected Color Num: {selectedColorNum}");
            int[] tmpCorrectColorNums = enemyCorrespondingCardsDict
                .Select(enemyCardPair =>
                {
                    int cardColorNum;
                    int enemyCardColorNum;
                    if (_playerCardData[index].Effect == ConfigConstants.CardEffect.Illusion)
                    {
                        Debug.Log($"Illusion Player Card Color: {card.ColorNum} to {card.ColorArgs[0]}");
                        cardColorNum = card.ColorArgs[0];
                    }
                    else
                    {
                        cardColorNum = card.ColorNum;
                    }

                    if (_enemyCardData[enemyCardPair.Key].Effect == ConfigConstants.CardEffect.Illusion)
                    {
                        Debug.Log(
                            $"Illusion Enemy Card Color: {enemyCardPair.Value.ColorNum} to {enemyCardPair.Value.ColorArgs[0]}");
                        enemyCardColorNum = enemyCardPair.Value.ColorArgs[0];
                    }
                    else
                    {
                        enemyCardColorNum = enemyCardPair.Value.ColorNum;
                    }

                    if (enemyCardColorNum == cardColorNum)
                    {
                        return cardColorNum;
                    }
                    else
                    {
                        int[] allColorNums = { 0, 1, 2 };
                        return allColorNums
                            .Except(new[] { cardColorNum, enemyCardColorNum }).First();
                    }
                })
                .Distinct()
                .ToArray();
            Debug.Log(
                $"Temporary Correct Color: {string.Join(", ", tmpCorrectColorNums.Select(_ => _.ToString()).ToArray())}");
            int[] correctColorNums = GetCorrectColorNums(tmpCorrectColorNums);
            Debug.Log($"Correct Color: {string.Join(", ", correctColorNums.Select(_ => _.ToString()).ToArray())}");
            if (correctColorNums.Contains(selectedColorNum))
            {
                PushCorrectButton(new int[] { index }, enemyCorrespondingCardsDict.Keys.ToArray());
                return;
            }
        }

        PushWrongButton();
    }

    private int GetSelectedColorNum(float angle)
    {
        if (angle >= Mathf.PI / 6 && angle < Mathf.PI * 5 / 6)
        {
            return 0;
        }
        else if (angle >= Mathf.PI * 5 / 6 || angle < Mathf.PI * -1 / 2)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    private int[] GetCorrectColorNums(int[] tmpCorrectColorNums)
    {
        int[] correctColorNums = tmpCorrectColorNums;
        Dictionary<int, Card> playerCardsDict = _playerCards
            .Select((playerCard, index) => new { Index = index, PlayerCard = playerCard })
            .Where(item => item.PlayerCard != null)
            .ToDictionary(item => item.Index, item => item.PlayerCard);
        Dictionary<int, Card> enemyCardsDict = _enemyCards
            .Select((enemyCard, index) => new { Index = index, EnemyCard = enemyCard })
            .Where(item => item.EnemyCard != null)
            .ToDictionary(item => item.Index, item => item.EnemyCard);
        foreach (var enemyCard in enemyCardsDict)
        {
            if (_enemyCardData[enemyCard.Key].Effect == ConfigConstants.CardEffect.Substitute)
            {
                Debug.Log(
                    $"Enemy Substitute Found: {string.Join(", ", correctColorNums.Select(_ => _.ToString()).ToArray())}");
                correctColorNums = correctColorNums
                    .Select(value => SubstituteCorrectColor(value, _enemyCards[enemyCard.Key])).Distinct().ToArray();
                Debug.Log(
                    $"After Enemy Substitute: {string.Join(", ", correctColorNums.Select(_ => _.ToString()).ToArray())}");
            }
        }

        foreach (var playerCard in playerCardsDict)
        {
            if (_playerCardData[playerCard.Key].Effect == ConfigConstants.CardEffect.Substitute)
            {
                Debug.Log(
                    $"Player Substitute Found: {string.Join(", ", correctColorNums.Select(_ => _.ToString()).ToArray())}");
                correctColorNums = correctColorNums
                    .Select(value => SubstituteCorrectColor(value, _playerCards[playerCard.Key])).Distinct().ToArray();
                Debug.Log(
                    $"After Player Substitute: {string.Join(", ", correctColorNums.Select(_ => _.ToString()).ToArray())}");
            }
        }

        foreach (var enemyCard in enemyCardsDict)
        {
            if (_enemyCardData[enemyCard.Key].Effect == ConfigConstants.CardEffect.Exchange)
            {
                Debug.Log($"Exchange Found: {string.Join(", ", correctColorNums.Select(_ => _.ToString()).ToArray())}");
                correctColorNums = correctColorNums.Select(value => ExchangeCorrectColor(value, enemyCard.Key))
                    .Distinct().ToArray();
                Debug.Log($"After Exchange: {string.Join(", ", correctColorNums.Select(_ => _.ToString()).ToArray())}");
            }
        }

        return correctColorNums;
    }

    private int ExchangeCorrectColor(int value, int index)
    {
        if (value == _enemyCards[index].ColorArgs[0])
        {
            Debug.Log($"Exchanged: {_enemyCards[index].ColorArgs[0]} to {_enemyCards[index].ColorArgs[1]}");
            return _enemyCards[index].ColorArgs[1];
        }
        else if (value == _enemyCards[index].ColorArgs[1])
        {
            Debug.Log($"Exchanged: {_enemyCards[index].ColorArgs[1]} to {_enemyCards[index].ColorArgs[0]}");
            return _enemyCards[index].ColorArgs[0];
        }
        else
        {
            return value;
        }
    }

    private int SubstituteCorrectColor(int value, Card card)
    {
        if (value == card.ColorArgs[0])
        {
            Debug.Log($"Substituted: {card.ColorArgs[0]} to {card.ColorArgs[1]}");
            return card.ColorArgs[1];
        }
        else
        {
            return value;
        }
    }

    private void PushCorrectButton(int[] playerCardsIndexes, int[] enemyCardsIndexes)
    {
        int? removeShapeNum = null;
        if (_removeIndex != null)
        {
            if ((_removeIndex[1] == 1 && playerCardsIndexes.Contains(_removeIndex[2])) ||
                (_removeIndex[1] == 0 && enemyCardsIndexes.Contains(_removeIndex[2])))
            {
                removeShapeNum = _removeIndex[0];
            }
        }

        Dictionary<int, int[]>[] cardUnderRemovedDicts = GetCardUnderRemovedDicts(removeShapeNum);
        photonView.RPC(nameof(PickUpDiscardPile), RpcTarget.Others, enemyCardsIndexes, playerCardsIndexes,
            removeShapeNum, cardUnderRemovedDicts[1], cardUnderRemovedDicts[0]);
        EnemyPickUpDiscardPile(playerCardsIndexes, enemyCardsIndexes, removeShapeNum, cardUnderRemovedDicts[0],
            cardUnderRemovedDicts[1]);
        TurnManager.Instance.SetIsMyTurn(false);
    }

    private void PushWrongButton()
    {
        int[] playerCardsIndexes = _playerCards
            .Select((playerCard, index) => new { Index = index, PlayerCard = playerCard })
            .Where(item => item.PlayerCard != null)
            .Select(item => item.Index)
            .ToArray();
        int[] enemyCardsIndexes = _enemyCards
            .Select((enemyCard, index) => new { Index = index, EnemyCard = enemyCard })
            .Where(item => item.EnemyCard != null)
            .Select(item => item.Index)
            .ToArray();
        int? removeShapeNum = null;
        if (_removeIndex != null)
        {
            if ((_removeIndex[1] == 1 && playerCardsIndexes.Contains(_removeIndex[2])) ||
                (_removeIndex[1] == 0 && enemyCardsIndexes.Contains(_removeIndex[2])))
            {
                removeShapeNum = _removeIndex[0];
            }
        }

        Dictionary<int, int[]>[] cardUnderRemovedDicts = GetCardUnderRemovedDicts(removeShapeNum);
        PickUpDiscardPile(playerCardsIndexes, enemyCardsIndexes, removeShapeNum, cardUnderRemovedDicts[0],
            cardUnderRemovedDicts[1]);
        photonView.RPC(nameof(EnemyPickUpDiscardPile), RpcTarget.Others, enemyCardsIndexes, playerCardsIndexes,
            removeShapeNum, cardUnderRemovedDicts[1], cardUnderRemovedDicts[0]);
        TurnManager.Instance.SetIsMyTurn(true);
    }

    private Dictionary<int, int[]>[] GetCardUnderRemovedDicts(int? removeShapeNum)
    {
        int[] removeIndexes = _playerCards
            .Select((playerCard, cardIndex) => new { Index = cardIndex, PlayerCard = playerCard })
            .Where(item => item.PlayerCard != null)
            .Where(item => item.PlayerCard.ShapeNum == removeShapeNum)
            .Select(item => item.Index)
            .ToArray();
        Dictionary<int, int[]> playerCardUnderRemovedDict =
            new Dictionary<int, int[]>(removeIndexes.Length);
        foreach (var i in removeIndexes)
        {
            // 一枚下のカードを探して表示し、カードも入れ替える
            int cardDataNum;
            int[] cnNums = { 0, 0 };
            ExceptCardData(ConfigConstants.CardEffect.Exchange, false);
            ExceptCardData(ConfigConstants.CardEffect.Substitute, true);
            ExceptCardData(ConfigConstants.CardEffect.Remove, true);
            cardDataNum = DecideCardDataNumber();
            if (_cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Exchange ||
                _cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Substitute)
            {
                List<int> colorIndexes = new List<int>(3) { 0, 1, 2 };
                cnNums[0] = UnityEngine.Random.Range(0, 3);
                colorIndexes.RemoveAt(cnNums[0]);
                cnNums[1] = colorIndexes[UnityEngine.Random.Range(0, 2)];
            }
            else if (_cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Illusion)
            {
                cnNums[0] = UnityEngine.Random.Range(0, 3);
            }

            List<int[]> filteredList = _playerPlayedData.Where(data => data[0] == i).ToList();
            int[] figureData = filteredList.ElementAtOrDefault(filteredList.Count - 2)?
                .Skip(1)
                .Take(2)
                .ToArray() ?? new int[] { DictionaryConstants.ByteMax, DictionaryConstants.ByteMax };
            int[] data = new int[5];
            data[0] = cardDataNum;
            Array.Copy(figureData, 0, data, 1, 2);
            Array.Copy(cnNums, 0, data, 3, 2);
            playerCardUnderRemovedDict.Add(i, data);
        }

        removeIndexes = _enemyCards
            .Select((enemyCard, index) => new { Index = index, EnemyCard = enemyCard })
            .Where(item => item.EnemyCard != null)
            .Where(item => item.EnemyCard.ShapeNum == removeShapeNum)
            .Select(item => item.Index)
            .ToArray();
        Dictionary<int, int[]> enemyCardUnderRemovedDict =
            new Dictionary<int, int[]>(removeIndexes.Length);
        foreach (var i in removeIndexes)
        {
            // 一枚下のカードを探して表示し、カードも入れ替える
            int cardDataNum;
            int[] cnNums = { 0, 0 };
            ExceptCardData(ConfigConstants.CardEffect.Exchange, false);
            ExceptCardData(ConfigConstants.CardEffect.Substitute, true);
            ExceptCardData(ConfigConstants.CardEffect.Remove, true);
            cardDataNum = DecideCardDataNumber();
            if (_cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Exchange ||
                _cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Substitute)
            {
                List<int> colorIndexes = new List<int>(3) { 0, 1, 2 };
                cnNums[0] = UnityEngine.Random.Range(0, 3);
                colorIndexes.RemoveAt(cnNums[0]);
                cnNums[1] = colorIndexes[UnityEngine.Random.Range(0, 2)];
            }
            else if (_cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Illusion)
            {
                cnNums[0] = UnityEngine.Random.Range(0, 3);
            }

            List<int[]> filteredList = _enemyPlayedData.Where(data => data[0] == i).ToList();
            int[] figureData = filteredList.ElementAtOrDefault(filteredList.Count - 2)?
                .Skip(1)
                .Take(2)
                .ToArray() ?? new int[] { DictionaryConstants.ByteMax, DictionaryConstants.ByteMax };
            int[] data = new int[5];
            data[0] = cardDataNum;
            Array.Copy(figureData, 0, data, 1, 2);
            Array.Copy(cnNums, 0, data, 3, 2);
            enemyCardUnderRemovedDict.Add(i, data);
        }

        return new Dictionary<int, int[]>[]
        {
            playerCardUnderRemovedDict,
            enemyCardUnderRemovedDict
        };
    }

    public void CheckIfDeckEmpty()
    {
        if (PlayerDeckNum == 0)
        {
            if (EnemyDeckNum == 0)
            {
                OnDeadlock();
                return;
            }

            TurnManager.Instance.SetIsMyTurn(false);
        }
    }

    private void OnDeadlock()
    {
        List<int[]> playedData = _playerPlayedData.Select(arr => new int[] { arr[1], arr[2] })
            .Concat(_enemyPlayedData.Select(arr => new int[] { arr[1], arr[2] }))
            .ToList();
        int leftDataNum = playedData.Count;
        List<int> playerDataIndexes = new List<int>(leftDataNum);
        for (int i = 0; i < leftDataNum; i++)
        {
            playerDataIndexes.Add(i);
        }

        while (leftDataNum-- > _playerPlayedData.Count)
        {
            int index = UnityEngine.Random.Range(0, leftDataNum);
            playerDataIndexes.RemoveAt(index);
        }

        if (_playerDeckData.Count != 0 || _enemyDeckData.Count != 0)
        {
            Debug.LogError("Deck Should Be Empty");
        }

        for (int i = 0; i < playedData.Count; i++)
        {
            if (playerDataIndexes.Contains(i))
            {
                _playerDeckData.Add(playedData[i]);
            }
            else
            {
                _enemyDeckData.Add(playedData[i]);
            }
        }

        PlayerDeckNum += _playerPlayedData.Count;
        EnemyDeckNum += _enemyPlayedData.Count;
        _playerPlayedData.Clear();
        _enemyPlayedData.Clear();
        Dictionary<int, int[]> playerDeckDataDic = new Dictionary<int, int[]>(PlayerDeckNum);
        Dictionary<int, int[]> enemyDeckDataDic = new Dictionary<int, int[]>(EnemyDeckNum);
        for (int i = 0; i < PlayerDeckNum; i++)
        {
            playerDeckDataDic.Add(i, _playerDeckData[i]);
        }

        for (int i = 0; i < EnemyDeckNum; i++)
        {
            enemyDeckDataDic.Add(i, _enemyDeckData[i]);
        }

        photonView.RPC(nameof(ReceiveDecksData), RpcTarget.Others, enemyDeckDataDic, playerDeckDataDic, true);
        DealAllCardsOnTables();
    }

    private async void DealAllCardsOnTables()
    {
        Card[] playerCards = _playerCards
            .Where(playerCard => playerCard != null)
            .ToArray();
        Card[] enemyCards = _enemyCards
            .Where(enemyCard => enemyCard != null)
            .ToArray();
        foreach (var playerCard in playerCards)
        {
            playerCard.GatherAndDeal();
        }

        foreach (var enemyCard in enemyCards)
        {
            enemyCard.GatherAndDeal();
        }

        Array.Clear(_playerCards, 0, _playerCards.Length);
        Array.Clear(_enemyCards, 0, _enemyCards.Length);
        Array.Clear(_playerCardData, 0, _playerCardData.Length);
        Array.Clear(_enemyCardData, 0, _enemyCardData.Length);
        // 美しくない
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsCardPlayingKey], 0 } }
        );
    }

    private void CheckExistsWinner()
    {
        if (PlayerDeckNum == 0 && _playerPlayedData.Count == 0)
        {
            _matchOverPanel.gameObject.SetActive(true);
            _matchOverPanel.SetResultText(true);
        }
        else if (EnemyDeckNum == 0 && _enemyPlayedData.Count == 0)
        {
            _matchOverPanel.gameObject.SetActive(true);
            _matchOverPanel.SetResultText(false);
        }
    }

    [PunRPC]
    public void ApplyExchangeToMyCards(int color0, int color1)
    {
        Card[] playerCards = _playerCards
            .Where(playerCard => playerCard != null)
            .ToArray();
        foreach (var playerCard in playerCards)
        {
            playerCard.ExchangeButton(color0, color1);
        }
    }

    [PunRPC]
    private void ReceiveDecksData(Dictionary<int, int[]> playerDeckDataDic, Dictionary<int, int[]> enemyDeckDataDic,
        bool isCalledFromOnDeadlock)
    {
        PlayerDeckNum += playerDeckDataDic.Count;
        EnemyDeckNum += enemyDeckDataDic.Count;
        _playerPlayedData.Clear();
        _enemyPlayedData.Clear();
        for (int i = 0; i < playerDeckDataDic.Count; i++)
        {
            _playerDeckData.Add(playerDeckDataDic[i]);
        }

        for (int i = 0; i < enemyDeckDataDic.Count; i++)
        {
            _enemyDeckData.Add(enemyDeckDataDic[i]);
        }

        if (isCalledFromOnDeadlock)
        {
            DealAllCardsOnTables();
        }
    }

    [PunRPC]
    private void PlayEnemyCard(int cardDataNum, int[] figureData, int[] cnNums, int tableNum)
    {
        if (_enemyCards[tableNum] != null)
        {
            Destroy(_enemyCards[tableNum].gameObject);
        }

        (_enemyCards[tableNum], _enemyCardData[tableNum]) = GenerateCard(cardDataNum, figureData, cnNums, tableNum, false);
        _enemyTables[tableNum].SetCardPos(_enemyCards[tableNum]);
        if (_enemyCardData[tableNum].Effect == ConfigConstants.CardEffect.Exchange)
        {
            ApplyExchangeToMyCards(cnNums[0], cnNums[1]);
        }

        if (_enemyCardData[tableNum].Effect == ConfigConstants.CardEffect.Remove)
        {
            _removeIndex = new[]
            {
                _enemyCards[tableNum].ShapeNum,
                0,
                tableNum
            };
        }

        _enemyPlayedData.Add(new int[] { tableNum, figureData[0], figureData[1] });
        EnemyDeckNum--;
        _enemyDeckData.RemoveAll(data => data.SequenceEqual(figureData));
    }

    [PunRPC]
    private async void PickUpDiscardPile(int[] playerCardsIndexes, int[] enemyCardsIndexes, int? removeShapeNum,
        Dictionary<int, int[]> playerCardUnderRemovedDict,
        Dictionary<int, int[]> enemyCardUnderRemovedDict)
    {
        if (removeShapeNum != null)
        {
            _removeIndex = null;
        }

        int removeCount = _playerDeckData.Count(data => data[0] == removeShapeNum);
        if (removeCount > 0)
        {
            // Animation
            _playerDeckData.RemoveAll(data => data[0] == removeShapeNum);
            PlayerDeckNum -= removeCount;
        }

        removeCount = _enemyDeckData.Count(data => data[0] == removeShapeNum);
        if (removeCount > 0)
        {
            // Animation
            _enemyDeckData.RemoveAll(data => data[0] == removeShapeNum);
            EnemyDeckNum -= removeCount;
        }

        foreach (var pair in playerCardUnderRemovedDict)
        {
            int i = pair.Key;
            int cardDataNum = pair.Value[0];
            int[] figureData = new int[2];
            Array.Copy(pair.Value, 1, figureData, 0, 2);
            int[] cnNums = new int[2];
            Array.Copy(pair.Value, 3, cnNums, 0, 2);
            GameObject cardGameObject = _playerCards[i].gameObject;
            _playerCards[i].transform.DOMove(_playerRemovePoint.transform.position, 1f).SetEase(Ease.Linear)
                .OnComplete(() => { Destroy(cardGameObject); });
            if (figureData.Any(x => x != DictionaryConstants.ByteMax))
            {
                (_playerCards[i], _playerCardData[i]) = GenerateCard(cardDataNum, figureData, cnNums, i, true);
                Card card = _playerCards[i];
                Card[] enemyExchangeCards = _enemyCards
                    .Select((enemyCard, index) => new { Index = index, EnemyCard = enemyCard })
                    .Where(item => item.EnemyCard != null)
                    .Where(item => _enemyCardData[item.Index].Effect == ConfigConstants.CardEffect.Exchange)
                    .Select(item => item.EnemyCard)
                    .ToArray();
                foreach (var enemyExchangeCard in enemyExchangeCards)
                {
                    card.ExchangeButton(enemyExchangeCard.ColorArgs[0], enemyExchangeCard.ColorArgs[1]);
                }

                if (_playerCardData[i].Effect == ConfigConstants.CardEffect.Remove)
                {
                    _removeIndex = new[]
                    {
                        _playerCards[i].ShapeNum,
                        1,
                        i
                    };
                }

                _playerTables[i].SetCardPos(card);
            }
            else
            {
                _playerCards[i] = null;
                _playerCardData[i] = null;
            }
        }

        foreach (var i in playerCardsIndexes)
        {
            if (_playerCards[i] != null)
            {
                _playerCards[i].PickedUp(_playerDeck, _playerTopHalfDeck, _playerBottomHalfDeck);
                _playerCards[i] = null;
                _playerCardData[i] = null;
            }

            List<int[]> playerPlayedData = _playerPlayedData.Where(data => data[0] == i)
                .Where(data => data[1] != removeShapeNum)
                .Select(data => new int[] { data[1], data[2] }).ToList();
            _playerDeckData.AddRange(playerPlayedData);
            PlayerDeckNum += playerPlayedData.Count;
            _playerPlayedData.RemoveAll(data => data[0] == i || data[1] == removeShapeNum);
        }

        foreach (var pair in enemyCardUnderRemovedDict)
        {
            int i = pair.Key;
            int cardDataNum = pair.Value[0];
            int[] figureData = new int[2];
            Array.Copy(pair.Value, 1, figureData, 0, 2);
            int[] cnNums = new int[2];
            Array.Copy(pair.Value, 3, cnNums, 0, 2);
            GameObject cardGameObject = _enemyCards[i].gameObject;
            _enemyCards[i].transform.DOMove(_playerRemovePoint.transform.position, 1f).SetEase(Ease.Linear)
                .OnComplete(() => { Destroy(cardGameObject); });
            if (figureData.Any(x => x != DictionaryConstants.ByteMax))
            {
                (_enemyCards[i], _enemyCardData[i]) = GenerateCard(cardDataNum, figureData, cnNums, i, false);
                _enemyTables[i].SetCardPos(_enemyCards[i]);
                if (_enemyCardData[i].Effect == ConfigConstants.CardEffect.Exchange)
                {
                    ApplyExchangeToMyCards(cnNums[0], cnNums[1]);
                }

                if (_enemyCardData[i].Effect == ConfigConstants.CardEffect.Remove)
                {
                    _removeIndex = new[]
                    {
                        _enemyCards[i].ShapeNum,
                        0,
                        i
                    };
                }
            }
            else
            {
                _enemyCards[i] = null;
                _enemyCardData[i] = null;
            }
        }

        foreach (var i in enemyCardsIndexes)
        {
            if (_enemyCards[i] != null)
            {
                _enemyCards[i].PickedUp(_playerDeck, _playerTopHalfDeck, _playerBottomHalfDeck);
                _enemyCards[i] = null;
                _enemyCardData[i] = null;
            }

            List<int[]> enemyPlayedData = _enemyPlayedData.Where(data => data[0] == i)
                .Where(data => data[1] != removeShapeNum)
                .Select(data => new int[] { data[1], data[2] }).ToList();
            _playerDeckData.AddRange(enemyPlayedData);
            PlayerDeckNum += enemyPlayedData.Count;
            _enemyPlayedData.RemoveAll(data => data[0] == i || data[1] == removeShapeNum);
        }

        CheckExistsWinner();
        // 美しくない
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsActionInProgressKey], 0 } }
        );
    }

    [PunRPC]
    private async void EnemyPickUpDiscardPile(int[] playerCardsIndexes, int[] enemyCardsIndexes, int? removeShapeNum,
        Dictionary<int, int[]> playerCardUnderRemovedDict,
        Dictionary<int, int[]> enemyCardUnderRemovedDict)
    {
        if (removeShapeNum != null)
        {
            _removeIndex = null;
        }

        int removeCount = _playerDeckData.Count(data => data[0] == removeShapeNum);
        if (removeCount > 0)
        {
            // Animation
            _playerDeckData.RemoveAll(data => data[0] == removeShapeNum);
            PlayerDeckNum -= removeCount;
        }

        removeCount = _enemyDeckData.Count(data => data[0] == removeShapeNum);
        if (removeCount > 0)
        {
            // Animation
            _enemyDeckData.RemoveAll(data => data[0] == removeShapeNum);
            EnemyDeckNum -= removeCount;
        }

        foreach (var pair in playerCardUnderRemovedDict)
        {
            int i = pair.Key;
            int cardDataNum = pair.Value[0];
            int[] figureData = new int[2];
            Array.Copy(pair.Value, 1, figureData, 0, 2);
            int[] cnNums = new int[2];
            Array.Copy(pair.Value, 3, cnNums, 0, 2);
            GameObject cardGameObject = _playerCards[i].gameObject;
            _playerCards[i].transform.DOMove(_enemyRemovePoint.transform.position, 1f).SetEase(Ease.Linear)
                .OnComplete(() => { Destroy(cardGameObject); });
            if (figureData.Any(x => x != DictionaryConstants.ByteMax))
            {
                (_playerCards[i], _playerCardData[i]) = GenerateCard(cardDataNum, figureData, cnNums, i, true);
                Card card = _playerCards[i];
                Card[] enemyExchangeCards = _enemyCards
                    .Select((enemyCard, index) => new { Index = index, EnemyCard = enemyCard })
                    .Where(item => item.EnemyCard != null)
                    .Where(item => _enemyCardData[item.Index].Effect == ConfigConstants.CardEffect.Exchange)
                    .Select(item => item.EnemyCard)
                    .ToArray();
                foreach (var enemyExchangeCard in enemyExchangeCards)
                {
                    card.ExchangeButton(enemyExchangeCard.ColorArgs[0], enemyExchangeCard.ColorArgs[1]);
                }

                if (_playerCardData[i].Effect == ConfigConstants.CardEffect.Remove)
                {
                    _removeIndex = new[]
                    {
                        _playerCards[i].ShapeNum,
                        1,
                        i
                    };
                }

                _playerTables[i].SetCardPos(card);
            }
            else
            {
                _playerCards[i] = null;
                _playerCardData[i] = null;
            }
        }

        foreach (var i in playerCardsIndexes)
        {
            if (_playerCards[i] != null)
            {
                _playerCards[i].PickedUp(_enemyDeck, _enemyTopHalfDeck, _enemyBottomHalfDeck);
                _playerCards[i] = null;
                _playerCardData[i] = null;
            }

            List<int[]> playerPlayedData = _playerPlayedData.Where(data => data[0] == i)
                .Where(data => data[1] != removeShapeNum)
                .Select(data => new int[] { data[1], data[2] }).ToList();
            _enemyDeckData.AddRange(playerPlayedData);
            EnemyDeckNum += playerPlayedData.Count;
            _playerPlayedData.RemoveAll(data => data[0] == i || data[1] == removeShapeNum);
        }

        foreach (var pair in enemyCardUnderRemovedDict)
        {
            int i = pair.Key;
            int cardDataNum = pair.Value[0];
            int[] figureData = new int[2];
            Array.Copy(pair.Value, 1, figureData, 0, 2);
            int[] cnNums = new int[2];
            Array.Copy(pair.Value, 3, cnNums, 0, 2);
            GameObject cardGameObject = _enemyCards[i].gameObject;
            _enemyCards[i].transform.DOMove(_enemyRemovePoint.transform.position, 1f).SetEase(Ease.Linear)
                .OnComplete(() => { Destroy(cardGameObject); });
            if (figureData.Any(x => x != DictionaryConstants.ByteMax))
            {
                (_enemyCards[i], _enemyCardData[i]) = GenerateCard(cardDataNum, figureData, cnNums, i, false);
                _enemyTables[i].SetCardPos(_enemyCards[i]);
                if (_enemyCardData[i].Effect == ConfigConstants.CardEffect.Exchange)
                {
                    ApplyExchangeToMyCards(cnNums[0], cnNums[1]);
                }

                if (_enemyCardData[i].Effect == ConfigConstants.CardEffect.Remove)
                {
                    _removeIndex = new[]
                    {
                        _enemyCards[i].ShapeNum,
                        0,
                        i
                    };
                }
            }
            else
            {
                _enemyCards[i] = null;
                _enemyCardData[i] = null;
            }
        }

        foreach (var i in enemyCardsIndexes)
        {
            if (_enemyCards[i] != null)
            {
                _enemyCards[i].PickedUp(_enemyDeck, _enemyTopHalfDeck, _enemyBottomHalfDeck);
                _enemyCards[i] = null;
                _enemyCardData[i] = null;
            }

            List<int[]> enemyPlayedData = _enemyPlayedData.Where(data => data[0] == i)
                .Where(data => data[1] != removeShapeNum)
                .Select(data => new int[] { data[1], data[2] }).ToList();
            _enemyDeckData.AddRange(enemyPlayedData);
            EnemyDeckNum += enemyPlayedData.Count;
            _enemyPlayedData.RemoveAll(data => data[0] == i || data[1] == removeShapeNum);
        }

        CheckExistsWinner();
        // 美しくない
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsActionInProgressKey], 0 } }
        );
    }
}