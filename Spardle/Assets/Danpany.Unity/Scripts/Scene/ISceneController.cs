using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Danpany.Unity.Scripts.Scene
{
    public interface ISceneController<TService, THierarchy, TUI> : IDisposable
        where TService : ISceneService
        where THierarchy : ISceneHierarchy<TUI>
        where TUI : ISceneUI
    {
        UniTask OnSceneCreated(TService service, THierarchy hierarchy, CancellationToken cancelToken);
        UniTask Run(ISceneContext context, TService service, THierarchy hierarchy, CancellationToken cancelToken);
        UniTask OnSceneDestroyed(TService service, THierarchy hierarchy, CancellationToken cancelToken);
    }
}
