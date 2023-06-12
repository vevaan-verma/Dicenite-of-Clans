using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip rotateSound;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip errorSound;

    public enum SoundType {

        Click, Place, Rotate, Remove, Error

    }

    public void PlaySound(SoundType soundType) {

        switch (soundType) {

            case SoundType.Click:

            audioSource.PlayOneShot(clickSound);
            break;

            case SoundType.Place:

            audioSource.PlayOneShot(placeSound);
            break;

            case SoundType.Rotate:

            audioSource.PlayOneShot(rotateSound);
            break;

            case SoundType.Remove:

            audioSource.PlayOneShot(removeSound);
            break;

            case SoundType.Error:

            audioSource.PlayOneShot(errorSound);
            break;

            default:
            break;

        }
    }
}
