using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TitleText : MonoBehaviour
{
    private Text _text;
    private RectTransform _rectTransform;
    private void Awake()
    {
        _text = GetComponent<Text>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public async UniTask AnimateScaleBounce(CancellationToken ct)
    {
        await _rectTransform.DOScale(ConfigConstants.TitleEndScale, ConfigConstants.ScaleBounceDuration).SetEase(Ease.OutBounce)
            .ToUniTask(cancellationToken: ct);
    }
    
    public void EnableText()
    {
        _text.enabled = true;
    }
}
