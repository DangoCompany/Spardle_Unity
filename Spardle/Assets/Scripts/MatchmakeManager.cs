using System.Threading;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class MatchmakeManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private SpardleTitle _spardleTitle;
    private bool _isInRoom;
    private bool _hasMatchmade;
    private byte _maxPlayerNum;
    private CancellationTokenSource _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();
        _spardleTitle.AnimateTitle(_cts.Token).Forget();
        this.UpdateAsObservable()
            .Where(_ => _isInRoom && !_hasMatchmade)
            .Where(_ => PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            .Subscribe(_ =>
            {
                _hasMatchmade = true;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                _cts.Cancel();
                _cts.Dispose();
                SceneManager.LoadScene("Main");
            });
    }

    public void OnClickConnectToMasterServer()
    {
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnClickTwoPlayer()
    {
        _maxPlayerNum = 2;
        PhotonNetwork.JoinRandomRoom(null, _maxPlayerNum);
    }

    // ゲームサーバーへの接続が成功したときに呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        _isInRoom = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = _maxPlayerNum }, TypedLobby.Default);
    }
    
    // ルームの作成が成功した時に呼ばれるコールバック
    public override void OnCreatedRoom() {
        Debug.Log("ルームの作成に成功しました");
    }
    
    // ルームの作成が失敗した時に呼ばれるコールバック
    public override void OnCreateRoomFailed(short returnCode, string message) {
        Debug.Log($"ルームの作成に失敗しました: {message}");
    }

    // マスターサーバーへの接続が成功したときに呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        Debug.Log("マスターサーバーに接続しました");
    }

    // Photonのサーバーから切断された時に呼ばれるコールバック
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"サーバーとの接続が切断されました: {cause.ToString()}");
    }
}