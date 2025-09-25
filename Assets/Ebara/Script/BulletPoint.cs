using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class BulletPoint : MonoBehaviour
{
    [Header("�����O�ݒ�")]
    [Min(0f)] public float radius = 2.0f;                  // �v���C���[����̋����i��]���j
    [SerializeField, Min(1)] int pointCount = 5;           // �e�̐��i�ρj

    [SerializeField] float angleSpeed = 100.0f;            // �e�̉�]���x(Z)
    public GameObject bulletPrefab;                        // ��������e

    [SerializeField] float spriteAlignDeg = 270f;          // �X�v���C�g�ɓK�p����z���̃I�t�Z�b�g
    [SerializeField] bool isDrawDebugTriangle = false;     // �f�o�b�O�p�O�p�`�`��t���O

    readonly List<Transform> points = new();               // �����O��|�C���g
    readonly List<Bullet> bullets = new();                 // ���������e
    private float rotDeg;                                  // �ݐω�]�i�K�v�Ȃ�g�p�j

    // ���ߒl�i�ύX���m�p�j
    int lastPointCount;
    float lastRadius;
    bool lastIsDrawDebugTriangle;

    void Awake()
    {
        RebuildRing();     // ��������
        HandleDebugTriangle(true);
        lastPointCount = pointCount;
        lastRadius = radius;
        lastIsDrawDebugTriangle = isDrawDebugTriangle;
    }

    void Update()
    {
        // �e�I�u�W�F�N�g���񂷁i�]���ʂ�j
        transform.Rotate(0f, 0f, angleSpeed * Time.deltaTime);

        // �C���X�y�N�^��R�[�h����̕ύX�����m���Ĕ��f
        if (pointCount != lastPointCount)
        {
            RebuildRing();
            lastPointCount = pointCount;
        }
        else if (!Mathf.Approximately(radius, lastRadius))
        {
            UpdatePointPositions();
            lastRadius = radius;
        }

        // �f�o�b�O�p�O�p�`�`�搧��
        HandleDebugTriangle();

        /* TEST CODE */
        if (Input.GetMouseButtonDown(0))
            TryShotFromClick(Input.mousePosition);
    }

    void RebuildRing()
    {
        // �����̒e�ƃ|�C���g�𐮗�
        for (int i = 0; i < bullets.Count; i++)
        {
            if (bullets[i]) Destroy(bullets[i].gameObject);
        }
        bullets.Clear();

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i]) Destroy(points[i].gameObject);
        }
        points.Clear();

        // �V�K�Ƀ|�C���g�ƒe���쐬
        for (int i = 0; i < pointCount; i++)
        {
            var pt = new GameObject($"BulletPoint_{i}").transform;
            pt.SetParent(transform, false);
            points.Add(pt);
        }
        UpdatePointPositions(); // ���a�ɉ����ă����O�z�u

        // �e�̐����ƃo�C���h
        for (int i = 0; i < pointCount; i++)
        {
            var pt = points[i];
            var obj = Instantiate(bulletPrefab, pt.position, pt.rotation);
            // ��]��z���ɑ΂��ăI�t�Z�b�g��K�p
            //obj.transform.rotation *= Quaternion.Euler(0f, 0f, spriteAlignDeg);
            var b = obj.GetComponent<Bullet>();
            if (b != null)
            {
                b.bindPoint = pt; // ��]��ԂŒǏ]
                b.isShot = false; // �����͉�]���
                bullets.Add(b);
            }
        }
    }

    //void UpdatePointPositions()
    //{
    //    if (points.Count == 0) return;

    //    float step = 360f / Mathf.Max(1, pointCount);
    //    float baseDeg = rotDeg + transform.eulerAngles.z;

    //    for (int i = 0; i < points.Count; i++)
    //    {
    //        float deg = baseDeg + i * step;
    //        float rad = deg * Mathf.Deg2Rad;
    //        // XY���ʂ̉~��� localPosition �Ŕz�u
    //        Vector3 local = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
    //        points[i].localPosition = local;
    //        points[i].localRotation = Quaternion.Euler(0, 0, deg);
    //    }
    //}
    void UpdatePointPositions()
    {
        if (points.Count == 0) return;

        float step = 360f / Mathf.Max(1, pointCount);

        // �e�̌��݊p�x
        float parentZ = transform.eulerAngles.z;

        for (int i = 0; i < points.Count; i++)
        {
            // ���[���h�ł́g���ˊp�h���v�Z�i�e�̉�]�͂����ł͓���Ȃ��j
            float worldAngle = i * step; // �K�v�Ȃ��I�t�Z�b�g�𑫂�

            // �ʒu�̓��[�J���ŉ~�z�u�i�e�����Έꏏ�ɉ��j
            float rad = worldAngle * Mathf.Deg2Rad;
            Vector3 local = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            points[i].localPosition = local;

            // �����F���[���h�� worldAngle �������������̂ŁA
            // �q�� localRotation = worldAngle - �e�p + �X�v���C�g�␳
            float localFace = worldAngle - parentZ + spriteAlignDeg;
            points[i].localRotation = Quaternion.Euler(0, 0, localFace);
        }
    }

    void TryShotFromClick(Vector2 clickScreenPos)
    {
        // UI ��͖����i�C�Ӂj
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        var cam = Camera.main;
        if (!cam) return;

        // �����̃X�N���[�����W
        Vector3 selfScreen = cam.WorldToScreenPoint(transform.position);
        Vector2 dirScreen = clickScreenPos - (Vector2)selfScreen;
        if (dirScreen.sqrMagnitude < 1e-8f) return;

        // �X�N���[�� �� ���[���h�iXY�O��j
        float zDist = Mathf.Abs(transform.position.z - cam.transform.position.z);
        Vector3 clickWorld = cam.ScreenToWorldPoint(new Vector3(clickScreenPos.x, clickScreenPos.y, zDist));
        Vector2 dirWorld = (Vector2)(clickWorld - transform.position);

        Shot(dirWorld);
    }

    // �f�o�b�O�p�O�p�`�`�搧��
    void HandleDebugTriangle(bool forceChange = false)
    {
        if (lastIsDrawDebugTriangle != isDrawDebugTriangle || forceChange)
        {
            lastIsDrawDebugTriangle = isDrawDebugTriangle;
            foreach (Transform t in GetComponentInChildren<Transform>(true))
            {
                if (t == transform) continue; // ���g�͏��O
                SpriteRenderer renderer = t.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = isDrawDebugTriangle;
                }
            }
        }
    }

    // �ł��߂������˒e������
    public void Shot(Vector2 direction)
    {
        if (bullets.Count == 0) return;
        if (direction.sqrMagnitude < Mathf.Epsilon) return;

        Vector2 center = transform.position;
        Vector2 targetPos = center + direction.normalized * radius;

        float best = float.MaxValue;
        Bullet pick = null;

        foreach (var b in bullets)
        {
            if (!b || b.isShot) continue;
            float dist = Vector2.Distance(targetPos, b.transform.position);
            if (dist < best) { best = dist; pick = b; }
        }

        if (pick != null)
        {
            Vector2 d = center - targetPos;
            float deg = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            if (deg < 0f) deg += 360f;
            pick.Shot(direction.normalized, deg);
        }
    }

    // --- �M�Y���i�C�Ӂj ---
    void OnDrawGizmosSelected()
    {
        // �~
        Gizmos.color = Color.cyan;
        const int seg = 60;
        Vector3 prev = transform.position + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= seg; i++)
        {
            float t = (float)i / seg * Mathf.PI * 2f;
            Vector3 curr = transform.position + new Vector3(Mathf.Cos(t) * radius, Mathf.Sin(t) * radius, 0f);
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }
        // �_
        Gizmos.color = Color.yellow;
        float step = 360f / Mathf.Max(1, pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            float rad = (i * step) * Mathf.Deg2Rad;
            Vector3 p = transform.position + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            Gizmos.DrawWireSphere(p, 0.05f);
        }
    }
}