using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Danpany.Unity.Scripts.Scene
{
    public interface ISceneService : IDisposable
    {
        UniTask OnSceneCreated(CancellationToken cancelToken);
        UniTask OnSceneDestroyed(CancellationToken cancelToken);
    }
}
