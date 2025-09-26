using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    [Range(0f, 1f)] public float defaultAlpha = 1f;
    public Color flashColor = Color.white;

    Canvas _canvas;
    Image _image;
    Coroutine _running;

    void Awake()
    {
        // フルスクリーンのオーバーレイCanvasを用意
        _canvas = new GameObject("FlashCanvas").AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = short.MaxValue; // いちばん手前

        var scaler = _canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        var go = new GameObject("FlashImage");
        go.transform.SetParent(_canvas.transform, false);

        _image = go.AddComponent<Image>();
        _image.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f); // 最初は透明
        _image.raycastTarget = false;

        var rt = _image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _canvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// 画面をフラッシュ（フレーム数指定）
    /// </summary>
    public void FlashFrames(int frames, float alpha = -1f)
    {
        if (frames < 1) return;
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(CoFlashFrames(frames, alpha < 0 ? defaultAlpha : Mathf.Clamp01(alpha)));
    }

    IEnumerator CoFlashFrames(int frames, float alpha)
    {
        _canvas.gameObject.SetActive(true);
        _image.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);

        // ちょうどNフレーム分保持（タイムスケールの影響なし）
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();

        // 消す
        _image.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        _canvas.gameObject.SetActive(false);
        _running = null;
    }

    /// <summary>
    /// 秒指定でフラッシュ（フェードアウト付き）
    /// </summary>
    public void FlashSeconds(float holdSeconds = 0.05f, float fadeSeconds = 0.1f, float alpha = -1f)
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(CoFlashSeconds(Mathf.Max(0, holdSeconds), Mathf.Max(0, fadeSeconds), alpha < 0 ? defaultAlpha : Mathf.Clamp01(alpha)));
    }

    IEnumerator CoFlashSeconds(float hold, float fade, float alpha)
    {
        _canvas.gameObject.SetActive(true);
        _image.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);

        // 保持（リアルタイムで）
        float t = 0f;
        while (t < hold) { t += Time.unscaledDeltaTime; yield return null; }

        // フェードアウト
        t = 0f;
        while (t < fade)
        {
            float a = Mathf.Lerp(alpha, 0f, t / Mathf.Max(fade, 0.0001f));
            _image.color = new Color(flashColor.r, flashColor.g, flashColor.b, a);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        _image.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        _canvas.gameObject.SetActive(false);
        _running = null;
    }
}
