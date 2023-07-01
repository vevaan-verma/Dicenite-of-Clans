using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlidingButton : MonoBehaviour {

    [Header("Slide Settings")]
    [SerializeField] private float slideDuration;
    [SerializeField] private Transform slideTarget;
    private Vector2 slideInitialPosition;
    private Button button;
    bool slideEnabled;
    bool slidOut;

    [Header("Animations")]
    private Coroutine slideCoroutine;

    private void Start() {

        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry1 = new EventTrigger.Entry();
        entry1.eventID = EventTriggerType.PointerEnter;
        entry1.callback.AddListener((eventData) => { StartSlideOut(); });
        trigger.triggers.Add(entry1);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerExit;
        entry2.callback.AddListener((eventData) => { StartSlideIn(); });
        trigger.triggers.Add(entry2);

        slideInitialPosition = transform.position;

        button = GetComponent<Button>();

        slideEnabled = true;

    }

    public void StartSlideOut() {

        if (!slideEnabled) {

            return;

        }

        if (slideCoroutine != null) {

            StopCoroutine(slideCoroutine);

        }

        slideCoroutine = StartCoroutine(SlideButton(transform.position, slideTarget.position, true));

    }

    public void StartSlideIn() {

        if (slideCoroutine != null) {

            StopCoroutine(slideCoroutine);

        }

        slideCoroutine = StartCoroutine(SlideButton(transform.position, slideInitialPosition, false));

    }

    public void EnableSlideIn() {

        button.interactable = true;
        slideEnabled = true;

    }

    public void DisableSlideIn() {

        if (slidOut) {

            StartSlideIn();

        }

        button.interactable = false;
        slideEnabled = false;

    }

    private IEnumerator SlideButton(Vector2 startPosition, Vector2 targetPosition, bool slideOut) {

        float currentTime = 0f;

        while (currentTime < slideDuration) {

            currentTime += Time.deltaTime;
            transform.position = Vector2.Lerp(startPosition, targetPosition, currentTime / slideDuration);
            yield return null;

        }

        transform.position = targetPosition;
        slideCoroutine = null;
        slidOut = slideOut;

    }
}
