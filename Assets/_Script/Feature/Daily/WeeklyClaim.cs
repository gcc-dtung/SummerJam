using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyClaim : MonoBehaviour
{
   [SerializeField] private WeekReward data;
   [SerializeField] private Image icon;
   [SerializeField] private GameObject claimedIcon;
   [SerializeField] private TextMeshProUGUI quanty;
   [SerializeField] private Button claimButton;

   private void Start()
   {
      icon.sprite = data.icon;
      quanty.text = data.Quanty.ToString();
   }

   private void OnEnable()
   {
      claimButton.onClick.AddListener(Claim);
   }

   private void OnDisable()
   {
      claimButton.onClick.RemoveAllListeners();
   }

   public void Init(WeekReward data)
   {
      this.data = data;
      icon.sprite = data.icon;
      quanty.text = data.Quanty.ToString();
   }

   public void SetUpCanClaim()
   {
      claimButton.interactable = true;
      claimedIcon.SetActive(false);
   }

   public void SetUpCannotClaim()
   {
      claimButton.interactable = false;
   }

   public void SetupHadClaim()
   {
      claimButton.interactable = false;
      claimedIcon.SetActive(true);
   }


   private void Claim()
   {
      GrantReward(data.RewardType,data.Quanty);
      claimButton.interactable = false;
      claimedIcon.SetActive(true);
   }
   
   private void GrantReward(RewardType type, int quantity)
   {
      switch (type)
      {
         case RewardType.Gold:
            EconomyManager.Instance.GetGold(quantity);
            break;
         case RewardType.Gem:
            EconomyManager.Instance.GetGem(quantity);
            break;
         case RewardType.MoveBooster:
            BoosterManager.Instance.AddMoreBooster(Booster.Move,quantity);
            break; 
         case RewardType.RemoveBooster:
            BoosterManager.Instance.AddMoreBooster(Booster.Remove,quantity);
            break; 
         case RewardType.UndoBooster:
            BoosterManager.Instance.AddMoreBooster(Booster.Undo,quantity);
            break;
      }
   }
}
