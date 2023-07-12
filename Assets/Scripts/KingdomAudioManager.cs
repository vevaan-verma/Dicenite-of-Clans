using UnityEngine;

public class KingdomAudioManager : MonoBehaviour {

    [Header("References")]
    private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip rotateSound;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip buySound;

    public enum KingdomSoundType {

        Click, Place, Rotate, Remove, Error, Buy

    }

    private void Start() {

        audioSource = GetComponent<AudioSource>();

    }

    public void PlaySound(KingdomSoundType soundType) {

        switch (soundType) {

            case KingdomSoundType.Click:

            audioSource.PlayOneShot(clickSound);
            break;

            case KingdomSoundType.Place:

            audioSource.PlayOneShot(placeSound);
            break;

            case KingdomSoundType.Rotate:

            audioSource.PlayOneShot(rotateSound);
            break;

            case KingdomSoundType.Remove:

            audioSource.PlayOneShot(removeSound);
            break;

            case KingdomSoundType.Error:

            audioSource.PlayOneShot(errorSound);
            break;

            case KingdomSoundType.Buy:

            audioSource.PlayOneShot(buySound);
            break;

            default:

            break;

        }
    }
}
