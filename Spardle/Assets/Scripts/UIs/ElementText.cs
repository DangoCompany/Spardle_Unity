using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ElementText : MonoBehaviour
{
    private Text _text;
    private int _textLength;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _text = GetComponent<Text>();
        _textLength = _text.text.Length;
        _rectTransform = GetComponent<RectTransform>();
    }

    public async UniTask AnimateTextGeneration(CancellationToken ct)
    {
        await _text.DOText(_text.text, ConfigConstants.TextGenerateSpeed * _textLength, scrambleMode:ScrambleMode.Uppercase).SetEase(Ease.Linear).ToUniTask(cancellationToken: ct);
    }
    
    public async UniTask MoveToCenter(CancellationToken ct)
    {
        _rectTransform.DOAnchorMin(new Vector2(_rectTransform.anchorMin.x, 0.35f), ConfigConstants.TextGenerateSpeed).SetEase(Ease.Linear).ToUniTask(cancellationToken: ct);
        await _rectTransform.DOAnchorMax(new Vector2(_rectTransform.anchorMax.x, 0.65f), ConfigConstants.TextGenerateSpeed).SetEase(Ease.Linear).ToUniTask(cancellationToken: ct);
    }

    public void DisableText()
    {
        _text.enabled = false;
    }
}
