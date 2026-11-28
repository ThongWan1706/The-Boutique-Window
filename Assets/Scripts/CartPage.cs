using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Put this on an object that stays switched on (e.g. "UI"), and drag the cart panel into "panel".
// Hook CartButton's On Click () to CartPage.Open, and the back arrow to CartPage.Close.
public class CartPage : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;
    public TMP_Text titleText;

    [Header("List")]
    public Transform listContent;
    public CartItemRow rowPrefab;
    public GameObject emptyMessage;

    [Header("Bottom bar")]
    public Toggle selectAllToggle;
    public Button checkoutButton;
    public TMP_Text checkoutButtonText;

    [Header("Checkout")]
    public PopupMessage popup;
    public bool deleteImagesOnRemove = true;
    [TextArea]
    public string checkoutMessage = "Checked out {0} item(s)!";
    [TextArea]
    public string promoReminder = "Your promo codes: {0}\nUse them when you buy items at the Swee Lee shop!";
    public UnityEvent<List<CartItem>> onCheckout;

    readonly List<CartItemRow> rows = new List<CartItemRow>();

    void Start()
    {
        selectAllToggle.onValueChanged.AddListener(SelectAll);
        checkoutButton.onClick.AddListener(Checkout);

        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnCartChanged += RebuildIfOpen;

        panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnCartChanged -= RebuildIfOpen;
    }

    public void Open()
    {
        panel.SetActive(true);
        Rebuild();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    void RebuildIfOpen()
    {
        if (panel.activeSelf)
            Rebuild();
    }

    void Rebuild()
    {
        foreach (var row in rows)
            Destroy(row.gameObject);
        rows.Clear();

        var cart = PlayerDataManager.Instance.Data.cart;

        // Newest photo at the top
        for (int i = cart.Count - 1; i >= 0; i--)
        {
            CartItemRow row = Instantiate(rowPrefab, listContent);
            row.Setup(cart[i], _ => UpdateSummary(), DeleteOne);
            rows.Add(row);
        }

        if (emptyMessage != null)
            emptyMessage.SetActive(cart.Count == 0);

        UpdateSummary();
    }

    void UpdateSummary()
    {
        int selected = rows.FindAll(r => r.IsSelected).Count;

        if (titleText != null)
            titleText.text = $"Shopping Cart ({rows.Count})";

        checkoutButtonText.text = $"Check Out ({selected})";
        checkoutButton.interactable = selected > 0;

        selectAllToggle.SetIsOnWithoutNotify(rows.Count > 0 && selected == rows.Count);
    }

    void SelectAll(bool on)
    {
        foreach (var row in rows)
            row.SetSelected(on);
        UpdateSummary();
    }

    void DeleteOne(CartItemRow row)
    {
        PlayerDataManager.Instance.RemoveCartItems(new List<CartItem> { row.Item }, deleteImagesOnRemove);
        // The cart-changed event rebuilds the list
    }

    void Checkout()
    {
        var selectedItems = new List<CartItem>();
        foreach (var row in rows)
            if (row.IsSelected)
                selectedItems.Add(row.Item);

        if (selectedItems.Count == 0)
            return;

        onCheckout?.Invoke(selectedItems);

        PlayerDataManager.Instance.RemoveCartItems(selectedItems, deleteImagesOnRemove);

        if (popup != null)
        {
            string message = string.Format(checkoutMessage, selectedItems.Count);

            var codes = PlayerDataManager.Instance.Data.promoCodes;
            if (codes.Count > 0)
                message += "\n\n" + string.Format(promoReminder, string.Join(", ", codes));

            popup.Show(message);
        }
    }
}