using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFadeOut : MonoBehaviour
{
    [Header("設定")]
    [Min(0f)] public float delay = 0f;        // 開始までの待機秒数
    [Min(0.01f)] public float duration = 2f;  // フェードにかける秒数
    public bool ignoreTimeScale = false;      // Time.timeScale無視するか
    public AnimationCurve ease = AnimationCurve.Linear(0, 0, 1, 1); // 補間カーブ

    SpriteRenderer _sr;
    Coroutine _co;
    CapsuleCollider2D col;

    void Awake()
    {
        col = GetComponent<CapsuleCollider2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(_sr.color.a < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>フェード開始（既存フェードは打ち切り）</summary>
    public void Play()
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FadeOutRoutine());
    }

    /// <summary>進行中のフェードを中断</summary>
    public void StopFade()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
    }

    IEnumerator FadeOutRoutine()
    {
        if (delay > 0f)
        {
            float tDelay = 0f;
            while (tDelay < delay)
            {
                tDelay += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }

        Color c = _sr.color;
        float startA = c.a;
        float t = 0f;

        while (t < duration)
        {
            t += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float k = ease.Evaluate(u);        // 0→1（カーブ適用）

            c.a = Mathf.Lerp(startA, 0f, k);   // アルファを補間
            _sr.color = c;
            yield return null;
        }

        // 最終値を保証
        c.a = 0f;
        _sr.color = c;
        _co = null;
    }
}