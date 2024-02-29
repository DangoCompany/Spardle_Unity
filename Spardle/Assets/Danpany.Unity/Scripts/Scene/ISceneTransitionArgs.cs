using System;
using VContainer;

namespace Danpany.Unity.Scripts.Scene
{
    public interface ISceneTransitionArgs
    {
    }

    public interface ISceneTransitionArgs<TController, TService, THierarchy, TUI> : ISceneTransitionArgs
        where TController : ISceneController<TService, THierarchy, TUI>
        where TService : ISceneService
        where THierarchy : ISceneHierarchy<TUI>
        where TUI : ISceneUI
    {
        (TController controller, TService service, Func<THierarchy> getHierarchy) GetSceneArgs(
            IObjectResolver objectResolver);
    }
}
