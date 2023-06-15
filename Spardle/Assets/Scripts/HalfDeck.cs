using UnityEngine;
using DG.Tweening;

public class HalfDeck : MonoBehaviour
{
    [SerializeField] private Deck _deck;
    private Sequence _shuffle;

    public void Shuffle()
    {
        CreateShuffleSequence();
        _shuffle.Play();
    }

    private void CreateShuffleSequence()
    {
        _shuffle = DOTween.Sequence();
        if (transform.localPosition.y >= 0)
        {
            _shuffle
                .Append(transform.DOLocalMove(new Vector3(64, 0, 0), ConfigConstants.QuarterShuffleTime)
                    .SetEase(Ease.OutSine))
                .AppendCallback(() => transform.SetSiblingIndex(1))
                .Append(transform.DOLocalMove(new Vector3(-4, -4, 0), ConfigConstants.QuarterShuffleTime)
                    .SetEase(Ease.InSine))
                .Append(transform.DOLocalMove(new Vector3(-64, 0, 0), ConfigConstants.QuarterShuffleTime)
                    .SetEase(Ease.OutSine))
                .AppendCallback(() => transform.SetSiblingIndex(2))
                .Append(transform.DOLocalMove(new Vector3(4, 4, 0), ConfigConstants.QuarterShuffleTime)
                    .SetEase(Ease.InSine));
        }
        else
        {
            _shuffle
                .Append(transform.DOLocalMove(new Vector3(-64, 0, 0), ConfigConstants.QuarterShuffleTime)
                    .SetEase(Ease.OutSine))
                .Append(transform.DOLocalMove(new Vector3(4, 4, 0), ConfigConstants.QuarterShuffleTime)
                    .SetEase(Ease.InSine))
                .Append(transform.DOLocalMove(new Vector3(64, 0, 0), ConfigConstants.QuarterShuffleTime)
                    .SetEase(Ease.OutSine))
                .Append(transform.DOLocalMove(new Vector3(-4, -4, 0), ConfigConstants.QuarterShuffleTime)
                    .SetEase(Ease.InSine));
        }

        _shuffle
            .OnComplete(() =>
            {
                _deck.gameObject.SetActive(true);
                gameObject.SetActive(false);
            });
    }
}