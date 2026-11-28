using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class playerDataManager : MonoBehaviour
{
    public static playerDataManager Instance { get; private set; }

    public PlayerData Data { get; private set; }
    public bool IsLoaded { get; private set; }

    public event Action OnCartChanged;
    public event Action OnPromoCodesChanged;

    // The placeholder item ProfileSetupManager used to add to every new account
    const string OldPlaceholderImage = "Screenshots/default.png";

    string userId;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Start with an empty player so the UI never sees "null" while Firebase is loading
        Data = new PlayerData("", "", "", "", 0, "", new List<string>(), new List<CartItem>());

        LoadFromFirebase();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void LoadFromFirebase()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("PlayerDataManager: no one is logged in, so the cart won't be saved.");
            return;
        }

        userId = user.UserId;

        FirebaseDatabase.DefaultInstance.GetReference("players").Child(userId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("PlayerDataManager: failed to load player data. " + task.Exception);
                    return;
                }

                string json = task.Result.GetRawJsonValue();
                if (!string.IsNullOrEmpty(json))
                {
                    var loaded = JsonUtility.FromJson<PlayerData>(json);
                    if (loaded != null)
                        Data = loaded;
                }

                Data.EnsureLists();

                // Clean up the fake starting item older accounts were created with
                int removed = Data.cart.RemoveAll(item => item.imagePath == OldPlaceholderImage);

                IsLoaded = true;
                if (removed > 0) Save();

                OnCartChanged?.Invoke();
                OnPromoCodesChanged?.Invoke();
            });
    }

    // For the cart section
    public void AddCartItem(CartItem item)
    {
        Data.cart.Add(item);
        Save();
        OnCartChanged?.Invoke();
    }

    public void RemoveCartItems(List<CartItem> items, bool deleteImageFiles)
    {
        foreach (var item in items)
        {
            Data.cart.Remove(item);

            if (deleteImageFiles && !string.IsNullOrEmpty(item.imagePath) && System.IO.File.Exists(item.imagePath))
                System.IO.File.Delete(item.imagePath);
        }

        Save();
        OnCartChanged?.Invoke();
    }

    // Promo codes for the players
    public bool HasPromoCode(string code)
    {
        return Data.promoCodes.Contains(code);
    }

    // Returns false if the player already has this code
    public bool AddPromoCode(string code)
    {
        if (string.IsNullOrEmpty(code) || HasPromoCode(code))
            return false;

        Data.promoCodes.Add(code);
        Save();
        OnPromoCodesChanged?.Invoke();
        return true;
    }


    // Writes the whole player back to Firebase under players/<userId>
    public void Save()
    {
        if (!IsLoaded || string.IsNullOrEmpty(userId))
            return; 

        string json = JsonUtility.ToJson(Data);

        FirebaseDatabase.DefaultInstance.GetReference("players").Child(userId)
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("PlayerDataManager: failed to save. " + task.Exception);
            });
    }
}