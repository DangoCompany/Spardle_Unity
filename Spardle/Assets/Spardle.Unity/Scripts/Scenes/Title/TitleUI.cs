using System;
using Danpany.Unity.Scripts.Scene;
using Danpany.Unity.Scripts.UI.UIElements;
using UniRx;
using UnityEngine.UIElements;

namespace Spardle.Unity.Scripts.Scenes.Title
{
    public class TitleUI : AbstractSceneUI
    {
        private const string ConnectMasterButtonName = "connect-master-button";
        private const string MatchmakeTwoButtonName = "matchmake-two-button";

        private readonly StandardButton _connectMasterButton;
        private readonly StandardButton _matchmakeTwoButton;
        
        public TitleUI(VisualElement root) : base(root)
        {
            _connectMasterButton = root.Q<StandardButton>(ConnectMasterButtonName);
            _matchmakeTwoButton = root.Q<StandardButton>(MatchmakeTwoButtonName);
        }

        public IObservable<Unit> OnConnectMasterClicked => _connectMasterButton.OnClicked;
        public IObservable<byte> OnMatchmakeTwoClicked => _matchmakeTwoButton.OnClicked.Select(_ => (byte)2);

        public void OnUpdated(TitleObservableData observableData)
        {
        }
    }
}
