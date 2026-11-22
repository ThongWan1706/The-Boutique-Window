using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on Object Menu
public class MenuCategoryFilter : MonoBehaviour
{
    public TMP_Dropdown filterDropdown;
    public Transform menuContent;       // Contents in the object menu
    public ScrollRect menuScrollView;   // Scroll View of the object menu

    void Start()
    {
        // Starts every time the player picks a dropdown option
        filterDropdown.onValueChanged.AddListener(ShowCategory);
        ShowCategory(0);
    }

    // Shows only the buttons whose name contains the chosen option's text.
    void ShowCategory(int optionNumber)
    {
        string chosenWord = filterDropdown.options[optionNumber].text.ToLower();

        foreach (Transform button in menuContent)
        {
            string buttonName = button.name.ToLower();
            bool showButton = chosenWord == "all" || buttonName.Contains(chosenWord);
            button.gameObject.SetActive(showButton);
        }

        // Re-arrange the buttons left so there are no gaps, then scroll back to the start
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)menuContent);
        if (menuScrollView != null)
        {
            menuScrollView.horizontalNormalizedPosition = 0;
        }
    }
}