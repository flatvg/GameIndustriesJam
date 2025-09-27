using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMover2D : MonoBehaviour
{
    [Header("振動パラメータ")]
    [Tooltip("往復する向き（正規化は自動）")]
    public Vector2 direction = Vector2.right;
    [Tooltip("片振幅（ワールド単位）")]
    public float amplitude = 1f;
    [Tooltip("周波数（1=1秒に1往復）")]
    public float frequency = 1f;
    [Tooltip("初期位相（度）")]
    public float phaseDeg = 0f;

    [Header("挙動オプション")]
    public bool useLocalSpace = false;     // ローカル座標で動かす
    public bool useUnscaledTime = false;   // Time.timeScaleの影響を受けない
    public bool autoDetectRigidbody2D = true;

    Vector3 _basePos;          // 基準位置
    Rigidbody2D _rb;

    void Awake()
    {
        CacheBasePos();
        if (autoDetectRigidbody2D) _rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable() => CacheBasePos();

    void CacheBasePos()
    {
        _basePos = useLocalSpace ? transform.localPosition : transform.position;
    }

    void Update()
    {
        // 物理移動は FixedUpdate に任せる
        if (_rb != null && _rb.simulated) return;

        Vector3 pos = ComputePosition(CurrentTime());
        if (useLocalSpace) transform.localPosition = pos;
        else transform.position = pos;
    }

    void FixedUpdate()
    {
        if (_rb == null || !_rb.simulated) return;
        Vector3 pos = ComputePosition(CurrentTime());
        _rb.MovePosition(pos);
    }

    Vector3 ComputePosition(float t)
    {
        // 正規化方向 × 振幅 × sin(2πft + 位相)
        Vector2 dirN = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        float rad = Mathf.Deg2Rad * phaseDeg;
        float offset = Mathf.Sin(2f * Mathf.PI * frequency * t + rad) * amplitude;
        Vector2 delta = dirN * offset;

        Vector3 basePos3 = _basePos;
        basePos3.x += delta.x;
        basePos3.y += delta.y;
        return basePos3;
    }

    float CurrentTime() => useUnscaledTime ? Time.unscaledTime : Time.time;

#if UNITY_EDITOR
    void OnValidate()
    {
        amplitude = Mathf.Max(0f, amplitude);
        frequency = Mathf.Max(0f, frequency);
    }
#endif
}
