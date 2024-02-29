using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using VContainer;

namespace Danpany.Unity.Scripts.Scene
{
    public class SceneContext : ISceneContext
    {
        private readonly IObjectResolver _objectResolver;
        private readonly SceneRunner _runner;
        private readonly Subject<Func<ISceneContext, CancellationToken, UniTask>> _on = new();
        private readonly Subject<ISceneTransitionArgs> _onTransit = new();

        public SceneContext(IObjectResolver objectResolver, SceneRunner runner)
        {
            _objectResolver = objectResolver;
            _runner = runner;
        }

        public IDisposable SubscribeNext(Action<Func<ISceneContext, CancellationToken, UniTask>> next)
        {
            return _on.Subscribe(next);
        }

        public IDisposable SubscribeTransition(Action<ISceneTransitionArgs> onTransit) =>
            _onTransit.Subscribe(onTransit);

        public void ChangeScene<TController, TService, THierarchy, TUI>(
            ISceneTransitionArgs<TController, TService, THierarchy, TUI> args)
            where TController : ISceneController<TService, THierarchy, TUI>
            where TService : ISceneService
            where THierarchy : ISceneHierarchy<TUI>
            where TUI : ISceneUI
        {
            _on.OnNext((context, cancelToken) =>
            {
                var (controller, service, getHierarchy) = args.GetSceneArgs(_objectResolver);
                return _runner.RunSceneAsync(context, controller, service, getHierarchy, false, cancelToken);
            });
            _onTransit.OnNext(args);
        }

        public async UniTask PushScene<TController, TService, THierarchy, TUI>(
            ISceneTransitionArgs<TController, TService, THierarchy, TUI> args,
            CancellationToken cancelToken)
            where TController : ISceneController<TService, THierarchy, TUI>
            where TService : ISceneService
            where THierarchy : ISceneHierarchy<TUI>
            where TUI : ISceneUI
        {
            var (controller, service, getHierarchy) = args.GetSceneArgs(_objectResolver);
            await _runner.RunSceneAsync(this, controller, service, getHierarchy, true, cancelToken);
        }
    }
}
