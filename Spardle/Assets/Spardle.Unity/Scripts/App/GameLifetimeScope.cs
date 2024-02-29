using Spardle.Unity.Scripts.App.UI;
using Spardle.Unity.Scripts.Features.Matchmaking;
using Spardle.Unity.Scripts.Photon;
using Spardle.Unity.Scripts.Scenes;
using VContainer;
using VContainer.Unity;

namespace Spardle.Unity.Scripts.App
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<SceneLoadingUI>();
            
            builder.Register<MatchmakeApi>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<MatchmakeCallbacks>();
            
            builder.Register<MatchmakingService>(Lifetime.Singleton);
            builder.Register<MatchmakingServerProxy>(Lifetime.Singleton);

            builder.RegisterEntryPoint<SceneManagerRunner>();
        }
    }
}
