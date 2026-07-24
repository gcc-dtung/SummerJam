using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class LosePanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private Transform losePanelParent;

    [Header("Animations")]
    [SerializeField] private OutOfTurnNoticeAnimation outOfTurnNoticeAnimation = new OutOfTurnNoticeAnimation();
    [SerializeField] private LosePhaseIntroAnimation losePhaseIntroAnimation = new LosePhaseIntroAnimation();
    [SerializeField] private PhaseTransitionAnimation phaseTransitionAnimation = new PhaseTransitionAnimation();

    [Header("Phase Canvas Groups")]
    [SerializeField] private CanvasGroup phase1CanvasGroup;
    [SerializeField] private CanvasGroup phase2CanvasGroup;

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
        outOfTurnNoticeAnimation.Stop();
        losePhaseIntroAnimation.Stop();
        phaseTransitionAnimation.Stop();

        coinButton.onClick.RemoveAllListeners();
        adsButton.onClick.RemoveAllListeners();
        chooseLoseButton.onClick.RemoveAllListeners();
        stayButton.onClick.RemoveAllListeners();
        loseButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        losePanelParent.gameObject.SetActive(false);
        outOfTurnNoticeAnimation.Hide();

        rootPhase1.gameObject.SetActive(false);
        rootPhase2.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);
    }

    [Button("TestLosePanel")]
    public void OnLose()
    {
        losePanelParent.gameObject.SetActive(true);

        rootPhase1.gameObject.SetActive(false);
        rootPhase2.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        losePhaseIntroAnimation.PrepareLosePanel();
        outOfTurnNoticeAnimation.Play(PlayPhase1Intro);
    }

    private void PlayPhase1Intro()
    {
        losePhaseIntroAnimation.Play(
            rootPhase1,
            rootPhase2,
            phase1CanvasGroup,
            phase2CanvasGroup
        );
    }

    private void NextPhase(bool isContinue = true)
    {
        if (isContinue)
        {
            PlayPhaseTransition(
                fromRoot: rootPhase1,
                fromCanvasGroup: phase1CanvasGroup,
                toRoot: rootPhase2,
                toCanvasGroup: phase2CanvasGroup
            );
        }
        else
        {
            PlayPhaseTransition(
                fromRoot: rootPhase2,
                fromCanvasGroup: phase2CanvasGroup,
                toRoot: rootPhase1,
                toCanvasGroup: phase1CanvasGroup
            );
        }
    }

    private void PlayPhaseTransition(
        Transform fromRoot,
        CanvasGroup fromCanvasGroup,
        Transform toRoot,
        CanvasGroup toCanvasGroup)
    {
        phaseTransitionAnimation.Play(
            fromRoot,
            fromCanvasGroup,
            toRoot,
            toCanvasGroup
        );
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
}
