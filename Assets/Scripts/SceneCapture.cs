using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SceneCapture : MonoBehaviour
{
    public Canvas uiCanvas;
    public PopupMessage popup; 

    // 1st photo gets the 1st code, 2nd photo the 2nd code, 3rd photo the 3rd code
    public List<string> promoCodes = new List<string> { "SWEELEE", "SWEELEE2", "SWEELEE3" };

    public void TakePicture()
    {
        StartCoroutine(TakePictureRoutine());
    }

    IEnumerator TakePictureRoutine()
    {
        //Hide the UI and wait for the screen to be drawn without it
        uiCanvas.enabled = false;
        yield return new WaitForEndOfFrame();

        //Take the screenshot, then show the UI again
        Texture2D picture = ScreenCapture.CaptureScreenshotAsTexture();
        uiCanvas.enabled = true;

        //Save the screenshot as a PNG file on the phone
        string fileName = "capture_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png";
        string imagePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(imagePath, picture.EncodeToPNG());
        Destroy(picture);

        // Make a list of the items the player placed in the scene
        List<string> placedItems = GetPlacedItems();

        // Add the photo to the cart (saved to Firebase, and updates the cart number)
        PlayerData player = playerDataManager.Instance.Data;
        string itemName = "My Setup " + (player.cart.Count + 1);
        long timeNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        playerDataManager.Instance.AddCartItem(new CartItem(itemName, imagePath, timeNow, placedItems));

        // Give a promo code for the first 3 photos
        string message = "Photo added to your cart!";

        int codesAlreadyGiven = player.promoCodes.Count;
        if (codesAlreadyGiven < promoCodes.Count)
        {
            string code = promoCodes[codesAlreadyGiven];
            playerDataManager.Instance.AddPromoCode(code);
            message += "\n\nYou got a promo code: " + code +
                       "\nUse it when you buy items at the Swee Lee shop!";
        }

        // Show the popup
        popup.Show(message);
    }

    // Finds every item placed in the scene and counts them
    // Example result: "Acoustic Guitar x2", "Drums"
    List<string> GetPlacedItems()
    {
        // Count how many of each item there are
        Dictionary<string, int> counts = new Dictionary<string, int>();

        foreach (MoveSound item in FindObjectsByType<MoveSound>(FindObjectsSortMode.None))
        {
            // Unity adds "(Clone)" to spawned objects, so remove it
            string name = item.gameObject.name.Replace("(Clone)", "").Trim();

            if (counts.ContainsKey(name))
                counts[name] = counts[name] + 1;
            else
                counts[name] = 1;
        }

        // Turn the counts into text, e.g. "Drums" or "Acoustic Guitar x2"
        List<string> result = new List<string>();
        foreach (var pair in counts)
        {
            if (pair.Value > 1)
                result.Add(pair.Key + " x" + pair.Value);
            else
                result.Add(pair.Key);
        }

        return result;
    }
}