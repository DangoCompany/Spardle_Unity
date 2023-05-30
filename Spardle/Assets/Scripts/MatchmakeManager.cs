using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MatchmakeManager : MonoBehaviourPunCallbacks
{
    private bool _isInRoom;
    private bool _hasMatchmade;
    private byte _maxPlayerNum;

    private void Update()
    {
        if (_isInRoom && !_hasMatchmade)
        {
            if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                _hasMatchmade = true;
                SceneManager.LoadScene("Main");
            }
        }
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