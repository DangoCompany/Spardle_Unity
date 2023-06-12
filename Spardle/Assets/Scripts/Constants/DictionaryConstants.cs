using UnityEngine;

public static class DictionaryConstants
{
    public static readonly string[] CustomPropertyKeysString =
    {
        "IsMasterClientTurn",
        "IsMasterCardPlaying",
        "IsNonMasterCardPlaying",
        "IsSenderActionInProgress",
        "IsReceiverActionInProgress"
    };

    public static readonly Color32[] FigureColors =
    {
        // 赤
        new Color32(230, 0, 51, 255),
        // 緑
        new Color32(62, 179, 112, 255),
        // 青
        new Color32(0, 149, 217, 255)
    };

    public static readonly string[] ColorsString =
    {
        // 赤
        "<color=#e60033>赤色</color>",
        // 緑
        "<color=#3eb370>緑色</color>",
        // 青
        "<color=#0095d9>青色</color>"
    };

    public static readonly string[] TurnsString =
    {
        "<color=#e60033>相手のターン</color>",
        "<color=#0095d9>あなたのターン</color>"
    };
    
    public static readonly int ByteMax = 255;
}