using System;
using UnityEngine;
using DG.Tweening;

public class HalfDeck : MonoBehaviour
{
    private static readonly float QuarterShuffleTime = 0.1f;
    [SerializeField] private Deck _deck;
    private Sequence _shuffle;

    private void Start()
    {
        if (transform.localPosition.y >= 0)
        {
            _shuffle = DOTween.Sequence()
                .Append(transform.DOLocalMove(new Vector3(64, 0, 0), QuarterShuffleTime).SetEase(Ease.OutSine))
                .AppendCallback(() => transform.SetSiblingIndex(1))
                .Append(transform.DOLocalMove(new Vector3(-4, -4, 0), QuarterShuffleTime).SetEase(Ease.InSine))
                .Append(transform.DOLocalMove(new Vector3(-64, 0, 0), QuarterShuffleTime).SetEase(Ease.OutSine))
                .AppendCallback(() => transform.SetSiblingIndex(2))
                .Append(transform.DOLocalMove(new Vector3(4, 4, 0), QuarterShuffleTime).SetEase(Ease.InSine))
                .OnComplete(() =>
                {
                    _deck.gameObject.SetActive(true);
                    gameObject.SetActive(false);
                });
        }
        else
        {
            _shuffle = DOTween.Sequence()
                .Append(transform.DOLocalMove(new Vector3(-64, 0, 0), QuarterShuffleTime).SetEase(Ease.OutSine))
                .Append(transform.DOLocalMove(new Vector3(4, 4, 0), QuarterShuffleTime).SetEase(Ease.InSine))
                .Append(transform.DOLocalMove(new Vector3(64, 0, 0), QuarterShuffleTime).SetEase(Ease.OutSine))
                .Append(transform.DOLocalMove(new Vector3(-4, -4, 0), QuarterShuffleTime).SetEase(Ease.InSine))
                .OnComplete(() =>
                {
                    _deck.gameObject.SetActive(true);
                    gameObject.SetActive(false);
                });
        }
    }

    public void Shuffle()
    {
        _shuffle.Play();
    }
}