using System.Threading;
using Cysharp.Threading.Tasks;
using Danpany.Unity.Scripts.Scene;
using UniRx;

namespace Spardle.Unity.Scripts.Scenes.Title
{
    public class TitleController : AbstractController<TitleSceneService, TitleHierarchy, TitleUI>
    {
        public override async UniTask Run(ISceneContext context, TitleSceneService service, TitleHierarchy hierarchy, CancellationToken cancelToken)
        {
            var shouldConnectMaster = false;
            byte? maxPlayerCount = null;
            
            using (service.ObservableData.Subscribe(hierarchy.OnUpdated))
            using (hierarchy.UI.OnConnectMasterClicked.Subscribe(_ => shouldConnectMaster = true))
            using (hierarchy.UI.OnMatchmakeTwoClicked.Subscribe(max => maxPlayerCount = max))
            {
                while (true)
                {
                    if (shouldConnectMaster)
                    {
                        var result = await service.ConnectMasterAsync(cancelToken);
                        if (result)
                        {
                            UnityEngine.Debug.Log("Connected to master server (Controller)");
                        }
                        else {
                            UnityEngine.Debug.LogError("Failed to connect to master server");
                        }

                        shouldConnectMaster = false;
                    }
                    if (maxPlayerCount.HasValue)
                    {
                        var result = await service.MatchmakeAsync(maxPlayerCount.Value, cancelToken);
                        if (result)
                        {
                            await service.WaitForPlayersAsync(cancelToken);
                            UnityEngine.Debug.Log("Should change scene");
                        }
                        else {
                            UnityEngine.Debug.LogError("Failed to matchmake");
                        }

                        maxPlayerCount = null;
                    }
                    await UniTask.Yield(cancelToken);
                }
            }
        }
    }
}
