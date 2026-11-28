using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupMessage : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text messageText;
    public Button okButton;

    readonly Queue<string> queue = new Queue<string>();

    void Awake()
    {
        okButton.onClick.AddListener(ShowNext);
        panel.SetActive(false);
    }

    public void Show(string message)
    {
        queue.Enqueue(message);
        if (!panel.activeSelf)
            ShowNext();
    }

    void ShowNext()
    {
        if (queue.Count == 0)
        {
            panel.SetActive(false);
            return;
        }

        messageText.text = queue.Dequeue();
        panel.SetActive(true);
    }
}