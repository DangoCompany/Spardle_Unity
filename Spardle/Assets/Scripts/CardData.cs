using UnityEngine;

[CreateAssetMenu]
public class CardData : ScriptableObject
{
    [SerializeField] private ConfigConstants.CardEffect _effect;

    public ConfigConstants.CardEffect Effect => _effect;

    [SerializeField] private string _effectName;

    public string EffectName => _effectName;

    [TextArea] [SerializeField] private string _effectDescr;

    public string EffectDescr => _effectDescr;
}