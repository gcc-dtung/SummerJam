using UnityEngine;

public class Test2 : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] string content;
    [SerializeField] TooltipPopup tooltipPopup;


    [ContextMenu("TestOn")]
    public void Test()
    {
        tooltipPopup.Show(name, "Toi thich an rau\nToi thich an thit bo\nToi muon ngoi canh Phuc");
    }

    [ContextMenu("TestOff")]
    public void TestOff()
    {
        tooltipPopup.Hide();
    }
    
    
}
