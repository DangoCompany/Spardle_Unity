public static class ConfigConstants
{
    // 追加したら随時更新すべし
    public static readonly int FigureShapeNum = 6;
    // 追加したら随時更新すべし
    public static readonly int CardDataNum = 5;
    public static readonly int TableCount = 3;
    public static readonly int TotalCardsNum = FigureShapeNum * DictionaryConstants.FigureColors.Length;
    public static readonly float QuarterShuffleTime = 0.1f;

    public enum CardEffect
    {
        Exchange,
        Illusion,
        None,
        Remove,
        Substitute
    }
    public enum CustomPropertyKey
    {
        IsMasterClientTurnKey,
        IsMasterCardPlaying,
        IsNonMasterCardPlaying,
        IsSenderActionInProgress,
        IsReceiverActionInProgress
    }
}