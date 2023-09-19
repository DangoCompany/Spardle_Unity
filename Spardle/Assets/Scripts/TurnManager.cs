using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public bool IsMyTurn { get; private set; }
    public bool IsMasterClient { get; private set; }
    [SerializeField] private TurnPanel _turnPanel;

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
        IsMasterClient = PhotonNetwork.IsMasterClient;
        this.ObserveEveryValueChanged(tm => tm.IsMyTurn)
            .Subscribe(_ => _turnPanel.FlipTurnPanel(Convert.ToInt32(IsMyTurn)));
        if (IsMasterClient)
        {
            GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsMasterClientTurnKey,
                UnityEngine.Random.Range(0, 2) == 1);
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsCardPlayingKey], 0 } }
            );
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsWrongPlayingKey], 0 } }
            );
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsActionInProgressKey], 0 } }
            );
        }
    }

    public void UpdateMyTurn(bool isMasterClientTurn)
    {
        IsMyTurn = !(isMasterClientTurn ^ IsMasterClient);
        if (IsMyTurn)
        {
            CardManager.Instance.CheckIfDeckEmpty();
        }
        
        if (CardManager.Instance.PlayerDeckNum != 0 || !IsMyTurn)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new Hashtable() { { DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsCardPlayingKey], 0 } }
            );
        }
    }

    public void SetIsMyTurn(bool isMyTurn)
    {
        GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsMasterClientTurnKey,
            !(isMyTurn ^ IsMasterClient));
    }
}