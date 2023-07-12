using UnityEngine;

public class MainMenuAudioManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip musicDay;
    [SerializeField] private AudioClip musicNight;
    [SerializeField] private AudioClip musicNightAlt;

    public enum MainMenuSoundType {

        Music_Day, Music_Night

    }

    private void Start() {

        backgroundAudioSource.loop = true;

    }

    public void PlaySound(MainMenuSoundType soundType) {

        switch (soundType) {

            case MainMenuSoundType.Music_Day:

            backgroundAudioSource.clip = musicDay;
            backgroundAudioSource.Play();
            break;

            case MainMenuSoundType.Music_Night:

            int musicNum = Random.Range(0, 2);

            if (musicNum == 0) {

                backgroundAudioSource.clip = musicNight;

            } else {

                backgroundAudioSource.clip = musicNightAlt;

            }

            backgroundAudioSource.Play();
            break;

            default:

            break;

        }
    }
}
