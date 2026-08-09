using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyClaim : MonoBehaviour
{
   private WeekReward data;
   [SerializeField] private Image icon;
   [SerializeField] private GameObject claimedIcon;
   [SerializeField] private TextMeshProUGUI quanty;
   

   public void Init(WeekReward data)
   {
      this.data = Instantiate(data);
      icon.sprite = data.icon;
      quanty.text = data.Quanty.ToString();
   }

   public void SetUpCanClaim() => claimedIcon.SetActive(false);
   public void SetupHadClaim() => claimedIcon.SetActive(true);
   
   
}
