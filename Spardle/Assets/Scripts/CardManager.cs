using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private Table[] _enemyTables;
    [SerializeField] private Deck _playerDeck;
    [SerializeField] private Deck _enemyDeck;
    [SerializeField] private HalfDeck _playerTopHalfDeck;
    [SerializeField] private HalfDeck _playerBottomHalfDeck;
    [SerializeField] private HalfDeck _enemyTopHalfDeck;
    [SerializeField] private HalfDeck _enemyBottomHalfDeck;
    [SerializeField] private MatchOverPanel _matchOverPanel;
    private int _playerDeckNum = 0;
    public int PlayerDeckNum => _playerDeckNum;
    private int _enemyDeckNum = 0;

    public int EnemyDeckNum => _enemyDeckNum;

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
            _playerDeckNum += ConfigConstants.TotalCardsNum / 2;
            _enemyDeckNum += ConfigConstants.TotalCardsNum / 2;
            DealCardsToPlayers();
            Dictionary<int, int[]> playerDeckDataDic = new Dictionary<int, int[]>(_playerDeckNum);
            Dictionary<int, int[]> enemyDeckDataDic = new Dictionary<int, int[]>(_enemyDeckNum);
            for (int i = 0; i < _playerDeckNum; i++)
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
                if (!(bool)await GameProperties.GetCustomPropertyValueAsync(ConfigConstants.CustomPropertyKey
                        .IsMasterCardPlaying) &&
                    !(bool)await GameProperties.GetCustomPropertyValueAsync(ConfigConstants.CustomPropertyKey
                        .IsNonMasterCardPlaying) &&
                    !(bool)await GameProperties.GetCustomPropertyValueAsync(ConfigConstants.CustomPropertyKey
                        .IsSenderActionInProgress) &&
                    !(bool)await GameProperties.GetCustomPropertyValueAsync(ConfigConstants.CustomPropertyKey
                        .IsReceiverActionInProgress))
                {
                    if (CanPutActionButton())
                    {
                        GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsSenderActionInProgress, true);
                        GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsReceiverActionInProgress, true);
                        PushWrongButton();
                        return;
                    }

                    GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsMasterCardPlaying, true);
                    GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsNonMasterCardPlaying, true);
                    int cardDataNum;
                    int figureNum = UnityEngine.Random.Range(0, _playerDeckNum);
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

                        Destroy(_playerCards[_tableNumberToPlace].gameObject);
                    }

                    if (_playerCardData
                        .Where(data => data != null)
                        .Any(data => data.Effect == ConfigConstants.CardEffect.Exchange))
                    {
                        _exceptedCardDataNums.Add((int)ConfigConstants.CardEffect.Exchange);
                    }

                    if (_playerCardData
                            .Where(data => data != null)
                            .Any(data => data.Effect == ConfigConstants.CardEffect.Substitute) ||
                        _enemyCardData
                            .Where(data => data != null)
                            .Any(data => data.Effect == ConfigConstants.CardEffect.Substitute))
                    {
                        _exceptedCardDataNums.Add((int)ConfigConstants.CardEffect.Substitute);
                    }

                    cardDataNum = DecideCardDataNumber();
                    if (_cardData != null)
                    {
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
                    }

                    (_playerCards[_tableNumberToPlace], _playerCardData[_tableNumberToPlace]) =
                        GenerateCard(cardDataNum, figureData, cnNums, true);
                    int tableNumber = _tableNumberToPlace;
                    Card card = _playerCards[tableNumber];
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

                    card.OnClickCard
                        .Subscribe(delta => OnReceiveCardAction(delta, card, tableNumber))
                        .AddTo(card);
                    _playerTables[tableNumber].SetCardPos(card);
                    _playerPlayedData.Add(new int[] { tableNumber, figureData[0], figureData[1] });
                    _playerDeckNum--;
                    _playerDeckData.Remove(figureData);
                    photonView.RPC(nameof(PlayEnemyCard), RpcTarget.Others, cardDataNum, figureData, cnNums,
                        tableNumber);
                    TurnManager.Instance.SetIsMyTurn(false);
                }
            });
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

    private (Card, CardData) GenerateCard(int cardDataNum, int[] figureData, int[] cnNums, bool isMyCard)
    {
        CardData cardData = _cardData[cardDataNum];
        Sprite shape = _cardShapes[figureData[0]];
        Color32 color = DictionaryConstants.FigureColors[figureData[1]];
        Card card = Instantiate(_cardPrefab);
        card.Initialize(figureData[0], figureData[1], new int[] { cnNums[0], cnNums[1] }, isMyCard, cardData.Effect,
            _playerDeck, _enemyDeck);
        card.SetCard(cardData, shape, color, cnNums);
        return (card, cardData);
    }

    private void OnReceiveCardAction(Vector2 delta, Card card, int index)
    {
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
        photonView.RPC(nameof(PickUpDiscardPile), RpcTarget.Others, enemyCardsIndexes, playerCardsIndexes);
        EnemyPickUpDiscardPile(playerCardsIndexes, enemyCardsIndexes);
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
        PickUpDiscardPile(playerCardsIndexes, enemyCardsIndexes);
        photonView.RPC(nameof(EnemyPickUpDiscardPile), RpcTarget.Others, enemyCardsIndexes, playerCardsIndexes);
        TurnManager.Instance.SetIsMyTurn(true);
    }

    public void CheckIfDeckEmpty()
    {
        if (_playerDeckNum == 0)
        {
            if (_enemyDeckNum == 0)
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

        _playerDeckNum += _playerPlayedData.Count;
        _enemyDeckNum += _enemyPlayedData.Count;
        _playerPlayedData.Clear();
        _enemyPlayedData.Clear();
        Dictionary<int, int[]> playerDeckDataDic = new Dictionary<int, int[]>(_playerDeckNum);
        Dictionary<int, int[]> enemyDeckDataDic = new Dictionary<int, int[]>(_enemyDeckNum);
        for (int i = 0; i < _playerDeckNum; i++)
        {
            playerDeckDataDic.Add(i, _playerDeckData[i]);
        }

        for (int i = 0; i < _enemyDeckNum; i++)
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
        GameProperties.SetCustomPropertyValue(
            TurnManager.Instance.IsMasterClient
                ? ConfigConstants.CustomPropertyKey.IsMasterCardPlaying
                : ConfigConstants.CustomPropertyKey.IsNonMasterCardPlaying, false);
    }

    private void CheckExistsWinner()
    {
        if (_playerDeckNum == 0 && _playerPlayedData.Count == 0)
        {
            _matchOverPanel.gameObject.SetActive(true);
            _matchOverPanel.SetResultText(true);
        }
        else if (_enemyDeckNum == 0 && _enemyPlayedData.Count == 0)
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
    private void ReceiveDecksData(Dictionary<int, int[]> playerDeckDataDic, Dictionary<int, int[]> enemyDeckDataDic, bool isCalledFromOnDeadlock)
    {
        _playerDeckNum += playerDeckDataDic.Count;
        _enemyDeckNum += enemyDeckDataDic.Count;
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

        (_enemyCards[tableNum], _enemyCardData[tableNum]) = GenerateCard(cardDataNum, figureData, cnNums, false);
        _enemyTables[tableNum].SetCardPos(_enemyCards[tableNum]);
        if (_cardData[cardDataNum].Effect == ConfigConstants.CardEffect.Exchange)
        {
            ApplyExchangeToMyCards(cnNums[0], cnNums[1]);
        }

        _enemyPlayedData.Add(new int[] { tableNum, figureData[0], figureData[1] });
        _enemyDeckNum--;
        _enemyDeckData.RemoveAll(data => data.SequenceEqual(figureData));
    }

    [PunRPC]
    private async void PickUpDiscardPile(int[] playerCardsIndexes, int[] enemyCardsIndexes)
    {
        foreach (var i in playerCardsIndexes)
        {
            _playerCards[i].PickedUp(_playerDeck, _playerTopHalfDeck, _playerBottomHalfDeck);
            _playerCards[i] = null;
            List<int[]> playerPlayedData = _playerPlayedData.Where(data => data[0] == i)
                .Select(data => new int[] { data[1], data[2] }).ToList();
            _playerDeckData.AddRange(playerPlayedData);
            _playerDeckNum += playerPlayedData.Count;
            _playerPlayedData.RemoveAll(data => data[0] == i);
            _playerCardData[i] = null;
        }

        foreach (var i in enemyCardsIndexes)
        {
            _enemyCards[i].PickedUp(_playerDeck, _playerTopHalfDeck, _playerBottomHalfDeck);
            _enemyCards[i] = null;
            List<int[]> enemyPlayedData = _enemyPlayedData.Where(data => data[0] == i)
                .Select(data => new int[] { data[1], data[2] }).ToList();
            _playerDeckData.AddRange(enemyPlayedData);
            _playerDeckNum += enemyPlayedData.Count;
            _enemyPlayedData.RemoveAll(data => data[0] == i);
            _enemyCardData[i] = null;
        }

        CheckExistsWinner();
        // 美しくない
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsReceiverActionInProgress, false);
    }

    [PunRPC]
    private async void EnemyPickUpDiscardPile(int[] playerCardsIndexes, int[] enemyCardsIndexes)
    {
        foreach (var i in playerCardsIndexes)
        {
            _playerCards[i].PickedUp(_enemyDeck, _enemyTopHalfDeck, _enemyBottomHalfDeck);
            _playerCards[i] = null;
            List<int[]> playerPlayedData = _playerPlayedData.Where(data => data[0] == i)
                .Select(data => new int[] { data[1], data[2] }).ToList();
            _enemyDeckData.AddRange(playerPlayedData);
            _enemyDeckNum += playerPlayedData.Count;
            _playerPlayedData.RemoveAll(data => data[0] == i);
            _playerCardData[i] = null;
        }

        foreach (var i in enemyCardsIndexes)
        {
            _enemyCards[i].PickedUp(_enemyDeck, _enemyTopHalfDeck, _enemyBottomHalfDeck);
            _enemyCards[i] = null;
            List<int[]> enemyPlayedData = _enemyPlayedData.Where(data => data[0] == i)
                .Select(data => new int[] { data[1], data[2] }).ToList();
            _enemyDeckData.AddRange(enemyPlayedData);
            _enemyDeckNum += enemyPlayedData.Count;
            _enemyPlayedData.RemoveAll(data => data[0] == i);
            _enemyCardData[i] = null;
        }

        CheckExistsWinner();
        // 美しくない
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsSenderActionInProgress, false);
    }
}