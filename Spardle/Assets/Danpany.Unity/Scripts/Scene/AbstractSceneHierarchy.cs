using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Danpany.Unity.Scripts.Scene
{
    public abstract class AbstractSceneHierarchy<TSceneUI> : MonoBehaviour, ISceneHierarchy<TSceneUI>
        where TSceneUI : ISceneUI
    {
        private TSceneUI _ui;

        public TSceneUI UI
        {
            get
            {
                if (_ui == null)
                {
                    _ui = GetSceneUI(GetComponent<UIDocument>());
                }

                return _ui;
            }
        }

        protected abstract TSceneUI GetSceneUI(UIDocument uiDocument);
        public virtual UniTask OnSceneCreated(CancellationToken cancelToken) => UniTask.Yield(cancelToken);

        public virtual void Dispose()
        {
            _ui?.Dispose();
        }
    }
}
