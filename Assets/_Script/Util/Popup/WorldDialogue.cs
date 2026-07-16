using TMPro;
using UnityEngine;

public class WorldDialogue : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private TextMeshPro text;

    [SerializeField] private float bubbleWidth = 3f;
    [SerializeField] private Vector2 padding = new(0.25f, 0.18f);

    public void SetText(string message)
    {
        float textWidth = bubbleWidth - padding.x * 2f;

        text.text = message;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        
        Vector2 preferred = text.GetPreferredValues(message, textWidth, 0);
        
        background.size = new Vector2(
            bubbleWidth,
            preferred.y + padding.y * 2f
        );
        
        text.rectTransform.sizeDelta = new Vector2(textWidth, preferred.y);
        text.transform.localPosition = Vector3.zero;
    }
}