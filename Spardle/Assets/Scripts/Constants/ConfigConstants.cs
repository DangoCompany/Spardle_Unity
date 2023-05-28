public static class ConfigConstants
{
    public static readonly int MaxPlayerNum = 2;
    // 追加したら随時更新すべし
    public static readonly int FigureShapeNum = 4;
    // 追加したら随時更新すべし
    public static readonly int CardDataNum = 4;
    public static readonly int TableCount = 3;
    public static readonly int TotalCardsNum = FigureShapeNum * DictionaryConstants.FigureColors.Length;
    public static readonly float QuarterShuffleTime = 0.1f;

    public enum CardEffect
    {
        Exchange,
        Illusion,
        None,
        Substitute
    }
    public enum CustomPropertyKey
    {
        IsMasterClientTurnKey,
        IsInProgressKey
    }
}