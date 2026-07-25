using UnityEngine;

public static class CanvasGroupUtility
{
    public static void SetInteractable(CanvasGroup canvasGroup, bool value)
    {
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }
}
