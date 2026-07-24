using System;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private Transform winPanelParent;

    [Header("Phase 1")] 
    [SerializeField] private Transform rootPhase1;
    [SerializeField] private Button rewardButton;
    [SerializeField] private Button adsButton;
    
    [Header("Phase 2")]
    [SerializeField] private Transform rootPhase2;
    [SerializeField] private Button contiueButton;

    private void OnEnable()
    {
        rewardButton.onClick.AddListener(OnPressRewardButton);
        adsButton.onClick.AddListener(OnPressAdsButton);
        contiueButton.onClick.AddListener(OnPressContiueButton);
    }

    private void OnDisable()
    {
        rewardButton.onClick.RemoveListener(OnPressRewardButton);
        adsButton.onClick.RemoveListener(OnPressAdsButton);
        contiueButton.onClick.RemoveListener(OnPressContiueButton);
    }

    private void Start()
    {
        winPanelParent.gameObject.SetActive(false);
    }

    public void OnWin()
    {
        winPanelParent.gameObject.SetActive(true);
        rootPhase1.gameObject.SetActive(true);
        rootPhase2.gameObject.SetActive(false);
    }

    private void OnPressRewardButton()
    {
        // TODO : Anim Cong Vang
        NextPhase();
    }

    private void OnPressAdsButton()
    {
        // TODO : Ads + Anim
        NextPhase();
    }

    private void OnPressContiueButton()
    {
        CanvasManager.Instance.ChangeToMainMenu();
        winPanelParent.gameObject.SetActive(false);
    }

    private void NextPhase()
    {
        rootPhase1.gameObject.SetActive(false);
        rootPhase2.gameObject.SetActive(true);
    }
}
