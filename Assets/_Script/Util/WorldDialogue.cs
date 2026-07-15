using TMPro;
using UnityEngine;

public class WorldDialogue : MonoBehaviour
{
    [SerializeField] SpriteRenderer background;
    [SerializeField] TextMeshPro text;

    [SerializeField] Vector2 padding = new(0.35f, 0.2f);

    public void SetText(string value)
    {
        text.text = value;

        // lấy kích thước thực của chữ trong world unit
        text.ForceMeshUpdate();
        Vector2 textSize = text.GetRenderedValues(false);

        background.size = textSize + padding;

        // đảm bảo chữ nằm giữa khung
        text.transform.localPosition = Vector3.zero;
    }
}