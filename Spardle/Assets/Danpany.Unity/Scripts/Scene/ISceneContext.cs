using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Danpany.Unity.Scripts.Scene
{
    public interface ISceneContext
    {
        IDisposable SubscribeNext(Action<Func<ISceneContext, CancellationToken, UniTask>> next);
        IDisposable SubscribeTransition(Action<ISceneTransitionArgs> onTransit);

        void ChangeScene<TController, TService, THierarchy, TUI>(
            ISceneTransitionArgs<TController, TService, THierarchy, TUI> args)
            where TController : ISceneController<TService, THierarchy, TUI>
            where TService : ISceneService
            where THierarchy : ISceneHierarchy<TUI>
            where TUI : ISceneUI;

        UniTask PushScene<TController, TService, THierarchy, TUI>(
            ISceneTransitionArgs<TController, TService, THierarchy, TUI> args, CancellationToken cancelToken)
            where TController : ISceneController<TService, THierarchy, TUI>
            where TService : ISceneService
            where THierarchy : ISceneHierarchy<TUI>
            where TUI : ISceneUI;
    }
}
