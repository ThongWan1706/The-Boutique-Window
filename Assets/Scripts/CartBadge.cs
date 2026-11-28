using TMPro;
using UnityEngine;

// Put this on the CartButton. Shows the little number at the top right of the cart icon.
public class CartBadge : MonoBehaviour
{
    public GameObject badge;      // The small red circle (child of CartButton)
    public TMP_Text countText;    // The number inside the circle

    void Start()
    {
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnCartChanged += Refresh;
        Refresh();
    }

    void OnEnable()
    {
        Refresh();
    }

    void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnCartChanged -= Refresh;
    }

    void Refresh()
    {
        var manager = PlayerDataManager.Instance;
        int count = (manager != null && manager.Data != null) ? manager.Data.cart.Count : 0;

        if (badge != null)
            badge.SetActive(count > 0);

        if (countText != null)
            countText.text = count > 99 ? "99+" : count.ToString();
    }
}