using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverTextButton : MonoBehaviour {

    [Header("UI References")]
    private Button button;
    private TMP_Text text;
    private Color startColor;

    [Header("Animations")]
    [SerializeField] private float textFadeDuration;
    [SerializeField] private Color textFadeColor;
    private Coroutine textFadeCoroutine;

    private void Start() {

        button = GetComponent<Button>();
        text = GetComponentInChildren<TMP_Text>();

        startColor = text.color;

        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry1 = new EventTrigger.Entry();
        entry1.eventID = EventTriggerType.PointerEnter;
        entry1.callback.AddListener((eventData) => { StartTextHover(text); });
        trigger.triggers.Add(entry1);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerExit;
        entry2.callback.AddListener((eventData) => { StopTextHover(text, startColor); });
        trigger.triggers.Add(entry2);

    }

    private void StartTextHover(TMP_Text text) {

        if (textFadeCoroutine != null) {

            StopCoroutine(textFadeCoroutine);

        }

        textFadeCoroutine = StartCoroutine(FadeText(text, text.color, textFadeColor));

    }

    private void StopTextHover(TMP_Text text, Color targetColor) {

        if (textFadeCoroutine != null) {

            StopCoroutine(textFadeCoroutine);

        }

        textFadeCoroutine = StartCoroutine(FadeText(text, text.color, targetColor));

    }

    private IEnumerator FadeText(TMP_Text text, Color startColor, Color targetColor) {

        float currentTime = 0f;

        while (currentTime < textFadeDuration) {

            currentTime += Time.deltaTime;
            text.color = Color.Lerp(startColor, targetColor, currentTime / textFadeDuration);
            yield return null;

        }

        text.color = targetColor;
        textFadeCoroutine = null;

    }
}
