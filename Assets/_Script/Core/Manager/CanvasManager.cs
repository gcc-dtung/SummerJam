using UnityEngine;

public class CanvasManager : Singleton<CanvasManager>
{
    [SerializeField] private Canvas canvasGamePlay;
    [SerializeField] private Canvas canvasMainMenu;
    [SerializeField] private CanvasTransition canvasTransition;

    private bool isChangingCanvas;

    public async void ChangeToMainMenu()
    {
        if (isChangingCanvas)
            return;

        isChangingCanvas = true;

        await canvasTransition.PlayAsync(() =>
        {
            canvasGamePlay.gameObject.SetActive(false);
            canvasMainMenu.gameObject.SetActive(true);
        });

        isChangingCanvas = false;
    }

    public async void ChangeToGameplayCanvas()
    {
        if (isChangingCanvas)
            return;

        isChangingCanvas = true;

        await canvasTransition.PlayAsync(() =>
        {
            canvasMainMenu.gameObject.SetActive(false);
            canvasGamePlay.gameObject.SetActive(true);
        });

        isChangingCanvas = false;
    }
}