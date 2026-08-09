using System;
using System.Collections.Generic;
[Serializable]
public class GameData
{
    private const int StartingLevelIndex = 0;
    private const int StartingBoosterCount = 5;
    private const int StartingGold = 1000;
    private const int StartingGem = 0;
    private const int StartingDailyRewardIndex = 0;

    public int currentLevelIndex;
    public int tutorialVersionCompleted;
    public Dictionary<Booster, int> boosterCounts;
    public Dictionary<int, int> shopPurchasedCounts;
    public int currentGold;
    public int currentGem;
    public int CurrentWeekReward;
    public bool HadClaimWeekReward;
    public DateTime lastTime;
    public GameData()
    {
        currentLevelIndex = StartingLevelIndex;
        tutorialVersionCompleted = 0;
        boosterCounts = new Dictionary<Booster, int>();
        shopPurchasedCounts = new Dictionary<int, int>();
        foreach (Booster boost in Enum.GetValues(typeof(Booster)))
            boosterCounts.Add(boost, StartingBoosterCount);
        currentGold = StartingGold;
        currentGem = StartingGem;
        CurrentWeekReward = StartingDailyRewardIndex;
        HadClaimWeekReward = false;
        lastTime = DateTime.Now;
    }
}
