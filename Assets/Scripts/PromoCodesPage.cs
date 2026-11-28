using TMPro;
using UnityEngine;

public class PromoCodesPage : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text codesText;
    [TextArea]
    public string emptyText = "No promo codes yet.\nTake photos of your setups to collect some!";
    [TextArea]
    public string footer = "\n\nUse these when you buy items at the Swee Lee shop.";

    void Start()
    {
        if (playerDataManager.Instance != null)
            playerDataManager.Instance.OnPromoCodesChanged += Refresh;
        panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (playerDataManager.Instance != null)
            playerDataManager.Instance.OnPromoCodesChanged -= Refresh;
    }

    public void Open()
    {
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    void Refresh()
    {
        var codes = playerDataManager.Instance.Data.promoCodes;
        codesText.text = codes.Count == 0
            ? emptyText
            : "• " + string.Join("\n• ", codes) + footer;
    }
}