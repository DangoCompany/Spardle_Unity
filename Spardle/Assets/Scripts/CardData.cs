using UnityEngine;

[CreateAssetMenu]
public class CardData : ScriptableObject
{
    [SerializeField] private CardEffect _effect;
    public CardEffect Effect { get => _effect; }
    public enum CardEffect
    {
        Exchange,
        Respond,
        Substitute
    }
    [SerializeField] private string _effectName;
    public string EffectName { get => _effectName; }
    [TextArea]
    [SerializeField] private string _effectDescr;
    public string EffectDescr { get => _effectDescr; }
}
