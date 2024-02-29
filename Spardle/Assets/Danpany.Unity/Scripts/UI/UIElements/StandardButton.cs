using System;
using System.Collections.Generic;
using Danpany.Unity.Scripts.UI.Extensions;
using UniRx;
using UnityEngine.UIElements;

namespace Danpany.Unity.Scripts.UI.UIElements
{
    public class StandardButton : VisualElement
    {
        private const string RootClassName = "danpany-standard-button";
        private static readonly string ButtonClassName = $"{RootClassName}__button";
        private static readonly string LabelClassName = $"{ButtonClassName}__label";

        private static string GetRootModifierClassName(string modifier) => $"{RootClassName}--{modifier}";
        private static string GetButtonModifierClassName(string modifier) => $"{ButtonClassName}--{modifier}";
        private static string GetLabelModifierClassName(string modifier) => $"{LabelClassName}--{modifier}";

        private readonly Button _button;
        private readonly Label _label;

        private string _modifier;

        public StandardButton()
        {
            AddToClassList(RootClassName);

            _button = new Button();
            _button.AddToClassList(ButtonClassName);
            Add(_button);

            _label = new Label();
            _label.AddToClassList(LabelClassName);
            _button.Add(_label);
        }

        public StandardButton(string text, string modifier) : this()
        {
            Text = text;
            Modifier = modifier;
        }

        public string Text
        {
            get => _label.text;
            private set => _label.text = value;
        }

        public string Modifier
        {
            get => _modifier;
            private set
            {
                if (!string.IsNullOrEmpty(_modifier))
                {
                    RemoveFromClassList(GetRootModifierClassName(_modifier));
                    _button.RemoveFromClassList(GetButtonModifierClassName(_modifier));
                    _label.RemoveFromClassList(GetLabelModifierClassName(_modifier));
                }

                _modifier = value;
                AddToClassList(GetRootModifierClassName(_modifier));
                _button.AddToClassList(GetButtonModifierClassName(_modifier));
                _label.AddToClassList(GetLabelModifierClassName(_modifier));
            }
        }

        public IObservable<Unit> OnClicked => _button.OnClickAsObservable();

        public void SetButtonEnabled(bool enabled)
        {
            _button.SetEnabled(enabled);
        }
        
        public new class UxmlFactory : UxmlFactory<StandardButton, UxmlTraits> {}
        
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription { name = "text" };
            private UxmlStringAttributeDescription _modifier = new UxmlStringAttributeDescription { name = "modifier" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var text = _text.GetValueFromBag(bag, cc);
                var modifier = _modifier.GetValueFromBag(bag, cc);
                var standardButton = (StandardButton)ve;
                standardButton.Text = text;
                standardButton.Modifier = modifier;
            }
        }
    }
}
