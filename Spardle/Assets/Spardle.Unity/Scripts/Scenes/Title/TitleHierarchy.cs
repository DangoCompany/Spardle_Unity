using Danpany.Unity.Scripts.Scene;
using UnityEngine.UIElements;

namespace Spardle.Unity.Scripts.Scenes.Title
{
    public class TitleHierarchy : AbstractSceneHierarchy<TitleUI>
    {
        protected override TitleUI GetSceneUI(UIDocument uiDocument)
        {
            return new TitleUI(uiDocument.rootVisualElement);
        }

        public void OnUpdated(TitleObservableData observableData)
        {
            UI.OnUpdated(observableData);
        }
    }
}
