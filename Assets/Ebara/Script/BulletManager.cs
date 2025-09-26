using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BulletManager : MonoBehaviour
{
    [Header("�����O�ݒ�")]
    [Min(0f)] public float radius = 2.0f;                  // �v���C���[����̋����i��]���j
    [SerializeField, Min(1)] int pointCount = 5;           // �e�̐��i�ρj
    public int bulletDamage = 1;                               // �e�̃_���[�W

    [SerializeField] float angleSpeed = 100.0f;            // �e�̉�]���x(Z)
    public GameObject bulletPrefab;                        // ��������e
    public Player player;                                  // �v���C���[
    public EnemySpawnaer spawnaer;
    public int chainCount = 5;
    public float thunderInterval = 0.05f;
    Coroutine chainRoutine;

    [SerializeField] float spriteAlignDeg = 270f;          // �X�v���C�g�ɓK�p����z���̃I�t�Z�b�g
    [SerializeField] bool isDrawDebugTriangle = false;     // �f�o�b�O�p�O�p�`�`��t���O

    readonly List<Transform> points = new();               // �����O��|�C���g
    public List<Bullet> bullets = new();                 // ���������e

    private float rot;                                     // �ݐϊp

    List<Bullet> bulletBuffer = new(); // �X�L���Ŏg�p����o���b�g

    public GameObject beamPrefab;

    private ConnectTwoPoints connecter;

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

        connecter = GetComponent<ConnectTwoPoints>();
    }

    void Update()
    {
        // �v���C���[�ɒǏ]
        if (player != null)
            transform.position = player.transform.position;

        // �e�I�u�W�F�N�g���񂷁i�]���ʂ�j
        transform.Rotate(0.0f, 0.0f, angleSpeed * Time.deltaTime);

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

        if (Input.GetKeyDown(KeyCode.A))
        {
            //Vector2 start = new Vector2(player.transform.position.x, player.transform.position.y);
            //Vector2 end = start + (player.direction * 5);
            //connecter.CreateLineBetween(start, end);
            //GetComponent<ScreenFlash>().FlashSeconds(0.03f, 0.08f); // �e�X�g����
            //UseSkill2_2(); // �e�X�g����
            //UseSkill3_3(); // �e�X�g����
            UseSkill5_5();
        }
    }

    // ��]�ʒu���Đ���
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
                b.manager = this;      // �}�l�[�W���[���Z�b�g
                b.bindPoint = pt;      // ��]��ԂŒǏ]
                b.isShot = false;      // �����͉�]���
                bullets.Add(b);
            }
        }
    }

    // ��]�ʒu���X�V
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

    private bool IsUseSkill(int level, int count)
    {
        int c = 0;
        List<Bullet> skillBullets = new List<Bullet>();
        foreach (var bullet in bullets)
        {
            if (bullet.level >= level)
            {
                skillBullets.Insert(c++, bullet);
            }
        }
        if (c < count) return false;

        for (int i = 0; i < count; i++)
        {
            var v = skillBullets[i];
            if (v != null)
            {
                v.level = 1;
            }
        }
        skillBullets.Clear();

        return true;
    }

    // �X�L��(���͂ȍU��)���g�p
    public void UseSkill2_2()
    {
        if (!IsUseSkill(2, 2)) return;

        Vector2 targetPos = (Vector2)player.transform.position + (player.direction * radius);

        float a = Mathf.Atan2(player.direction.y, player.direction.x) * Mathf.Rad2Deg;
        a -= 90f;
        GameObject obj = GameObject.Instantiate(beamPrefab, targetPos, Quaternion.Euler(0f, 0f, a));
        BeamBullet b = obj.GetComponent<BeamBullet>();
        b.manager = this;
    }

    public void UseSkill3_3()
    {
        if (!IsUseSkill(3, 3)) return;

        Vector2 targetPos = (Vector2)player.transform.position + (player.direction * radius);

        // �`�d���X�g�i�ŏ��̓v���C���[�j
        List<Transform> chainPoints = new List<Transform> { player.transform };

        // ��ʓ��̓G�� Transform ���X�g�ցiTransform�^Component�^GameObject ���ł��Ή��j
        var raw = spawnaer.GetInScreenEnemyes() as System.Collections.IEnumerable;
        var candidates = new List<Transform>();
        if (raw != null)
        {
            foreach (var e in raw)
            {
                if (e is Transform t) candidates.Add(t);
                else if (e is Component cpt && cpt) candidates.Add(cpt.transform);
                else if (e is GameObject go && go) candidates.Add(go.transform);
            }
        }
        if (candidates.Count == 0) return;

        // �`�d�̗�����\�z�F���񌻍݈ʒu���甼�a���ōł��߂��G��I��
        Transform current = player.transform;
        float hopRadius = radius * 5;               // 1�z�b�v�̍ő勗���i�K�v�Ȃ璲���j
        int maxJumps = chainCount;                  // ���񒵂˂邩�i�K�v�������j
        var used = new HashSet<Transform> { current };

        for (int j = 0; j < maxJumps; j++)
        {
            Transform next = null;
            float bestSq = hopRadius * hopRadius;

            for (int i = 0; i < candidates.Count; i++)
            {
                var t = candidates[i];
                if (t == null || used.Contains(t)) continue;

                float d2 = (t.position - current.position).sqrMagnitude;
                if (d2 <= bestSq)
                {
                    bestSq = d2;
                    next = t;
                }
            }

            if (next == null) break; // ���a���ɑΏۂȂ��ŏI��
            chainPoints.Add(next);
            used.Add(next);
            current = next;
        }

        // �G�ɂ͈ꉞ�m��_���[�W��^����
        foreach (Transform t in chainPoints)
        {
            var e = t.GetComponent<EnemyBase>();
            e?.TakeDamage(3, Vector2.zero);
        }

        // �e�����N�Ԃɉ����C���i���j�𐶐�
        for (int i = 0; i < chainPoints.Count - 1; i++)
        {
            Vector2 a = chainPoints[i].position;
            Vector2 b = chainPoints[i + 1].position;
            connecter.CreateLineBetween(a, b);
        }
    }

    public void UseSkill5_5()
    {
        //if (!IsUseSkill(5, 5)) return;

        GetComponent<ScreenFlash>().FlashSeconds(0.06f, 0.16f);

        var enemies = spawnaer.GetInScreenEnemyes();
        foreach (var enemy in enemies)
        {
            enemy.TakeDamage(5, Vector2.zero);
        }
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
        if (bullets.Count == 0)
        {
            Debug.Log("Bullet Count Is 0.");
            return;
        }
        if (direction.sqrMagnitude < Mathf.Epsilon)
        {
            Debug.Log("Shot Direction is tiny.");
            return;
        }

        Vector2 center = transform.position;
        Vector2 targetPos = center + direction.normalized * radius;

        float best = float.MaxValue;
        Bullet pick = null;

        foreach (var b in bullets)
        {
            if (b.isShot) continue;
            float dist = Vector2.Distance(targetPos, (Vector2)b.transform.position);
            if (dist < best)
            {
                best = dist;
                pick = b;
            }
        }

        if (pick != null)
        {
            Vector2 d = center - targetPos;
            float deg = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            if (deg < 0f) deg += 360f;
            pick.Shot(direction.normalized, deg);
        }
        else
        {
            Debug.Log("Shotble Bullet Not Found.");
        }
    }

    // �M�Y���`��
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