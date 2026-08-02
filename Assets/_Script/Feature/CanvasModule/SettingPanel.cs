using System;
using UnityEngine;

public class SettingPanel : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("Assign the full-screen Settings popup root. Leave empty when this component is on that root.")]
    [SerializeField] private Transform settingPanelParent;

    [Tooltip("Assign the content that should scale and fade during intro/outro.")]
    [SerializeField] private Transform contentRoot;

    [Tooltip("Assign the CanvasGroup on Content Root.")]
    [SerializeField] private CanvasGroup contentCanvasGroup;

    [Header("Animation")]
    [Tooltip("Uses the same fade and pop animation as Lose Panel. Assign this popup's background Image inside the animation settings.")]
    [SerializeField] private LosePhaseIntroAnimation introOutroAnimation = new LosePhaseIntroAnimation();

    private bool isOpen;
    private bool isClosing;
    private bool ownsPause;
    private float timeScaleBeforeOpen = 1f;

    public bool IsVisible => isOpen && PanelRoot.gameObject.activeInHierarchy;

    private Transform PanelRoot => settingPanelParent != null ? settingPanelParent : transform;

    private void Awake()
    {
        // Show can be invoked through a UnityEvent while this GameObject is inactive.
        // In that case SetActive(true) invokes Awake in the middle of Show, so keep the
        // opening state instead of immediately hiding the panel again.
        if (!isOpen)
            HideImmediate();
    }

    private void OnDisable()
    {
        introOutroAnimation.Stop();
        RestoreTimeScale();
        isOpen = false;
        isClosing = false;
    }

    public void Show()
    {
        if (isOpen || isClosing)
            return;

        if (!HasAnimationReferences())
        {
            Debug.LogError(
                "SettingPanel needs Content Root, Content Canvas Group, and the popup background Image inside Intro Outro Animation.",
                this
            );
            return;
        }

        PauseGame();

        isOpen = true;
        PanelRoot.gameObject.SetActive(true);
        contentRoot.gameObject.SetActive(true);

        CanvasGroupUtility.SetInteractable(contentCanvasGroup, false);
        introOutroAnimation.PrepareLosePanel();
        introOutroAnimation.Play(contentRoot, contentCanvasGroup, useUnscaledTime: true);
    }

    public void Resume()
    {
        Close(RestoreTimeScale);
    }

    public void Replay()
    {
        Close(() =>
        {
            RestoreTimeScale();

            if (FlowManager.Instance != null)
                FlowManager.Instance.ReplayCurrentLevel();
            else if (GameManager.Instance != null)
                GameManager.Instance.UpdateGameState(GameState.Replay);
        });
    }

    public void BackToMainMenu()
    {
        Close(() =>
        {
            RestoreTimeScale();

            if (GameManager.Instance != null)
                GameManager.Instance.ResumeCurrentLevelOnNextStart();

            if (FlowManager.Instance != null)
                FlowManager.Instance.BackToMainMenu();
            else if (CanvasManager.Instance != null)
                CanvasManager.Instance.ChangeToMainMenu();
        });
    }

    public void ToggleSFX()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.MuteAndUnMuteSFX();
    }

    public void ToggleBGM()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.MuteAndUnMuteBGM();
    }

    public void HideImmediate()
    {
        introOutroAnimation.Stop();
        CanvasGroupUtility.SetInteractable(contentCanvasGroup, false);

        if (contentRoot != null)
            contentRoot.gameObject.SetActive(false);

        if (settingPanelParent != null)
            settingPanelParent.gameObject.SetActive(false);
        else if (gameObject.activeSelf)
            gameObject.SetActive(false);

        RestoreTimeScale();
        isOpen = false;
        isClosing = false;
    }

    private void Close(Action onComplete)
    {
        if (isClosing)
            return;

        if (!IsVisible || !HasAnimationReferences())
        {
            HideImmediate();
            onComplete?.Invoke();
            return;
        }

        isClosing = true;
        CanvasGroupUtility.SetInteractable(contentCanvasGroup, false);

        introOutroAnimation.PlayOutro(
            contentRoot,
            contentCanvasGroup,
            () =>
            {
                HideImmediate();
                onComplete?.Invoke();
            },
            useUnscaledTime: true
        );
    }

    private void PauseGame()
    {
        if (ownsPause)
            return;

        timeScaleBeforeOpen = Time.timeScale;
        Time.timeScale = 0f;
        ownsPause = true;
    }

    private void RestoreTimeScale()
    {
        if (!ownsPause)
            return;

        Time.timeScale = timeScaleBeforeOpen;
        ownsPause = false;
    }

    private bool HasAnimationReferences()
    {
        return contentRoot != null &&
               contentCanvasGroup != null &&
               introOutroAnimation.IsConfigured;
    }
}
