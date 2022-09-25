using UnityEngine;
using DG.Tweening;

public class HalfDeck : MonoBehaviour
{
    private static readonly float QuarterShuffleTime = 0.1f;
    [SerializeField] private Deck _deck;

    private void Start()
    {
        Shuffle(1);
    }
    public void Shuffle(int shuffleCount)
    {
        Sequence sequence = DOTween.Sequence();
        if (transform.localPosition.y >= 0)
        {
            sequence
                .Append(transform.DOLocalMove(new Vector3(64, 0, 0), QuarterShuffleTime).SetEase(Ease.OutSine))
                .AppendCallback(() => transform.SetSiblingIndex(0))
                .Append(transform.DOLocalMove(new Vector3(-4, -4, 0), QuarterShuffleTime).SetEase(Ease.InSine))
                .Append(transform.DOLocalMove(new Vector3(-64, 0, 0), QuarterShuffleTime).SetEase(Ease.OutSine))
                .AppendCallback(() => transform.SetSiblingIndex(1))
                .Append(transform.DOLocalMove(new Vector3(4, 4, 0), QuarterShuffleTime).SetEase(Ease.InSine));
        }
        else
        {
            sequence
                .Append(transform.DOLocalMove(new Vector3(-64, 0, 0), QuarterShuffleTime).SetEase(Ease.OutSine))
                .Append(transform.DOLocalMove(new Vector3(4, 4, 0), QuarterShuffleTime).SetEase(Ease.InSine))
                .Append(transform.DOLocalMove(new Vector3(64, 0, 0), QuarterShuffleTime).SetEase(Ease.OutSine))
                .Append(transform.DOLocalMove(new Vector3(-4, -4, 0), QuarterShuffleTime).SetEase(Ease.InSine));
        }
        sequence.SetLoops(shuffleCount).OnComplete(() =>
        {
            _deck.gameObject.SetActive(true);
            gameObject.SetActive(false);
        });
        sequence.Play();
    }
}
