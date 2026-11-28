using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartPage : MonoBehaviour
{
    [Header("Cart page")]
    public GameObject cartPanel;
    public TMP_Text titleText;
    public GameObject emptyMessage;

    [Header("List of photos")]
    public Transform content;
    public CartItemRows rowPrefab; 

    [Header("Check out")]
    public Button checkoutButton;
    public TMP_Text checkoutText;
    public PopupMessage popup;

    // All the rows that are on screen
    List<CartItemRows> rows = new List<CartItemRows>();


    void Start()
    {
        // When the Check Out button is pressed, run CheckOut()
        checkoutButton.onClick.AddListener(CheckOut);

        // When the cart changes (photo added or removed), refresh the list
        playerDataManager.Instance.OnCartChanged += RefreshIfOpen;

        // The cart is hidden when the game starts
        cartPanel.SetActive(false);
    }


    // Open up the cart panel
    public void Open()
    {
        cartPanel.SetActive(true);
        ShowRows();
    }


    // Close the cart panel
    public void Close()
    {
        cartPanel.SetActive(false);
    }


    // Refresh the list, but only if the cart page is open
    void RefreshIfOpen()
    {
        if (cartPanel.activeSelf)
            ShowRows();
    }


    // Removes the old rows and makes a new row for each photo in the cart
    void ShowRows()
    {
        //Remove the old rows
        foreach (CartItemRows oldRow in rows)
            Destroy(oldRow.gameObject);
        rows.Clear();

        //Make a new row for each photo (newest photo at the top)
        List<CartItem> cart = playerDataManager.Instance.Data.cart;

        for (int i = cart.Count - 1; i >= 0; i--)
        {
            CartItemRows newRow = Instantiate(rowPrefab, content);
            newRow.Setup(cart[i], OnRowTicked, DeleteRow);
            rows.Add(newRow);
        }

        //Show "Your cart is empty!" only when there are no photos
        emptyMessage.SetActive(cart.Count == 0);

        //Update the title and the Check Out button
        UpdateTexts();
    }

    void UpdateTexts()
    {
        int tickedCount = CountTickedRows();

        titleText.text = "Shopping Cart (" + rows.Count + ")";
        checkoutText.text = "Check Out (" + tickedCount + ")";

        // The Check Out button only works when at least one photo is ticked
        checkoutButton.interactable = tickedCount > 0;
    }


    // Counts how many rows the player has ticked
    int CountTickedRows()
    {
        int count = 0;
        foreach (CartItemRows row in rows)
        {
            if (row.IsSelected)
                count++;
        }
        return count;
    }


    // Update the () in the checkout text
    void OnRowTicked(CartItemRows row)
    {
        UpdateTexts();
    }


    // Delete the row in the cart
    void DeleteRow(CartItemRows row)
    {
        List<CartItem> itemsToRemove = new List<CartItem> { row.Item };
        playerDataManager.Instance.RemoveCartItems(itemsToRemove, true);
        // The list refreshes by itself because the cart changed
    }


    // When the player clicked the Checkout button
    void CheckOut()
    {
        //Collect all the ticked photos
        List<CartItem> tickedItems = new List<CartItem>();
        foreach (CartItemRows row in rows)
        {
            if (row.IsSelected)
                tickedItems.Add(row.Item);
        }

        if (tickedItems.Count == 0)
            return;

        //Remove them from the cart => saves to Firebase
        playerDataManager.Instance.RemoveCartItems(tickedItems, true);

        //Show a message + remind the player of their promo codes
        string message = "Checked out " + tickedItems.Count + " item(s)!";

        List<string> codes = playerDataManager.Instance.Data.promoCodes;
        if (codes.Count > 0)
        {
            message += "\n\nYour promo codes: " + string.Join(", ", codes) +
                       "\nUse them when you buy items at the Swee Lee shop!";
        }

        popup.Show(message);
    }
}
