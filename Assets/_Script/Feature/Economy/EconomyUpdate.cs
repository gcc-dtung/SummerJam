using System;
using TMPro;
using UnityEngine;

public class EconomyUpdate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] cointText;
    [SerializeField] private TextMeshProUGUI[] gemText;

    private void OnEnable()
    {
        EconomyManager.Instance.OnGoldChange += UpdateCoin;
        EconomyManager.Instance.OnGemChange += UpdateGem;
    }

    private void OnDisable()
    {
        if(EconomyManager.Instance == null) return;
        EconomyManager.Instance.OnGoldChange -= UpdateCoin;
        EconomyManager.Instance.OnGemChange -= UpdateGem;
    }

    private void UpdateCoin(int coin)
    {
        for(int i = 0; i < cointText.Length; i++)
        {
            cointText[i].text = coin.ToString();
        }
    }

    private void UpdateGem(int gem)
    {
        for (int i = 0; i < gemText.Length; i++)
        {
            gemText[i].text = gem.ToString();
        }
    }

}
