using System.Threading;
using Cysharp.Threading.Tasks;
using Spardle.Unity.Scripts.Photon;
using VContainer;

namespace Spardle.Unity.Scripts.Features.Matchmaking
{
    public class MatchmakingServerProxy
    {
        private readonly MatchmakeApi _matchmakeApi;
        private readonly MatchmakeCallbacks _matchmakeCallbacks;
        
        [Inject]
        public MatchmakingServerProxy(MatchmakeApi matchmakeApi, MatchmakeCallbacks matchmakeCallbacks)
        {
            _matchmakeApi = matchmakeApi;
            _matchmakeCallbacks = matchmakeCallbacks;
        }
        
        public async UniTask<bool> ConnectMasterAsync(CancellationToken cancelToken)
        {
            _matchmakeApi.ConnectMaster();
            while (!_matchmakeCallbacks.IsConnectedToMaster.HasValue)
            {
                await UniTask.Yield(cancelToken);
            }

            return _matchmakeCallbacks.IsConnectedToMaster.Value;
        }
        public async UniTask<bool> MatchmakeRandomlyAsync(byte maxPlayerCount, CancellationToken cancelToken)
        {
            _matchmakeCallbacks.MaxPlayerCount = maxPlayerCount;
            _matchmakeApi.JoinRandomRoom(maxPlayerCount);
            while (!_matchmakeCallbacks.IsJoinedRoom.HasValue)
            {
                await UniTask.Yield(cancelToken);
            }

            return _matchmakeCallbacks.IsJoinedRoom.Value;
        }
        public async UniTask WaitForPlayersAsync(CancellationToken cancelToken)
        {
            await _matchmakeApi.WaitForPlayersAsync(cancelToken);
        }
    }
}
