using System;
using System.Collections.Generic;

[Serializable]
public class CartItem
{
    public string id;          // Unique id for this photo, e.g. "3f9c1a..."
    public string sceneName;   // Name shown in the cart, e.g. "My Setup 1"
    public string imagePath;   // Where the screenshot PNG is saved on the device
    public long timestamp;     // When the photo was taken (Unix time in milliseconds)

    public CartItem(string sceneName, string imagePath, long timestamp)
    {
        this.id = Guid.NewGuid().ToString("N");
        this.sceneName = sceneName;
        this.imagePath = imagePath;
        this.timestamp = timestamp;
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

    // Data loaded from JSON or a database can come back with empty (null) lists. 
    public void EnsureLists()
    {
        if (promoCodes == null) promoCodes = new List<string>();
        if (cart == null) cart = new List<CartItem>();

        // Older cart items saved before ids existed get one now
        foreach (var item in cart)
            if (string.IsNullOrEmpty(item.id))
                item.id = Guid.NewGuid().ToString("N");
    }
}