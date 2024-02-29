using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;

namespace Spardle.Unity.Scripts.Photon
{
    public class MatchmakeApi
    {
        public void ConnectMaster()
        {
            // PhotonServerSettings の設定内容を使ってマスターサーバーへ接続する
            PhotonNetwork.ConnectUsingSettings();
        }

        public void JoinRandomRoom(byte maxPlayerCount)
        {
            PhotonNetwork.JoinRandomRoom(null, maxPlayerCount);
        }
        
        public async UniTask WaitForPlayersAsync(CancellationToken cancelToken)
        {
            while (PhotonNetwork.CurrentRoom.MaxPlayers != PhotonNetwork.CurrentRoom.PlayerCount)
            {
                await UniTask.Yield(cancelToken);
            }
        }
    }
}
