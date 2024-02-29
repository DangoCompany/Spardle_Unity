using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace Danpany.Unity.Scripts.Scene
{
    public abstract class AbstractSceneUI : ISceneUI
    {
        private readonly VisualElement _root;

        protected AbstractSceneUI(VisualElement root)
        {
            _root = root;
        }

        public UniTask ShowErrorIfNeedAsync(IReadOnlyList<int> errorCodes, CancellationToken cancelToken)
        {
            return UniTask.Yield(cancelToken);
        }

        public virtual void Dispose()
        {
        }
    }
}
