using UnityEngine;

public class MainMenuSettingPanel : MonoBehaviour
{
    [Header("Popup")]
    [Tooltip("Root GameObject of the audio settings popup.")]
    [SerializeField] private GameObject popupRoot;

    [Header("Audio State Icons")]
    [Tooltip("Image shown only while SFX is muted.")]
    [SerializeField] private GameObject sfxOffImage;

    [Tooltip("Image shown only while BGM is muted.")]
    [SerializeField] private GameObject bgmOffImage;

    private void OnEnable()
    {
        Hide();
        RefreshAudioStateIcons();
    }

    private void Start()
    {
        // Run after every Awake so the SoundManager is ready on the first frame.
        RefreshAudioStateIcons();
    }

    public void Show()
    {
        if (popupRoot == null)
            return;

        popupRoot.SetActive(true);
        RefreshAudioStateIcons();
    }

    public void Hide()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    public void ToggleSFX()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.MuteAndUnMuteSFX();

        RefreshAudioStateIcons();
    }

    public void ToggleBGM()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.MuteAndUnMuteBGM();

        RefreshAudioStateIcons();
    }

    public void RefreshAudioStateIcons()
    {
        SoundManager soundManager = SoundManager.Instance;

        if (sfxOffImage != null)
            sfxOffImage.SetActive(soundManager != null && soundManager.IsSFXMuted);

        if (bgmOffImage != null)
            bgmOffImage.SetActive(soundManager != null && soundManager.IsBGMMuted);
    }
}
