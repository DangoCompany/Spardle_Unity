using System;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public bool IsMyTurn { get; private set; }
    private bool _isMasterClient;
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
        _isMasterClient = PhotonNetwork.IsMasterClient;
        this.ObserveEveryValueChanged(tm => tm.IsMyTurn)
            .Subscribe(_ => _turnPanel.FlipTurnPanel(Convert.ToInt32(IsMyTurn)));
        if (_isMasterClient)
        {
            GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsMasterClientTurnKey, UnityEngine.Random.Range(0, 2) == 1);
            GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsInProgressKey, false);
        }
    }

    public void UpdateMyTurn(bool isMasterClientTurn)
    {
        if (IsMyTurn == !(isMasterClientTurn ^ _isMasterClient))
        {
            Debug.LogError("Turn Should Be Changed Here");
        }
        IsMyTurn = !(isMasterClientTurn ^ _isMasterClient);
        if (IsMyTurn)
        {
            CardManager.Instance.CheckIfMyDeckEmpty();
        }
    }

    public void SetIsMyTurn(bool isMyTurn)
    {
        if (isMyTurn != IsMyTurn)
        {
            GameProperties.SetCustomPropertyValue(ConfigConstants.CustomPropertyKey.IsMasterClientTurnKey, !(isMyTurn ^ _isMasterClient));
        }
    }
}