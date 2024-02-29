using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Danpany.Unity.Scripts.Scene
{
    public interface ISceneHierarchy<TSceneUI> : IDisposable
        where TSceneUI : ISceneUI
    {
        TSceneUI UI { get; }
        UniTask OnSceneCreated(CancellationToken cancelToken);
    }
}
