using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Spardle.Unity.Scripts.Features.Matchmaking
{
    public class MatchmakingService
    {
        private readonly MatchmakingServerProxy _serverProxy;

        [Inject]
        public MatchmakingService(MatchmakingServerProxy serverProxy)
        {
            _serverProxy = serverProxy;
        }

        public async UniTask<bool> ConnectMasterAsync(CancellationToken cancelToken)
        {
            return await _serverProxy.ConnectMasterAsync(cancelToken);
        }
        public async UniTask<bool> MatchmakeRandomlyAsync(byte maxPlayerCount, CancellationToken cancelToken)
        {
            return await _serverProxy.MatchmakeRandomlyAsync(maxPlayerCount, cancelToken);
        }
        public async UniTask WaitForPlayersAsync(CancellationToken cancelToken)
        {
            await _serverProxy.WaitForPlayersAsync(cancelToken);
        }
    }
}
