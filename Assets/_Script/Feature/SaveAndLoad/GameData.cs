using System;
using System.Collections.Generic;
[Serializable]
public class GameData
{
    public int currentLevelIndex;
    public Dictionary<Booster, int> boosterCounts;
    public Dictionary<int, int> shopPurchasedCounts;
    public int currentGold;
    public int currentGem;
    public DateTime lastTime;
    public GameData()
    {
        currentLevelIndex = 0;
        boosterCounts = new Dictionary<Booster, int>();
        shopPurchasedCounts = new Dictionary<int, int>();
        foreach (Booster boost in Enum.GetValues(typeof(Booster)))
            boosterCounts.Add(boost, 1);
        currentGold = 0;
        currentGem = 0;
        lastTime = DateTime.Now;
    }
}