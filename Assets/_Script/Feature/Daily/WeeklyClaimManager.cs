using System;
using UnityEngine;

public class WeeklyClaimManager : MonoBehaviour
{
   [SerializeField] private WeeklyClaim[] weeklyClaimHolders;
   [SerializeField] private WeekReward[] _weekRewards;

   private void Start()
   {
      for (int i = 0; i < 7; i++)
      {
         weeklyClaimHolders[i].Init(_weekRewards[i]);
      }
   }
}
