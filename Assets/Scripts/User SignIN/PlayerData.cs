using System;
using System.Collections.Generic;

[Serializable]
public class CartItem
{
    public string id;          // Id for the photo
    public string sceneName;   // Name shown in the cart
    public string imagePath;   // Screenshot PNG is saved on the device
    public long timestamp;     // When the photo was taken
    public List<string> items; // What was placed in the scene, e.g. "Acoustic Guitar x2", "Drums"

    public CartItem(string sceneName, string imagePath, long timestamp, List<string> items)
    {
        this.id = Guid.NewGuid().ToString("N");
        this.sceneName = sceneName;
        this.imagePath = imagePath;
        this.timestamp = timestamp;
        this.items = items;
    }
}

[Serializable]
public class PlayerData
{
    public string id;
    public string email;
    public string password;
    public string playername;
    public long birthday;
    public string avatarId;
    public List<string> promoCodes;   // Changed from one string to a list, so the player can collect several codes
    public List<CartItem> cart;

    public PlayerData(string id, string email, string password, string playername, long birthday, string avatarId, List<string> promoCodes, List<CartItem> cart)
    {
        this.id = id;
        this.email = email;
        this.password = password;
        this.playername = playername;
        this.birthday = birthday;
        this.avatarId = avatarId;
        this.promoCodes = promoCodes;
        this.cart = cart;
        EnsureLists();
    }

    // Data loaded from JSON or a database can come back with empty (null) lists. This fixes that.
    public void EnsureLists()
    {
        if (promoCodes == null) promoCodes = new List<string>();
        if (cart == null) cart = new List<CartItem>();

        // Older cart items saved before ids / item lists existed get them now
        foreach (var item in cart)
        {
            if (string.IsNullOrEmpty(item.id))
                item.id = Guid.NewGuid().ToString("N");
            if (item.items == null)
                item.items = new List<string>();
        }
    }
}