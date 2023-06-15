using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SpardleTitle : MonoBehaviour
{
    [SerializeField] private ElementText _speed;
    [SerializeField] private ElementText _card;
    [SerializeField] private ElementText _battle;
    [SerializeField] private TitleText _spardle;

    public async UniTaskVoid AnimateTitle(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _speed.AnimateTextGeneration(ct).Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(ConfigConstants.TextGenerateSpeed));
        _card.AnimateTextGeneration(ct).Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(ConfigConstants.TextGenerateSpeed));
        await _battle.AnimateTextGeneration(ct);
        _speed.MoveToCenter(ct).Forget();
        await _battle.MoveToCenter(ct);
        _speed.DisableText();
        _card.DisableText();
        _battle.DisableText();
        _spardle.EnableText();
        _spardle.AnimateScaleBounce(ct).Forget();
    }
}