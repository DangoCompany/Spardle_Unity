using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Danpany.Unity.Scripts.UI.Extensions
{
    public static class UIElementsExtensions
    {
        public static IObservable<Unit> OnClickAsObservable(this Button source)
        {
            return Observable.FromEvent(
                h => source.clicked += h,
                h => source.clicked -= h);
        }
        
        public static void SetDisplay(this IStyle style, bool display)
        {
            style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void SetVisible(this IStyle style, bool visible)
        {
            style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        public static ValueAnimation<float> FadeIn(this VisualElement visualElement, TimeSpan duration,
            float toOpacity = 1)
        {
            visualElement.style.opacity = 0;
            visualElement.style.SetDisplay(true);
            visualElement.style.SetVisible(true);
            return visualElement.experimental.animation.Start(
                0, toOpacity, (int)duration.TotalMilliseconds, (ve, opacity) =>
                {
                    ve.style.opacity = opacity;
                });
        }

        public static ValueAnimation<float> FadeOut(this VisualElement visualElement, TimeSpan duration)
        {
            return visualElement.experimental.animation.Start(
                1, 0, (int)duration.TotalMilliseconds, (ve, opacity) =>
                {
                    ve.style.opacity = opacity;
                }).OnCompleted(() =>
            {
                visualElement.style.SetDisplay(false);
                visualElement.style.SetVisible(false);
            });
        }

        public static UniTask WaitAnimation(this IValueAnimation valueAnimation, CancellationToken cancelToken)
        {
            return UniTask.WaitWhile(() => valueAnimation.isRunning, cancellationToken: cancelToken);
        }
    }
}
