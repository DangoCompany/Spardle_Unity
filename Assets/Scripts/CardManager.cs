using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public class CardManager : MonoBehaviourPunCallbacks
{
    //追加したら随時更新すべし
    private static readonly int FigureShapeNum = 4;
    private static readonly Color32[] FigureColors =
    {
        //赤
        new Color32(230, 0, 51, 255),
        //緑
        new Color32(62, 179, 112, 255),
        //青
        new Color32(0, 149, 217, 255)
    };
    //追加したら随時更新すべし
    private static readonly int CardDatasNum = 3;
    private static readonly int TotalCardsNum = FigureShapeNum * FigureColors.Length;
    private static readonly float RespondTime = 5f;
    private static readonly Dictionary<string, int> KeyToColorNumDic = new Dictionary<string, int>
    {
        { "z", 0 },
        { "x", 1 },
        { "c", 2 }
    };
    [SerializeField] private Sprite[] _images0;
    [SerializeField] private Sprite[] _images1;
    [SerializeField] private CardData[] _cardDatas;
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private Table _playerTable;
    [SerializeField] private Table _enemyTable;
    private int _cardsNum = TotalCardsNum;
    private int _playerDeckNum = TotalCardsNum / 2;
    private int _enemyDeckNum = TotalCardsNum / 2;
    private List<int[]> _playerDeckData = new List<int[]>(TotalCardsNum);
    private List<int[]> _enemyDeckData = new List<int[]>(TotalCardsNum);
    private int[] _playerFigureData;
    private int[] _enemyFigureData;
    private CardData _playerCardData;
    private CardData _enemyCardData;
    private List<int[]> _playerPlayedData = new List<int[]>(TotalCardsNum);
    private List<int[]> _enemyPlayedData = new List<int[]>(TotalCardsNum);
    private Card _playerCard;
    private Card _enemyCard;
    private bool[] _canPushEachRGBButton;
    private Coroutine _limitRespondTimeCoroutine;
    private int _colorNumToRespond;
    private bool _canRespond;
    private List<int> _exceptedCardDataNums = new List<int>(CardDatasNum);

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            DealCardsToPlayers();
            Dictionary<int, int[]> playerDeckDataDic = new Dictionary<int, int[]>(_playerDeckNum);
            Dictionary<int, int[]> enemyDeckDataDic = new Dictionary<int, int[]>(_enemyDeckNum);
            for(int i = 0; i < _playerDeckNum; i++)
            {
                playerDeckDataDic.Add(i, _playerDeckData[i]);
                enemyDeckDataDic.Add(i, _enemyDeckData[i]);
            }
            photonView.RPC(nameof(ReceiveDecksData), RpcTarget.Others, enemyDeckDataDic, playerDeckDataDic);
        }
    }
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                int cardDataNum;
                int figureNum = Random.Range(0, _playerDeckNum);
                int[] figureData = _playerDeckData[figureNum];
                int[] CnNums = { 0, 0 };
                if (_playerCard != null)
                {
                    Destroy(_playerCard.gameObject);
                }
                if(_enemyCardData != null)
                {
                    if(_enemyCardData.Effect == CardData.CardEffect.Substitute)
                    {
                        _exceptedCardDataNums.Add(2);
                    }
                }
                cardDataNum = DecideCardDataNumber();
                if (_cardDatas[cardDataNum].Effect == CardData.CardEffect.Exchange ||
                    _cardDatas[cardDataNum].Effect == CardData.CardEffect.Substitute)
                {
                    List<int> colorIndexes = new List<int>(3) { 0, 1, 2 };
                    CnNums[0] = Random.Range(0, 3);
                    colorIndexes.RemoveAt(CnNums[0]);
                    CnNums[1] = colorIndexes[Random.Range(0, 2)];
                }
                else if (_cardDatas[cardDataNum].Effect == CardData.CardEffect.Respond)
                {
                    CnNums[0] = Random.Range(0, 3);
                    CnNums[1] = Random.Range(0, 3);
                }
                (_playerCard, _playerCardData) = GenerateCard(cardDataNum, figureData, CnNums);
                _playerTable.SetCardPos(_playerCard);
                _playerFigureData = new int[]
                {
                figureData[0] % 2,
                Mathf.FloorToInt(figureData[0] / 2),
                figureData[1]
                };
                _playerPlayedData.Add(figureData);
                _cardsNum--;
                _playerDeckNum--;
                _playerDeckData.Remove(figureData);
                photonView.RPC(nameof(PlayEnemyCard), RpcTarget.Others, cardDataNum, figureData, CnNums);
                photonView.RPC(nameof(UpdateGameFlags), RpcTarget.All);
            }
            else if (KeyToColorNumDic.ContainsKey(Input.inputString))
            {
                int selectedColorNum = KeyToColorNumDic[Input.inputString];
                if(_enemyCardData != null)
                {
                    if (_enemyCardData.Effect == CardData.CardEffect.Exchange)
                    {
                        if (selectedColorNum == _enemyCard.ColorArgs[0])
                        {
                            selectedColorNum = _enemyCard.ColorArgs[1];
                        }
                        else if (selectedColorNum == _enemyCard.ColorArgs[1])
                        {
                            selectedColorNum = _enemyCard.ColorArgs[0];
                        }
                    }
                }
                if (_enemyCardData != null)
                {
                    if (_enemyCardData.Effect == CardData.CardEffect.Respond)
                    {
                        if (_enemyCard.ColorArgs[0] == selectedColorNum)
                        {
                            photonView.RPC(nameof(ReceiveRespondFlag), RpcTarget.Others);
                        }
                    }
                }
                if (_canRespond)
                {
                    if(_playerCardData != null)
                    {
                        if (_playerCardData.Effect == CardData.CardEffect.Substitute)
                        {
                            if (_colorNumToRespond == _playerCard.ColorArgs[0])
                            {
                                _colorNumToRespond = _playerCard.ColorArgs[1];
                            }
                        }
                    }
                    if (_enemyCardData != null)
                    {
                        if (_enemyCardData.Effect == CardData.CardEffect.Substitute)
                        {
                            if (_colorNumToRespond == _enemyCard.ColorArgs[0])
                            {
                                _colorNumToRespond = _enemyCard.ColorArgs[1];
                            }
                        }
                    }
                    if (selectedColorNum == _colorNumToRespond)
                    {
                        _canRespond = false;
                        StopCoroutine(_limitRespondTimeCoroutine);
                    }
                    else
                    {
                        if (_canPushEachRGBButton[selectedColorNum])
                        {
                            PushCorrectButton();
                        }
                        else
                        {
                            PushWrongButton();
                        }
                    }
                }
                else
                {
                    if (_canPushEachRGBButton[selectedColorNum])
                    {
                        PushCorrectButton();
                    }
                    else
                    {
                        PushWrongButton();
                    }
                }
            }
        }
    }
    private void DealCardsToPlayers()
    {
        int deckNum = TotalCardsNum / 2;
        int leftCardsNum = TotalCardsNum;
        List<int> playerCardIndexes = new List<int>(TotalCardsNum);
        for (int i = 0; i < leftCardsNum; i++)
        {
            playerCardIndexes.Add(i);
        }
        while(leftCardsNum-- > deckNum)
        {
            int index = Random.Range(0, leftCardsNum);
            playerCardIndexes.RemoveAt(index);
        }
        for (int i = 0; i < FigureShapeNum; i++)
        {
            for (int j = 0; j < FigureColors.Length; j++)
            {
                int[] data = { i, j };
                if(playerCardIndexes.Contains(i * FigureColors.Length + j))
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
    private int DecideCardDataNumber()
    {
        List<int> availableNums = new List<int>(CardDatasNum);
        for(int i = 0; i < CardDatasNum; i++)
        {
            availableNums.Add(i);
        }
        availableNums = availableNums.Except(_exceptedCardDataNums).ToList();
        _exceptedCardDataNums.Clear();
        return availableNums[Random.Range(0, availableNums.Count())];
    }
    private (Card, CardData) GenerateCard(int cardDataNum, int[] figureData, int[] CnNums)
    {
        CardData cardData = _cardDatas[cardDataNum];
        Sprite figure;
        if(figureData[0] % 2 == 0)
        {
            figure = _images0[figureData[0] / 2];
        }
        else
        {
            figure = _images1[(figureData[0] - 1) / 2];
        }
        Color32 color = FigureColors[figureData[1]];
        Card card = Instantiate(_cardPrefab);
        card.ColorArgs = new int[] { CnNums[0], CnNums[1] };
        card.SetCard(cardData, figure, color, CnNums);
        return (card, cardData);
    }
    private void PushCorrectButton()
    {
        photonView.RPC(nameof(PickUpDiscardPile), RpcTarget.Others);
        EnemyPickUpDiscardPile();
        photonView.RPC(nameof(UpdateGameFlags), RpcTarget.All);
    }
    private void PushWrongButton()
    {
        PickUpDiscardPile();
        photonView.RPC(nameof(EnemyPickUpDiscardPile), RpcTarget.Others);
        photonView.RPC(nameof(UpdateGameFlags), RpcTarget.All);
    }
    private IEnumerator LimitRespondTime()
    {
        yield return new WaitForSeconds(RespondTime);
        _canRespond = false;
        PickUpDiscardPile();
        photonView.RPC(nameof(EnemyPickUpDiscardPile), RpcTarget.Others);
        photonView.RPC(nameof(UpdateGameFlags), RpcTarget.All);
    }
    [PunRPC]
    private void ReceiveDecksData(Dictionary<int, int[]> playerDeckDataDic, Dictionary<int, int[]> enemyDeckDataDic)
    {
        for(int i = 0; i < playerDeckDataDic.Count; i++)
        {
            _playerDeckData.Add(playerDeckDataDic[i]);
            _enemyDeckData.Add(enemyDeckDataDic[i]);
        }
    }
    [PunRPC]
    private void PlayEnemyCard(int cardDataNum, int[] figureData, int[] CnNums)
    {
        if(_enemyCard != null)
        {
            Destroy(_enemyCard.gameObject);
        }
        (_enemyCard, _enemyCardData) = GenerateCard(cardDataNum, figureData, CnNums);
        _enemyTable.SetCardPos(_enemyCard);
        _enemyFigureData = new int[]
        {
            figureData[0] % 2,
            Mathf.FloorToInt(figureData[0] / 2),
            figureData[1]
        };
        _enemyPlayedData.Add(figureData);
        _cardsNum--;
        _enemyDeckNum--;
        for(int i = 0; i < _enemyDeckData.Count; i++)
        {
            if (_enemyDeckData[i].SequenceEqual(figureData))
            {
                _enemyDeckData.RemoveAt(i);
            }
        }
    }
    [PunRPC]
    private void UpdateGameFlags()
    {
        Debug.LogError("PlayerDeckNumber: " + _playerDeckNum);
        Debug.LogError("EnemyDeckNumber: " + _enemyDeckNum);
        _canPushEachRGBButton = new bool[] { false, false, false };
        if(_playerFigureData != null && _enemyFigureData != null)
        {
            if(_playerFigureData[0] != _enemyFigureData[0])
            {
                if(_playerFigureData[1] == _enemyFigureData[1])
                {
                    if(_playerFigureData[2] == _enemyFigureData[2])
                    {
                        _canPushEachRGBButton[_playerFigureData[2]] = true;
                    }
                    else
                    {
                        int[] tmp = { 0, 1, 2 };
                        int colorNum = tmp.Single(_ => _ != _playerFigureData[2] && _ != _enemyFigureData[2]);
                        _canPushEachRGBButton[colorNum] = true;
                    }
                }
            }
            if(_playerCardData.Effect == CardData.CardEffect.Substitute)
            {
                if (_canPushEachRGBButton[_playerCard.ColorArgs[0]])
                {
                    _canPushEachRGBButton[_playerCard.ColorArgs[0]] = false;
                    _canPushEachRGBButton[_playerCard.ColorArgs[1]] = true;
                }
            }
            else if(_enemyCardData.Effect == CardData.CardEffect.Substitute)
            {
                if (_canPushEachRGBButton[_enemyCard.ColorArgs[0]])
                {
                    _canPushEachRGBButton[_enemyCard.ColorArgs[0]] = false;
                    _canPushEachRGBButton[_enemyCard.ColorArgs[1]] = true;
                }
            }
        }
    }
    [PunRPC]
    private void PickUpDiscardPile()
    {
        _cardsNum += _playerPlayedData.Count + _enemyPlayedData.Count;
        _playerDeckNum += _playerPlayedData.Count + _enemyPlayedData.Count;
        _playerDeckData.AddRange(_playerPlayedData);
        _playerDeckData.AddRange(_enemyPlayedData);
        _playerFigureData = null;
        _enemyFigureData = null;
        _playerPlayedData.Clear();
        _enemyPlayedData.Clear();
        if(_playerCard != null)
        {
            Destroy(_playerCard.gameObject);
        }
        _playerCard = null;
        _playerCardData = null;
        if(_enemyCard != null)
        {
            Destroy(_enemyCard.gameObject);
        }
        _enemyCard = null;
        _enemyCardData = null;
    }
    [PunRPC]
    private void EnemyPickUpDiscardPile()
    {
        _cardsNum += _playerPlayedData.Count + _enemyPlayedData.Count;
        _enemyDeckNum += _playerPlayedData.Count + _enemyPlayedData.Count;
        _enemyDeckData.AddRange(_playerPlayedData);
        _enemyDeckData.AddRange(_enemyPlayedData);
        _playerFigureData = null;
        _enemyFigureData = null;
        _playerPlayedData.Clear();
        _enemyPlayedData.Clear();
        if (_playerCard != null)
        {
            Destroy(_playerCard.gameObject);
        }
        _playerCard = null;
        _playerCardData = null;
        if (_enemyCard != null)
        {
            Destroy(_enemyCard.gameObject);
        }
        _enemyCard = null;
        _enemyCardData = null;
    }
    [PunRPC]
    private void ReceiveRespondFlag()
    {
        _canRespond = true;
        _colorNumToRespond = _playerCard.ColorArgs[1];
        _limitRespondTimeCoroutine = StartCoroutine(LimitRespondTime());
    }
}
