using System;
using System.Collections.Generic;
[Serializable]
public class GameData
{
    public int currentLevelIndex;
    public Dictionary<Booster, int> boosterCounts;
    public GameData()
    {
        currentLevelIndex = 0;
        boosterCounts = new Dictionary<Booster, int>();
        foreach (Booster boost in Enum.GetValues(typeof(Booster)))
            boosterCounts.Add(boost, 1);
    }
}