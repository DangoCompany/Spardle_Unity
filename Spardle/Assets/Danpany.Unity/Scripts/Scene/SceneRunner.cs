using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine.SceneManagement;

namespace Danpany.Unity.Scripts.Scene
{
    public class SceneRunner
    {
        private readonly Stack<Func<ISceneContext, CancellationToken, UniTask>> _runSceneAsyncStack = new();
        private readonly BehaviorSubject<bool> _onLoadingToggled = new(true);
        private Func<ISceneContext, CancellationToken, UniTask> _nextRunSceneAsync;
        private Func<ISceneContext, CancellationToken, UniTask> _currentRunSceneAsync;
        public IObservable<bool> OnLoadingToggled => _onLoadingToggled;

        public async UniTask RunSceneAsync<TService, THierarchy, TUI>(
            ISceneContext context,
            ISceneController<TService, THierarchy, TUI> controller, TService service, Func<THierarchy> getHierarchy,
            bool skipLoading,
            CancellationToken cancelToken)
            where TService : ISceneService
            where THierarchy : ISceneHierarchy<TUI>
            where TUI : ISceneUI
        {
            using (controller)
            using (service)
            {
                var sceneName = controller.GetType().Name.Replace("Controller", "");

                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                var scene = SceneManager.GetSceneByName(sceneName);
                SceneManager.SetActiveScene(scene);

                using var hierarchy = getHierarchy();
                
                UnityEngine.Debug.Log("OnSceneCreated");
                await service.OnSceneCreated(cancelToken);
                await hierarchy.OnSceneCreated(cancelToken);
                await controller.OnSceneCreated(service, hierarchy, cancelToken);

                var cancelTokenSourceForRun = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);

                void OnNextRun(Func<ISceneContext, CancellationToken, UniTask> nextRunSceneAsync)
                {
                    cancelTokenSourceForRun.Cancel();
                    _nextRunSceneAsync = nextRunSceneAsync;
                }

                using (context.SubscribeNext(OnNextRun))
                {
                    try
                    {
                        if (!skipLoading)
                        {
                            _onLoadingToggled.OnNext(false);
                        }
                        
                        UnityEngine.Debug.Log("Run");
                        await controller.Run(context, service, hierarchy, cancelTokenSourceForRun.Token);
                    }
                    catch (OperationCanceledException e)
                    {
                        UnityEngine.Debug.Log(e);
                    }
                    finally
                    {
                        if (!skipLoading)
                        {
                            _onLoadingToggled.OnNext(true);
                        }
                    }
                }
                
                UnityEngine.Debug.Log("OnSceneDestroyed");
                await controller.OnSceneDestroyed(service, hierarchy, cancelToken);
                await service.OnSceneDestroyed(cancelToken);

                await SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.None);
            }
        }

        public async UniTask RunAsync(
            ISceneContext context,
            Func<ISceneContext, CancellationToken, UniTask> firstRunSceneAsync,
            CancellationToken cancelToken)
        {
            _nextRunSceneAsync = firstRunSceneAsync;
            while (_nextRunSceneAsync != null)
            {
                if (_currentRunSceneAsync != null)
                {
                    _runSceneAsyncStack.Push(_currentRunSceneAsync);
                }

                _currentRunSceneAsync = _nextRunSceneAsync;
                _nextRunSceneAsync = null;
                await _currentRunSceneAsync(context, cancelToken);
                await UniTask.Yield(cancelToken);
                
                if (_nextRunSceneAsync == null)
                {
                    _currentRunSceneAsync = null;
                    _nextRunSceneAsync = _runSceneAsyncStack.Pop();
                }
            }
        }
    }
}
