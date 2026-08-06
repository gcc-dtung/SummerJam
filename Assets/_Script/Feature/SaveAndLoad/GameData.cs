using System;
using System.Collections.Generic;
[Serializable]
public class GameData
{
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
        currentLevelIndex = 0;
        tutorialVersionCompleted = 0;
        boosterCounts = new Dictionary<Booster, int>();
        shopPurchasedCounts = new Dictionary<int, int>();
        foreach (Booster boost in Enum.GetValues(typeof(Booster)))
            boosterCounts.Add(boost, 1);
        currentGold = 0;
        currentGem = 0;
        CurrentWeekReward = 0;
        HadClaimWeekReward = false;
        lastTime = DateTime.Now;
    }
}
