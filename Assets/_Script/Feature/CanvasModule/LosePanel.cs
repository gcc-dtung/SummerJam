using UnityEngine;
using UnityEngine.UI;

public class LosePanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private Transform losePanelParent;

    [Header("Phase 1")] 
    [SerializeField] private Transform rootPhase1;
    [SerializeField] private Button coinButton;
    [SerializeField] private Button adsButton;
    [SerializeField] private Button chooseLoseButton;
    
    [Header("Phase 2")]
    [SerializeField] private Transform rootPhase2;
    [SerializeField] private Button stayButton;
    [SerializeField] private Button loseButton;

    private void OnEnable()
    {
        coinButton.onClick.AddListener(OnPressCoinButton);
        adsButton.onClick.AddListener(OnPressAdsButton);
        chooseLoseButton.onClick.AddListener(OnPressChooseLoseButton);
        stayButton.onClick.AddListener(OnpressStayButton);
        loseButton.onClick.AddListener(OnPressLoseButton);
    }

    private void OnDisable()
    {
        coinButton.onClick.RemoveAllListeners();
        adsButton.onClick.RemoveAllListeners();
        chooseLoseButton.onClick.RemoveAllListeners();
        stayButton.onClick.RemoveAllListeners();
        loseButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        loseButton.gameObject.SetActive(false);
    }

    public void OnLose()
    {
        //TODO : Anim
        losePanelParent.gameObject.SetActive(true);
        rootPhase1.gameObject.SetActive(true);
        rootPhase2.gameObject.SetActive(false);
    }

    private void OnPressCoinButton()
    {
        // TODO: Anim Qlai Man choi + Them luot
    }

    private void OnPressAdsButton()
    {
        // TODO: Qc + Anim Qlai Man choi + Them luot
    }

    private void OnPressChooseLoseButton()
    {
        NextPhase();
    }

    private void OnpressStayButton()
    {
        NextPhase(false);
    }

    private void OnPressLoseButton()
    {
        CanvasManager.Instance.ChangeToMainMenu();
        losePanelParent.gameObject.SetActive(false);
    }

    private void NextPhase(bool isContionue = true)
    {
        if(isContionue)
        {
            rootPhase1.gameObject.SetActive(false);
            //TODO : Anim
            rootPhase2.gameObject.SetActive(true);
        }
        else
        {
            rootPhase2.gameObject.SetActive(false);
            //TODO : Anim
            rootPhase1.gameObject.SetActive(true);
        }
    }
}
