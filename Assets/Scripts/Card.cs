using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private static readonly string[] ColorsString =
    {
        //赤
        "<color=#e60033>赤色</color>",
        //緑
        "<color=#3eb370>緑色</color>",
        //青
        "<color=#0095d9>青色</color>"
    };
    [SerializeField] private Image _figure;
    [SerializeField] private Text _effectName;
    [SerializeField] private Text _effectDescr;
    private int[] _colorArgs;
    public int[] ColorArgs { get => _colorArgs; set => _colorArgs = value; }

    public void SetCard(CardData cardData, Sprite figure, Color32 color, int[] CnNums)
    {
        _figure.sprite = figure;
        _figure.color = color;
        _effectName.text = cardData.EffectName;
        _effectDescr.text = cardData.EffectDescr;
        _effectDescr.text = _effectDescr.text.Replace("C0", ColorsString[CnNums[0]]);
        _effectDescr.text = _effectDescr.text.Replace("C1", ColorsString[CnNums[1]]);
    }
}
