using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMover2D : MonoBehaviour
{
    [Header("�U���p�����[�^")]
    [Tooltip("������������i���K���͎����j")]
    public Vector2 direction = Vector2.right;
    [Tooltip("�АU���i���[���h�P�ʁj")]
    public float amplitude = 1f;
    [Tooltip("���g���i1=1�b��1�����j")]
    public float frequency = 1f;
    [Tooltip("�����ʑ��i�x�j")]
    public float phaseDeg = 0f;

    [Header("�����I�v�V����")]
    public bool useLocalSpace = false;     // ���[�J�����W�œ�����
    public bool useUnscaledTime = false;   // Time.timeScale�̉e�����󂯂Ȃ�
    public bool autoDetectRigidbody2D = true;

    Vector3 _basePos;          // ��ʒu
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
        // �����ړ��� FixedUpdate �ɔC����
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
        // ���K������ �~ �U�� �~ sin(2��ft + �ʑ�)
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
