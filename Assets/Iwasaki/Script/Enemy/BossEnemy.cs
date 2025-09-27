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

    [Header("シールド")]
    [SerializeField] private GameObject shieldObject; // 半透明白球のオブジェクト

    [Header("警告UI")]
    [SerializeField] private Canvas warningCanvas;
    [SerializeField] private Image verticalWarningPrefab;
    [SerializeField] private Image horizontalWarningPrefab;

    [Header("ビームPrefab")]
    [SerializeField] private GameObject verticalBeamPrefab;
    [SerializeField] private GameObject horizontalBeamPrefab;

    private Image[] verticalWarnings = new Image[2];
    private Image[] horizontalWarnings = new Image[3];

    [Header("召喚設定")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int spawnCountPerTick = 2;
    [SerializeField] private float spawnRadius = 3f;

    [Header("行動/時間")]
    [SerializeField, Tooltip("ボスが脆弱になり子分を湧かす時間（秒）")]
    private float spawnPhaseDuration = 8f;
    [SerializeField, Tooltip("ビーム発射前の警告時間（秒）")]
    private float warnDuration = 1.2f;
    [SerializeField, Tooltip("ビーム攻撃の持続時間（秒）")]
    private float beamDuration = 0.6f;
    [SerializeField, Tooltip("ビーム後の硬直時間（秒）")]
    private float postAttackDuration = 1.2f;

    [Header("移動ポイント（0:通常位置、1:Beam退避位置…）")]
    [SerializeField] private Vector2[] movePoints;

    // internal
    private Coroutine stateLoopCoroutine;
    private bool isEnraged = false;
    private int currentPointIndex = 0;
    private bool reachedPoint = false;

    private Vector2 targetPoint;
    private bool movingToPoint;

    private int loopIndex = 0; // 1周目=0, 2周目=1...

    public void SetLoopIndex(int index)
    {
        loopIndex = index;
    }

    // 周回倍率 (WaveManager が設定する)
    private float hpMultiplier = 1f;

    public void SetHpMultiplier(float multiplier)
    {
        hpMultiplier = multiplier;
        int newMax = Mathf.Max(1, Mathf.RoundToInt(GetMaxHp() * hpMultiplier));
        SetMaxHp(newMax);
        // 現在HPも最大に揃える（ボスはループ開始時フルHPで出すと良い）
        hp = newMax;
    }

    protected override void OnInit()
    {
        base.OnInit();
        // ボスは物理で押されないように kinematic 推奨
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 警告UI生成
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
        // 最初はOFF
        if (shieldObject != null) shieldObject.SetActive(false);
        HideAllWarnings();
    }

    private void StartStateMachine()
    {
        if (stateLoopCoroutine != null) StopCoroutine(stateLoopCoroutine);
        stateLoopCoroutine = StartCoroutine(StateLoop());
    }

    // Boss の状態が WindUp または BeamAttack のときは状態由来の無敵とする
    protected override bool IsStateInvincible()
    {
        return state == BossState.WindUp || state == BossState.BeamAttack;
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        if (shieldObject != null)
            shieldObject.SetActive(invincible); // 無敵時だけON
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
            // 生成時の無敵やダメージ無効が残っている可能性があるため明示的に解除
            EndInvincibility(); // ← 追加（EnemyBase の protected メソッドを呼ぶ）
            SetInvincible(false); // 脆弱

            // ここで通知（開始）
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

            // ここで通知（終了）
            OnSpawnPhaseChanged?.Invoke(false);

            // --- 逃げる ---
            state = BossState.MoveToBeamPosition;
            MoveToPoint(movePoints[1]); // ← 1番に逃げる位置を登録
            yield return new WaitUntil(() => ReachedTarget());

            // --- ビームパターン ---
            // ループ数に応じてパターンを変える
            // 1周目: 縦ビーム左右半分
            // 2周目: まず縦→横ビーム
            // 3周目以降: 縦→横→縦＋横
            // さらに派生パターンを追加しても良い
            yield return StartCoroutine(FireBeamPattern(loopIndex));

            // --- 戻る ---
            state = BossState.ReturnFromBeam;
        SetInvincible(false);
        MoveToPoint(movePoints[0]); // ← 0番は画面内の場所
        yield return new WaitUntil(() => ReachedTarget());

            // --- POST ATTACK ---
            state = BossState.PostAttack;
            SetInvincible(false); // 攻撃終わったら脆弱に戻す
            yield return new WaitForSeconds(postAttackDuration);

            // Enraged check (例: HP閾値 or loopによる)
            if (!isEnraged && (hp <= GetMaxHp() * 0.5f))
            {
                EnterEnraged();
            }
        }
    }
    private IEnumerator FireBeamPattern(int loopIndex)
    {
        if (isDead) yield break;

        // --- WINDUP (無敵 + 警告) ---
        state = BossState.WindUp;
        SetInvincible(true);

        // --- ビーム直前に警告 ---
        switch (loopIndex)
        {
            case 0:
                // 縦ビーム：右か左ランダム
                int side = UnityEngine.Random.Range(0, 2); // 0=左,1=右
                yield return ShowWarning(verticalWarnings[side], warnDuration);
                FireVerticalBeam(side);
                break;

            case 1:
                // 横3分割
                for (int i = 0; i < 3; i++)
                {
                    yield return ShowWarning(horizontalWarnings[i], warnDuration);
                    FireHorizontalBeam(i);
                }
                break;

            case 2:
                // 縦（右左ランダム） + 横3分割
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
                // 縦（右左ランダム） + 横3分割
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
                // 縦ランダム → 横3分割 → 縦逆 → 横3分割
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

        // --- 戻る ---
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

        // Canvas が Screen Space Overlay の場合
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rect,  // 警告UIのRectTransform
            rect.position, // スクリーン座標
            Camera.main,   // 表示用のカメラ
            out worldPos);

        // 必要ならZを0に
        worldPos.z = 0f;

        var beam = Instantiate(prefab, worldPos, prefab.transform.rotation);

        // ビームの寿命（例：2秒後に消える）
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
        // 例: spawn頻度を上げる / spawn数を増やす
        spawnCountPerTick += 1;
        spawnInterval *= 0.9f; // 10% 速く
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
                // camera内ランダム位置（端をすこし避ける）
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
            // スケーリングが必要ならここで参照を取得して SetMaxHp 等を呼ぶ
            var eb = go.GetComponent<EnemyBase>();
            if (eb != null)
            {
                // 例：Loop でグローバル倍率がある場合、WaveManager が設定する（外部依存）
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