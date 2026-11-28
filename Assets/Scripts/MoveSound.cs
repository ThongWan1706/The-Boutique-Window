using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Put in each item prefab
public class MoveSound : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip pickUpSound;
    public float volume = 1f;

    [Header("Music Note VFX")]
    public ParticleSystem musicNoteVFX;     // You can drag the particle prefab straight from the Project window
    public Vector3 vfxOffset = new Vector3(0f, 0.5f, 0f); // Where the notes appear, compared to the item (0.5 = a bit above it)
    public float vfxDuration = 2f;          // How long the notes keep coming out after pickup
    public bool stopVFXWhenReleased = false; // Tick this if the notes should stop as soon as the item is let go

    XRGrabInteractable grabInteractable;
    AudioSource speaker;
    Coroutine vfxRoutine;

    void Awake()
    {
        grabInteractable = GetComponentInChildren<XRGrabInteractable>();

        // Add a speaker to the item so the sound comes from where the item is
        speaker = gameObject.AddComponent<AudioSource>();
        speaker.playOnAwake = false;
        speaker.spatialBlend = 1f;
        speaker.volume = volume;

        if (musicNoteVFX != null)
        {
            // If the VFX is a prefab from the Project window (not inside this item),
            // make a copy of it and attach it to this item
            if (!musicNoteVFX.gameObject.scene.IsValid())
            {
                musicNoteVFX = Instantiate(musicNoteVFX, transform);
                musicNoteVFX.transform.localPosition = vfxOffset;
            }

            // Make sure the notes don't play when the item first appears
            musicNoteVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // When the item is turned on
    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPickedUp);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    // When the item is removed or turned off
    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnPickedUp);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    // Every time the player picks up the item
    void OnPickedUp(SelectEnterEventArgs args)
    {
        if (pickUpSound != null)
            speaker.PlayOneShot(pickUpSound);

        PlayVFX();
    }

    // Every time the player lets go of the item
    void OnReleased(SelectExitEventArgs args)
    {
        if (stopVFXWhenReleased)
            StopVFX();
    }

    void PlayVFX()
    {
        if (musicNoteVFX == null) return;

        // If it's already playing, restart the timer
        if (vfxRoutine != null)
            StopCoroutine(vfxRoutine);

        vfxRoutine = StartCoroutine(PlayVFXForAWhile());
    }

    IEnumerator PlayVFXForAWhile()
    {
        musicNoteVFX.Play();
        yield return new WaitForSeconds(vfxDuration);
        StopVFX();
    }

    void StopVFX()
    {
        if (musicNoteVFX == null) return;

        if (vfxRoutine != null)
        {
            StopCoroutine(vfxRoutine);
            vfxRoutine = null;
        }

        // Stop making new notes, but let the ones already out fade away naturally
        musicNoteVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}