using System;
using Danpany.Unity.Scripts.Scene;
using Spardle.Unity.Scripts.Features.Matchmaking;
using VContainer;

namespace Spardle.Unity.Scripts.Scenes.Title
{
    public class TitleTransitionArgs : ISceneTransitionArgs<TitleController, TitleSceneService, TitleHierarchy, TitleUI>
    {
        public (TitleController controller, TitleSceneService service, Func<TitleHierarchy> getHierarchy) GetSceneArgs(IObjectResolver objectResolver)
        {
            var matchmakingService = objectResolver.Resolve<MatchmakingService>();
            
            var controller = new TitleController();
            var service = new TitleSceneService(matchmakingService);
            return (controller, service, UnityEngine.Object.FindObjectOfType<TitleHierarchy>);
        }
    }
}
