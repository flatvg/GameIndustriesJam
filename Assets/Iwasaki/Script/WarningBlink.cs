using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningBlink : MonoBehaviour
{
    public float blinkSpeed = 2f;
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void Update()
    {
        Color c = img.color;
        c.a = 0.3f + 0.5f * Mathf.Sin(Time.time * blinkSpeed);
        img.color = c;
    }
}
