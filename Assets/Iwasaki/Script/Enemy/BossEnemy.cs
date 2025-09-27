using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossEnemy : EnemyBase
{
    public static event Action<BossEnemy> OnBossDefeated;
    public event Action<bool> OnSpawnPhaseChanged;

    public enum BossState { Approach, SpawnPhase, MoveToBeamPosition,WindUp, BeamAttack, ReturnFromBeam, PostAttack, Enraged, Dead }
    [SerializeField] private BossState state = BossState.Approach;

    [Header("�V�[���h")]
    [SerializeField] private GameObject shieldObject; // �����������̃I�u�W�F�N�g

    [Header("�x��UI")]
    [SerializeField] private Canvas warningCanvas;
    [SerializeField] private Image verticalWarningPrefab;
    [SerializeField] private Image horizontalWarningPrefab;

    [Header("�r�[��Prefab")]
    [SerializeField] private GameObject verticalBeamPrefab;
    [SerializeField] private GameObject horizontalBeamPrefab;

    private Image[] verticalWarnings = new Image[2];
    private Image[] horizontalWarnings = new Image[3];

    [Header("�����ݒ�")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int spawnCountPerTick = 2;
    [SerializeField] private float spawnRadius = 3f;

    [Header("�s��/����")]
    [SerializeField, Tooltip("�{�X���Ǝ�ɂȂ�q����N�������ԁi�b�j")]
    private float spawnPhaseDuration = 8f;
    [SerializeField, Tooltip("�r�[�����ˑO�̌x�����ԁi�b�j")]
    private float warnDuration = 1.2f;
    [SerializeField, Tooltip("�r�[���U���̎������ԁi�b�j")]
    private float beamDuration = 0.6f;
    [SerializeField, Tooltip("�r�[����̍d�����ԁi�b�j")]
    private float postAttackDuration = 1.2f;

    [Header("�ړ��|�C���g�i0:�ʏ�ʒu�A1:Beam�ޔ��ʒu�c�j")]
    [SerializeField] private Vector2[] movePoints;

    // internal
    private Coroutine stateLoopCoroutine;
    private bool isEnraged = false;
    private int currentPointIndex = 0;
    private bool reachedPoint = false;

    private Vector2 targetPoint;
    private bool movingToPoint;

    private int loopIndex = 0; // 1����=0, 2����=1...

    public void SetLoopIndex(int index)
    {
        loopIndex = index;
    }

    // ����{�� (WaveManager ���ݒ肷��)
    private float hpMultiplier = 1f;

    public void SetHpMultiplier(float multiplier)
    {
        hpMultiplier = multiplier;
        int newMax = Mathf.Max(1, Mathf.RoundToInt(GetMaxHp() * hpMultiplier));
        SetMaxHp(newMax);
        // ����HP���ő�ɑ�����i�{�X�̓��[�v�J�n���t��HP�ŏo���Ɨǂ��j
        hp = newMax;
    }

    protected override void OnInit()
    {
        base.OnInit();
        // �{�X�͕����ŉ�����Ȃ��悤�� kinematic ����
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // �x��UI����
        if (warningCanvas != null)
        {
            for (int i = 0; i < 2; i++)
            {
                verticalWarnings[i] = Instantiate(verticalWarningPrefab, warningCanvas.transform);
                RectTransform rt = verticalWarnings[i].rectTransform;
                rt.anchorMin = new Vector2(i * 0.5f, 0);
                rt.anchorMax = new Vector2((i + 1) * 0.5f, 1);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);
                verticalWarnings[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < 3; i++)
            {
                horizontalWarnings[i] = Instantiate(horizontalWarningPrefab, warningCanvas.transform);
                RectTransform rt = horizontalWarnings[i].rectTransform;
                rt.anchorMin = new Vector2(0, i / 3f);
                rt.anchorMax = new Vector2(1, (i + 1) / 3f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);
                horizontalWarnings[i].gameObject.SetActive(false);
            }
        }

        StartStateMachine();
        // �ŏ���OFF
        if (shieldObject != null) shieldObject.SetActive(false);
        HideAllWarnings();
    }

    private void StartStateMachine()
    {
        if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
        stateLoopCoroutine = StartCoroutine(StateLoop());
    }

    // Boss �̏�Ԃ� WindUp �܂��� BeamAttack �̂Ƃ��͏�ԗR���̖��G�Ƃ���
    protected override bool IsStateInvincible()
    {
        return state == BossState.WindUp || state == BossState.BeamAttack;
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        if (shieldObject != null)
            shieldObject.SetActive(invincible); // ���G������ON
    }

    private IEnumerator StateLoop()
    {
        while (!isDead)
        {
            // Approach
            state = BossState.Approach;
            MoveToPoint(movePoints[0]);
            yield return new WaitUntil(() => ReachedTarget());

            // --- SPAWN PHASE ---
            state = BossState.SpawnPhase;
            // �������̖��G��_���[�W�������c���Ă���\�������邽�ߖ����I�ɉ���
            EndInvincibility(); // �� �ǉ��iEnemyBase �� protected ���\�b�h���Ăԁj
            SetInvincible(false); // �Ǝ�

            // �����Œʒm�i�J�n�j
            OnSpawnPhaseChanged?.Invoke(true);

            float t = 0f;
            float spawnTick = 0f;
            while (t < spawnPhaseDuration && !isDead)
            {
                t += Time.deltaTime;
                spawnTick += Time.deltaTime;
                if (spawnTick >= spawnInterval)
                {
                    spawnTick -= spawnInterval;
                    SpawnMinions(screenSafe: true, count: spawnCountPerTick * (1 + (loopIndex % 2)));
                }
                yield return null;
            }
            if (isDead) break;

            // �����Œʒm�i�I���j
            OnSpawnPhaseChanged?.Invoke(false);

            // --- ������ ---
            state = BossState.MoveToBeamPosition;
            MoveToPoint(movePoints[1]); // �� 1�Ԃɓ�����ʒu��o�^
            yield return new WaitUntil(() => ReachedTarget());

            // --- �r�[���p�^�[�� ---
            // ���[�v���ɉ����ăp�^�[����ς���
            // 1����: �c�r�[�����E����
            // 2����: �܂��c�����r�[��
            // 3���ڈȍ~: �c�������c�{��
            // ����ɔh���p�^�[����ǉ����Ă��ǂ�
            yield return StartCoroutine(FireBeamPattern(loopIndex));

            // --- �߂� ---
            state = BossState.ReturnFromBeam;
        SetInvincible(false);
        MoveToPoint(movePoints[0]); // �� 0�Ԃ͉�ʓ��̏ꏊ
        yield return new WaitUntil(() => ReachedTarget());

            // --- POST ATTACK ---
            state = BossState.PostAttack;
            SetInvincible(false); // �U���I�������Ǝ�ɖ߂�
            yield return new WaitForSeconds(postAttackDuration);

            // Enraged check (��: HP臒l or loop�ɂ��)
            if (!isEnraged && (hp <= GetMaxHp() * 0.5f))
            {
                EnterEnraged();
            }
        }
    }
    private IEnumerator FireBeamPattern(int loopIndex)
    {
        if (isDead) yield break;

        // --- WINDUP (���G + �x��) ---
        state = BossState.WindUp;
        SetInvincible(true);

        // --- �r�[�����O�Ɍx�� ---
        switch (loopIndex)
        {
            case 0:
                // �c�r�[���F�E���������_��
                int side = UnityEngine.Random.Range(0, 2); // 0=��,1=�E
                yield return ShowWarning(verticalWarnings[side], warnDuration);
                FireVerticalBeam(side);
                break;

            case 1:
                // ��3����
                for (int i = 0; i < 3; i++)
                {
                    yield return ShowWarning(horizontalWarnings[i], warnDuration);
                    FireHorizontalBeam(i);
                }
                break;

            case 2:
                // �c�i�E�������_���j + ��3����
                side = UnityEngine.Random.Range(0, 2);
                yield return ShowWarning(verticalWarnings[side], warnDuration);
                FireVerticalBeam(side);
                for (int i = 0; i < 3; i++)
                {
                    yield return ShowWarning(horizontalWarnings[i], warnDuration);
                    FireHorizontalBeam(i);
                }
                break;

            case 3:
                // �c�i�E�������_���j + ��3����
                side = UnityEngine.Random.Range(0, 2);
                yield return ShowWarning(verticalWarnings[side], warnDuration);
                FireVerticalBeam(side);
                for (int i = 0; i < 3; i++)
                {
                    yield return ShowWarning(horizontalWarnings[i], warnDuration);
                    FireHorizontalBeam(i);
                }
                break;

            case 4:
                // �c�����_�� �� ��3���� �� �c�t �� ��3����
                side = UnityEngine.Random.Range(0, 2);
                int oppositeSide = 1 - side;
                yield return ShowWarning(verticalWarnings[side], warnDuration);
                FireVerticalBeam(side);
                for (int i = 0; i < 3; i++)
                {
                    yield return ShowWarning(horizontalWarnings[i], warnDuration);
                    FireHorizontalBeam(i);
                }
                yield return ShowWarning(verticalWarnings[oppositeSide], warnDuration);
                FireVerticalBeam(oppositeSide);
                for (int i = 0; i < 3; i++)
                {
                    yield return ShowWarning(horizontalWarnings[i], warnDuration);
                    FireHorizontalBeam(i);
                }
                break;
        }

        // --- BEAM ATTACK ---
        state = BossState.BeamAttack;
        yield return new WaitForSeconds(beamDuration);
        if (isDead) yield break;

        // --- �߂� ---
        state = BossState.ReturnFromBeam;
        SetInvincible(false);
        HideAllWarnings();
        MoveToPoint(movePoints[0]);
        yield return new WaitUntil(() => ReachedTarget());
    }
    private IEnumerator ShowWarning(Image warning, float duration)
    {
        warning.gameObject.SetActive(true);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.PingPong(Time.time * 2f, 0.5f) + 0.3f;
            warning.color = new Color(1, 0, 0, alpha);
            yield return null;
        }
        warning.gameObject.SetActive(false);
    }

    private GameObject SpawnBeam(GameObject prefab, RectTransform rect)
    {
        Vector3 worldPos;

        // Canvas �� Screen Space Overlay �̏ꍇ
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rect,  // �x��UI��RectTransform
            rect.position, // �X�N���[�����W
            Camera.main,   // �\���p�̃J����
            out worldPos);

        // �K�v�Ȃ�Z��0��
        worldPos.z = 0f;

        var beam = Instantiate(prefab, worldPos, prefab.transform.rotation);

        // �r�[���̎����i��F2�b��ɏ�����j
        Destroy(beam, beamDuration);

        return beam;
    }

    private void FireVerticalBeam(int index)
    {
        SpawnBeam(verticalBeamPrefab, verticalWarnings[index].rectTransform);
    }

    private void FireHorizontalBeam(int index)
    {
        SpawnBeam(horizontalBeamPrefab, horizontalWarnings[index].rectTransform);
    }

    private void FireVerticalAndHorizontalBeam()
    {
        for (int i = 0; i < 2; i++) FireVerticalBeam(i);
        for (int i = 0; i < 3; i++) FireHorizontalBeam(i);
    }

    private void HideAllWarnings()
    {
        if (verticalWarnings != null)
        {
            foreach (var w in verticalWarnings) if (w != null) w.gameObject.SetActive(false);
        }
        if (horizontalWarnings != null)
        {
            foreach (var w in horizontalWarnings) if (w != null) w.gameObject.SetActive(false);
        }
    }

    private void EnterEnraged()
    {
        isEnraged = true;
        state = BossState.Enraged;
        // ��: spawn�p�x���グ�� / spawn���𑝂₷
        spawnCountPerTick += 1;
        spawnInterval *= 0.9f; // 10% ����
        // optional: change beam pattern
    }

    private void SpawnMinions(bool screenSafe, int count)
    {
        Camera cam = Camera.main;
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos;
            if (screenSafe && cam != null)
            {
                // camera�������_���ʒu�i�[��������������j
                float vx = UnityEngine.Random.Range(0.08f, 0.92f);
                float vy = UnityEngine.Random.Range(0.08f, 0.92f);
                spawnPos = cam.ViewportToWorldPoint(new Vector3(vx, vy, Mathf.Abs(cam.transform.position.z - transform.position.z)));
                spawnPos.z = 0f;
                // optional: enforce near-boss radius
                if (Vector3.Distance(spawnPos, transform.position) > spawnRadius)
                {
                    // clamp toward boss
                    Vector3 dir = (spawnPos - transform.position).normalized;
                    spawnPos = transform.position + dir * Mathf.Min(spawnRadius, Vector3.Distance(spawnPos, transform.position));
                }
            }
            else
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
                spawnPos = transform.position + (Vector3)offset;
            }

            var go = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            // �X�P�[�����O���K�v�Ȃ炱���ŎQ�Ƃ��擾���� SetMaxHp �����Ă�
            var eb = go.GetComponent<EnemyBase>();
            if (eb != null)
            {
                // ��FLoop �ŃO���[�o���{��������ꍇ�AWaveManager ���ݒ肷��i�O���ˑ��j
            }
        }
    }

    private void MoveToPoint(Vector2 point)
    {
        targetPoint = point;
        movingToPoint = true;
    }

    private bool ReachedTarget()
    {
        return !movingToPoint;
    }

    protected override void Move()
    {
        if (movingToPoint)
        {
            Vector3 dir = (Vector3)targetPoint - transform.position;
            float dist = dir.magnitude;
            if (dist > 0.05f)
            {
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
            }
            else
            {
                movingToPoint = false;
            }
        }
        
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        // notify WaveManager
        OnBossDefeated?.Invoke(this);
    }
}