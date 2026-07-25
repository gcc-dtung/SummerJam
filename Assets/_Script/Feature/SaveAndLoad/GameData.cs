using System;
using System.Collections.Generic;
[Serializable]
public class GameData
{
    public int currentLevelIndex;
    public Dictionary<Booster, int> boosterCounts;
    public int currentGold;
    public int currentGem;
    public GameData()
    {
        currentLevelIndex = 0;
        boosterCounts = new Dictionary<Booster, int>();
        foreach (Booster boost in Enum.GetValues(typeof(Booster)))
            boosterCounts.Add(boost, 1);
        currentGold = 0;
        currentGem = 0;
    }
}