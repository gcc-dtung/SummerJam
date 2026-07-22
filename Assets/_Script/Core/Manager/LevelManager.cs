using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
     [SerializeField] private List<LevelConfig> levelConfigs;
     public event Action<LevelConfig> OnLevelConfigChange;
     private int count = -1;

     [Button("Next Level")]
     public void NextLevelButton()
     {
         if (Application.isPlaying && GameManager.Instance != null)
         {
             GameManager.Instance.StartCoroutine(LoadNextLevelFlow());
         }
         else
         {
             LoadNextLevel();
         }
     }

     private IEnumerator LoadNextLevelFlow()
     {
         GameManager.Instance.UpdateGameState(GameState.SetUp);
         LoadNextLevel();
         yield return null;
         GameManager.Instance.UpdateGameState(GameState.GamePlay);
     }

     public void LoadNextLevel()
     {
         count++;
         if (count < 0 || count >= levelConfigs.Count) count = 0;
         OnLevelConfigChange?.Invoke(levelConfigs[count]);
     }

     public void LoadCurrentLevel()
     {
         if (count < 0 || count >= levelConfigs.Count) count = 0;
         OnLevelConfigChange?.Invoke(levelConfigs[count]);
     }
 
}
