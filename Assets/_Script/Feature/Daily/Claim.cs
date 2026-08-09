using UnityEngine;
using UnityEngine.UI;

public class Claim : MonoBehaviour
{
    [SerializeField] private Sprite baseClaimButton;
    [SerializeField] private Sprite ClaimedButton;
    [SerializeField] private Image claimIcon;
    [SerializeField] private Button claimButton;

    public void SetUpHadClaim()
    {
        claimButton.interactable = false;
        claimIcon.sprite = ClaimedButton;
    }

    public void SetUpHadnotClaim()
    {
        claimButton.interactable = true;
        claimIcon.sprite = baseClaimButton;
    }


    public void ClaimPressed()
    {
        SetUpHadClaim();
       if (WeeklyClaimManager.Instance != null)
       {
          WeeklyClaimManager.Instance.ClaimCurrentDay();
       }
    }
    
    
}
