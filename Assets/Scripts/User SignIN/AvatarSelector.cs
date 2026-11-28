using UnityEngine;
using UnityEngine.UI;

using UnityEngine;

public class AvatarSelector : MonoBehaviour
{
    // Store the selected avatar ID globally (0, 1, or 2)
    public static int selectedAvatarID = 0; 

    public void SelectAvatarByID(int avatarID)
    {
        selectedAvatarID = avatarID;
        Debug.Log("Selected Avatar ID: " + selectedAvatarID);
    }
}