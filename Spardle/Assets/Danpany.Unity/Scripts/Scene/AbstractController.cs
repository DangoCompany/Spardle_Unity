using System.Threading;
using Cysharp.Threading.Tasks;

namespace Danpany.Unity.Scripts.Scene
{
    public abstract class AbstractController<TService, THierarchy, TUI> : ISceneController<TService, THierarchy, TUI>
        where TService : ISceneService
        where THierarchy : ISceneHierarchy<TUI>
        where TUI : ISceneUI
    {
        public virtual UniTask OnSceneCreated(TService service, THierarchy hierarchy, CancellationToken cancelToken) =>
            UniTask.Yield(cancelToken);

        public virtual async UniTask Run(ISceneContext context, TService service, THierarchy hierarchy,
            CancellationToken cancelToken)
        {
            while (true)
            {
                cancelToken.ThrowIfCancellationRequested();
                await UniTask.Yield(cancelToken);
            }
        }

        public virtual UniTask
            OnSceneDestroyed(TService service, THierarchy hierarchy, CancellationToken cancelToken) =>
            UniTask.Yield(cancelToken);

        public virtual void Dispose()
        {
        }
    }
}
