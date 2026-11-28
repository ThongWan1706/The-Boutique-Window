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

    //Works when the player click onto the camera icon
    IEnumerator TakePictureRoutine()
    {
        //Hide the UI and wait for the screen to be drawn without it
        uiCanvas.enabled = false;
        yield return new WaitForEndOfFrame();

        //Take the screenshot
        Texture2D picture = ScreenCapture.CaptureScreenshotAsTexture();
        uiCanvas.enabled = true;

        //Save the screenshot as a PNG file on the phone
        string fileName = "capture_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png";
        string imagePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(imagePath, picture.EncodeToPNG());
        Destroy(picture);

        //Add the photo to the cart (saved to Firebase, and updates the cart number)
        PlayerData player = PlayerDataManager.Instance.Data;
        string itemName = "My Setup " + (player.cart.Count + 1);
        long timeNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        PlayerDataManager.Instance.AddCartItem(new CartItem(itemName, imagePath, timeNow));

        //Give a promo code for the first 3 photos
        string message = "Photo added to your cart!";

        int photoNumber = player.promoCodes.Count;   // How many codes they already have
        if (photoNumber < promoCodes.Count)
        {
            string code = promoCodes[photoNumber];
            PlayerDataManager.Instance.AddPromoCode(code);
            message += "\n\nYou got a promo code: " + code +
                       "\nUse it when you buy items at the Swee Lee shop!";
        }

        //Show the popup message
        popup.Show(message);
    }
}