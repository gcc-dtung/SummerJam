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

    [Header("Phase Canvas Group")]
    [SerializeField] private CanvasGroup phase1CanvasGroup;

    [Header("Lose Options")]
    [SerializeField] private Transform rootPhase1;
    [SerializeField] private Button coinButton;
    [SerializeField] private Button adsButton;
    [SerializeField] private Button chooseLoseButton;

    private bool isClosing;

    private void OnEnable()
    {
        coinButton.onClick.AddListener(OnPressCoinButton);
        adsButton.onClick.AddListener(OnPressAdsButton);
        chooseLoseButton.onClick.AddListener(OnPressChooseLoseButton);
    }

    private void OnDisable()
    {
        outOfTurnNoticeAnimation.Stop();
        losePhaseIntroAnimation.Stop();

        coinButton.onClick.RemoveAllListeners();
        adsButton.onClick.RemoveAllListeners();
        chooseLoseButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        losePanelParent.gameObject.SetActive(false);
        outOfTurnNoticeAnimation.Hide();

        rootPhase1.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
    }

    [Button("TestLosePanel")]
    public void OnLose()
    {
        isClosing = false;
        losePanelParent.gameObject.SetActive(true);

        rootPhase1.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);

        losePhaseIntroAnimation.PrepareLosePanel();
        outOfTurnNoticeAnimation.Play(PlayPhase1Intro);
    }

    private void PlayPhase1Intro()
    {
        losePhaseIntroAnimation.Play(
            rootPhase1,
            phase1CanvasGroup
        );
    }

    private void OnPressCoinButton()
    {
        // TODO: Anim Qlai Man choi + Them luot
        if(!EconomyManager.Instance.SpendGold(300)) return;
        MoveManager.Instance.AddMoreMove(5);
        GameManager.Instance.UpdateGameState(GameState.GamePlay);
       // CanvasManager.Instance.ChangeToGameplayCanvas();
        losePanelParent.gameObject.SetActive(false);
    }

    private void OnPressAdsButton()
    {
        // TODO: Qc + Anim Qlai Man choi + Them luot
        MoveManager.Instance.AddMoreMove(5);
        GameManager.Instance.UpdateGameState(GameState.GamePlay);
        // CanvasManager.Instance.ChangeToGameplayCanvas();
        losePanelParent.gameObject.SetActive(false);
    }

    private void OnPressChooseLoseButton()
    {
        if (isClosing)
            return;

        isClosing = true;
        losePhaseIntroAnimation.PlayOutro(rootPhase1, phase1CanvasGroup, () =>
        {
            CanvasManager.Instance.ChangeToMainMenu();
            losePanelParent.gameObject.SetActive(false);
            isClosing = false;
        });
    }
}
