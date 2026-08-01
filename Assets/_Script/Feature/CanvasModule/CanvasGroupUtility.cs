using UnityEngine;

public static class CanvasGroupUtility
{
    public static void SetInteractable(CanvasGroup canvasGroup, bool value)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }
}
