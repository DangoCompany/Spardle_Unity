using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Danpany.Unity.Scripts.Scene;
using Spardle.Unity.Scripts.Features.Matchmaking;
using UniRx;
using VContainer;

namespace Spardle.Unity.Scripts.Scenes.Title
{
    public class TitleSceneService : AbstractSceneService
    {
        private readonly MatchmakingService _matchmakingService;

        [Inject]
        public TitleSceneService(MatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }
        
        public IObservable<TitleObservableData> ObservableData { get; private set; } = new Subject<TitleObservableData>();

        public async UniTask<bool> ConnectMasterAsync(CancellationToken cancelToken)
        {
            return await _matchmakingService.ConnectMasterAsync(cancelToken);
        }
        public async UniTask<bool> MatchmakeAsync(byte maxPlayerCount, CancellationToken cancelToken)
        {
            return await _matchmakingService.MatchmakeRandomlyAsync(maxPlayerCount, cancelToken);
        }
        public async UniTask WaitForPlayersAsync(CancellationToken cancelToken)
        {
            await _matchmakingService.WaitForPlayersAsync(cancelToken);
        }
    }
}
