using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class OnlineManager : MonoBehaviourPunCallbacks
{
    private const int MaxPlayerNum = 2;
    private bool _isInRoom;
    private bool _hasMatchmade;

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

    public void OnClickTwoPlayer()
    {
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    // マスターサーバーへの接続が成功したときに呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    // ゲームサーバーへの接続が成功したときに呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        _isInRoom = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayerNum }, TypedLobby.Default);
    }
}