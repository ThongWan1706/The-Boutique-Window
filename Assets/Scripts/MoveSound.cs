using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Put in each item prefab
public class MoveSound : MonoBehaviour
{
    public AudioClip pickUpSound;
    public float volume = 1f;

    XRGrabInteractable grabInteractable;
    AudioSource speaker;

    void Awake()
    {
        grabInteractable = GetComponentInChildren<XRGrabInteractable>();

        // Add a speaker to the item so the sound comes from where the item is
        speaker = gameObject.AddComponent<AudioSource>();
        speaker.playOnAwake = false;
        speaker.spatialBlend = 1f;
        speaker.volume = volume;
    }

    // When the item gets picked up
    void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.AddListener(OnPickedUp);
    }

    // When the item is removed or turned off
    void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnPickedUp);
    }

    // Every time the player picks up the item
    void OnPickedUp(SelectEnterEventArgs args)
    {
        if (pickUpSound != null)
            speaker.PlayOneShot(pickUpSound);
    }
}
