using UnityEngine;

[CreateAssetMenu]
public class CardData : ScriptableObject
{
    [SerializeField] private CardEffect _effect;

    public CardEffect Effect => _effect;

    public enum CardEffect
    {
        Exchange,
        Illusion,
        None,
        Substitute
    }

    [SerializeField] private string _effectName;

    public string EffectName => _effectName;

    [TextArea] [SerializeField] private string _effectDescr;

    public string EffectDescr => _effectDescr;
}