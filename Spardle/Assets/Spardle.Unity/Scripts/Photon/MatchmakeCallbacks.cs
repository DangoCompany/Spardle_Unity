using Photon.Pun;
using Photon.Realtime;

namespace Spardle.Unity.Scripts.Photon
{
    public class MatchmakeCallbacks : MonoBehaviourPunCallbacks
    {
        private bool? _isConnectedToMaster = null;
        private bool? _isJoinedRoom = null;
        
        public bool? IsConnectedToMaster => _isConnectedToMaster;
        public bool? IsJoinedRoom => _isJoinedRoom;
        public byte? MaxPlayerCount { get; set; }

        // マスターサーバーへの接続が成功したときに呼ばれるコールバック
        public override void OnConnectedToMaster()
        {
            UnityEngine.Debug.Log("Connected to master server");
            _isConnectedToMaster = true;
        }

        // Photon のサーバーから切断された時に呼ばれるコールバック
        public override void OnDisconnected(DisconnectCause cause)
        {
            UnityEngine.Debug.Log($"Disconnected from master server: {cause.ToString()}");
            _isConnectedToMaster = false;
        }
        
        // ゲームサーバーへの接続が成功したときに呼ばれるコールバック
        public override void OnJoinedRoom()
        {
            UnityEngine.Debug.Log("Joined room");
            _isJoinedRoom = true;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            if (MaxPlayerCount.HasValue)
            {
                PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayerCount.Value }, TypedLobby.Default);
            }
            else
            {
                UnityEngine.Debug.LogError("Max Player Count is undefined");
                _isJoinedRoom = false;
            }
        }
    
        // ルームの作成が成功した時に呼ばれるコールバック
        public override void OnCreatedRoom() {
            UnityEngine.Debug.Log("ルームの作成に成功しました");
            _isJoinedRoom = true;
        }
    
        // ルームの作成が失敗した時に呼ばれるコールバック
        public override void OnCreateRoomFailed(short returnCode, string message) {
            UnityEngine.Debug.LogError($"ルームの作成に失敗しました: {message}");
            _isJoinedRoom = false;
        }
    }
}
