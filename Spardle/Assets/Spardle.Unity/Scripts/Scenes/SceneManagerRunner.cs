using System.Threading;
using Cysharp.Threading.Tasks;
using Danpany.Unity.Scripts.App;
using Danpany.Unity.Scripts.Scene;
using Spardle.Unity.Scripts.App.UI;
using Spardle.Unity.Scripts.Scenes.Title;
using UniRx;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Spardle.Unity.Scripts.Scenes
{
    public class SceneManagerRunner : IStartable
    {
        [Inject] private IObjectResolver _objectResolver;
        [Inject] private SceneLoadingUI _sceneLoadingUi;

        void IStartable.Start()
        {
            var sceneManager = new SceneRunner();
            var cancelToken = new CancellationToken();

            var args = new TitleTransitionArgs();
            var (scene, service, getHierarchy) = args.GetSceneArgs(_objectResolver);
            var context = new SceneContext(_objectResolver, sceneManager);

            async UniTask RunAsync()
            {
                try
                {
                    using (sceneManager.OnLoadingToggled.Subscribe(_sceneLoadingUi.SetDisplay))
                    {
                        await sceneManager.RunAsync(
                            context,
                            (c, ct) => sceneManager.RunSceneAsync(
                                c, scene, service, getHierarchy, false, ct),
                            cancelToken);
                    }
                }
                catch (RebootException)
                {
                    SceneManager.LoadScene("Manager");
                }
            }
            
            RunAsync().Forget();
        }
    }
}
