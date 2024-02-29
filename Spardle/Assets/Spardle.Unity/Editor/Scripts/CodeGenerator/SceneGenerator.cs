using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Spardle.Unity.Editor.Scripts.CodeGenerator
{
    public class SceneGenerator : EditorWindow
    {
        [MenuItem("Tools/CodeGenerator/SceneGenerator")]
        public static void ShowWindow()
        {
            GetWindow<SceneGenerator>("SceneGenerator");
        }

        string _toBemCase(string word)
        {
            var snake = Regex.Replace(word, "([A-Za-z])([0-9]+)", "$1-$2");
            snake = Regex.Replace(snake, "([0-9]+)([A-Za-z])", "$1-$2");
            snake = Regex.Replace(snake, "([A-Z]+)([A-Z][a-z])", "$1-$2");
            snake = Regex.Replace(snake, "([a-z]+)([A-Z])", "$1-$2");
            return snake.ToLower();
        }
        
        string _getControllerCode(string sceneName)
        {
            return @$"using System.Threading;
using Cysharp.Threading.Tasks;
using Danpany.Unity.Scripts.Scene;
using UniRx;

namespace Spardle.Unity.Scripts.Scenes.{sceneName}
{{
    public class {sceneName}Controller : AbstractController<{sceneName}SceneService, {sceneName}Hierarchy, {sceneName}UI>
    {{
        public override async UniTask Run(ISceneContext context, {sceneName}SceneService service, {sceneName}Hierarchy hierarchy, CancellationToken cancelToken)
        {{
            using (service.ObservableData.Subscribe(hierarchy.OnUpdated))
            {{
                await UniTask.Delay(2_000, cancellationToken: cancelToken);
            }}
        }}
    }}
}}
";
        }
        string _getHierarchyCode(string sceneName)
        {
            return @$"using Danpany.Unity.Scripts.Scene;
using UnityEngine.UIElements;

namespace Spardle.Unity.Scripts.Scenes.{sceneName}
{{
    public class {sceneName}Hierarchy : AbstractSceneHierarchy<{sceneName}UI>
    {{
        protected override {sceneName}UI GetSceneUI(UIDocument uiDocument)
        {{
            return new {sceneName}UI(uiDocument.rootVisualElement);
        }}

        public void OnUpdated({sceneName}ObservableData observableData)
        {{
            UI.OnUpdated(observableData);
        }}
    }}
}}
";
        }
        string _getObservableDataCode(string sceneName)
        {
            return @$"namespace Spardle.Unity.Scripts.Scenes.{sceneName}
{{
    public struct {sceneName}ObservableData
    {{
    }}
}}
";
        }
        string _getServiceCode(string sceneName)
        {
            return @$"using System;
using Danpany.Unity.Scripts.Scene;
using UniRx;

namespace Spardle.Unity.Scripts.Scenes.{sceneName}
{{
    public class {sceneName}SceneService : AbstractSceneService
    {{
        public IObservable<{sceneName}ObservableData> ObservableData {{ get; private set; }} = new Subject<{sceneName}ObservableData>();
    }}
}}
";
        }
        string _getTransitionArgsCode(string sceneName)
        {
            return @$"using System;
using Danpany.Unity.Scripts.Scene;
using VContainer;

namespace Spardle.Unity.Scripts.Scenes.{sceneName}
{{
    public class {sceneName}TransitionArgs : ISceneTransitionArgs<{sceneName}Controller, {sceneName}SceneService, {sceneName}Hierarchy, {sceneName}UI>
    {{
        public ({sceneName}Controller controller, {sceneName}SceneService service, Func<{sceneName}Hierarchy> getHierarchy) GetSceneArgs(IObjectResolver objectResolver)
        {{
            var controller = new {sceneName}Controller();
            var service = new {sceneName}SceneService();
            return (controller, service, UnityEngine.Object.FindObjectOfType<{sceneName}Hierarchy>);
        }}
    }}
}}
";
        }
        string _getUICode(string sceneName)
        {
            return @$"using Danpany.Unity.Scripts.Scene;
using UnityEngine.UIElements;

namespace Spardle.Unity.Scripts.Scenes.{sceneName}
{{
    public class {sceneName}UI : AbstractSceneUI
    {{
        public {sceneName}UI(VisualElement root) : base(root)
        {{
        }}

        public void OnUpdated({sceneName}ObservableData observableData)
        {{
        }}
    }}
}}
";
        }

        string _getUssCode(string sceneNameBemCase)
        {
            return @$".scenes-{sceneNameBemCase} {{
    flex: 1;
}}
";
        }
        string _getUxmlCode(string sceneName, string sceneNameBemCase)
        {
            return @$"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:danpany=""Danpany.Unity.Scripts.UI.UIElements"" editor-extension-mode=""False"">
    <ui:Style src=""../../Global.uss"" />
    <ui:Style src=""./{sceneName}.uss"" />
    <danpany:SafeArea class=""scenes-{sceneNameBemCase}"">
        <ui:Label class=""scenes-{sceneNameBemCase}__label"" text=""{sceneName}"" />
    </danpany:SafeArea>
</ui:UXML>
";
        }

        void _generateSceneCodes(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException();
            }
            
            Debug.Log($"シーン {sceneName} に必要なコードを生成");

            var sceneNameBemCase = _toBemCase(sceneName);

            // スクリプト生成
            var scriptDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Spardle.Unity", "Scripts", "Scenes", sceneName);
            if (!Directory.Exists(scriptDir))
            {
                Directory.CreateDirectory(scriptDir);
            }
            File.WriteAllText(Path.Combine(scriptDir, $"{sceneName}Controller.cs"), _getControllerCode(sceneName));
            File.WriteAllText(Path.Combine(scriptDir, $"{sceneName}Hierarchy.cs"), _getHierarchyCode(sceneName));
            File.WriteAllText(Path.Combine(scriptDir, $"{sceneName}ObservableData.cs"), _getObservableDataCode(sceneName));
            File.WriteAllText(Path.Combine(scriptDir, $"{sceneName}SceneService.cs"), _getServiceCode(sceneName));
            File.WriteAllText(Path.Combine(scriptDir, $"{sceneName}TransitionArgs.cs"), _getTransitionArgsCode(sceneName));
            File.WriteAllText(Path.Combine(scriptDir, $"{sceneName}UI.cs"), _getUICode(sceneName));
            
            // UI 生成
            var uiDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Spardle.Unity", "UIs", "Scenes", sceneName);
            if (!Directory.Exists(uiDir))
            {
                Directory.CreateDirectory(uiDir);
            }
            File.WriteAllText(Path.Combine(uiDir, $"{sceneName}.uss"), _getUssCode(sceneNameBemCase));
            File.WriteAllText(Path.Combine(uiDir, $"{sceneName}.uxml"), _getUxmlCode(sceneName, sceneNameBemCase));
        }

        Type _getTypeByFullName(string assemblyQualifiedName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName == assemblyQualifiedName)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
        
        void _generateScene(string sceneName)
        {
            // シーン生成
            var sceneDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Spardle.Unity", "Scenes", $"{sceneName}.unity");
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(newScene);
            var hierarchyGameObject = new GameObject("Hierarchy");
            var uiDocument = hierarchyGameObject.AddComponent<UIDocument>();
            uiDocument.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(Path.Combine("Assets", "UI Toolkit", "PanelSettings.asset"));
            uiDocument.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine("Assets", "Spardle.Unity", "UIs", "Scenes", sceneName, $"{sceneName}.uxml"));
            hierarchyGameObject.AddComponent(_getTypeByFullName($"Spardle.Unity.Scripts.Scenes.{sceneName}.{sceneName}Hierarchy"));
            EditorSceneManager.SaveScene(newScene, sceneDir);
            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Concat(new[] { new EditorBuildSettingsScene(Path.GetRelativePath(Directory.GetCurrentDirectory(), newScene.path), true) })
                .ToArray();
            AssetDatabase.SaveAssets();
        }
        
        void OnEnable()
        {
            var label = new Label("シーン追加に必要な\nデフォルトスクリプト, シーンを生成します");

            var label1 = new Label("1. シーン名を入力して「デフォルトコード生成」");
            var textField = new TextField("シーン名");
            var codeButton = new Button(() => {
                _generateSceneCodes(textField.text);
            }) { text = "デフォルトコード生成" };

            var label2 = new Label("2. デフォルトコードを Unity が認識した後、\nシーン名を確認して「シーン生成」");
            var sceneButton = new Button(() => {
                _generateScene(textField.text);
            }) { text = "シーン生成" };

            rootVisualElement.Add(label);
            rootVisualElement.Add(textField);
            rootVisualElement.Add(label1);
            rootVisualElement.Add(codeButton);
            rootVisualElement.Add(label2);
            rootVisualElement.Add(sceneButton);
        }
    }
}
