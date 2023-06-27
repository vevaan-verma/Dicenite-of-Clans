using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingImage : MonoBehaviour {

    [Header("UI References")]
    private RawImage image;

    [Header("Scroll Settings")]
    [SerializeField] private float xSpeed;
    [SerializeField] private float ySpeed;

    private void Start() {

        image = GetComponent<RawImage>();

    }

    private void Update() {

        image.uvRect = new Rect(image.uvRect.position + new Vector2(xSpeed, ySpeed) * Time.deltaTime, image.uvRect.size);

    }
}
