using Danpany.Unity.Scripts.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spardle.Unity.Scripts.App.UI
{
    public class SceneLoadingUI : MonoBehaviour
    {
        private VisualElement _root;

        private void Awake()
        {
            var uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;
        }

        public void SetDisplay(bool display)
        {
            _root.style.SetDisplay(display);
        }
    }
}
