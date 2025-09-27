using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnaer : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        [Header("基本設定")]
        public GameObject prefab;
        [Range(0f, 100f)] public float weight = 10f; // 相対重み

        [Header("レベル設定")]
        [Tooltip("この敵の基本最小Lv")]
        public int baseMinLevel = 1;
        [Tooltip("この敵の基本最大Lv")]
        public int baseMaxLevel = 1;
        [Tooltip("周回ごとに増える最小Lv（例: 1なら loopIndex=1毎に +1）")]
        public int minLevelIncreasePerLoop = 0;
        [Tooltip("周回ごとに増える最大Lv")]
        public int maxLevelIncreasePerLoop = 0;
        [Tooltip("スポーン時の確率フィルター（0~1）。重みで選ばれてもこの確率で実際にスポーンする）")]
        [Range(0f, 1f)]
        public float spawnChance = 1f;

        [Header("その他の強化設定")]
        public float speedMultiplier = 1.0f;
        public float sizeMultiplier = 1.0f;
    }

    [System.Serializable]
    public class WaveSpawnConfig
    {
        [Header("ウェーブ情報")]
        public string waveName = "Wave";
        public int waveNumber = 1;

        [Header("スポーン設定")]
        public float spawnInterval = 2f;        // 生成間隔（秒）
        public float spawnDistance = 10f;       // プレイヤーからの距離 (Viewport外に出す距離調整)
        public int maxEnemiesAtOnce = 5;        // 同時最大敵数
        [Header("ウェーブ継続時間（秒）")]
        public float waveDuration = 30f;

        [Header("敵構成")]
        public EnemySpawnData[] enemies;        // このウェーブの敵リスト

        [Header("次のウェーブまでの遅延")]
        public float nextWaveDelay = 5f;

        [Header("Boss設定 (ボスウェーブに使う)")]
        public bool bossWave = false;
        public GameObject bossPrefab;           // bossWave=true のときにスポーンする Boss Prefab
        [Tooltip("bossWave の時、ボスの SpawnPhase のみ雑魚をスポーンする")]
        public bool spawnOnlyDuringBossSpawnPhase = true;
    }

    [Header("ウェーブ設定")]
    [SerializeField] private WaveSpawnConfig[] waves;

    [Header("全体設定")]
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private bool loopWaves = true; // 最後のWaveをループ
    [SerializeField, Header("周回ごとの最小レベル上昇値(未使用なら0)")]
    private int levelIncreasePerLoop = 0; // 全体ベース補正（個別の増加設定と合わせて使う）
    [SerializeField, Header("Boss HP 強化倍率（ループごと）")]
    private float bossHpMultiplierPerLoop = 1.5f;
    [SerializeField, Header("レベル上限時に敵数増加 (追加敵/周回)")]
    private int extraEnemiesPerLoop = 2;

    [Header("デバッグ")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool showDebugInfo = true;

    // internal
    private Transform player;
    private bool isWaveActive = false;
    private float spawnTimer = 0f;
    private float waveTimer = 0f;
    private int loopCount = 0; // 0 は最初のループ（表示用は loopIndex = loopCount+1）
    private int currentEnemyCount = 0;

    // boss 管理
    private BossEnemy currentBoss = null;
    private bool bossAllowsSpawns = false; // ボスの SpawnPhase が来てるか

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag.");
            return;
        }

        if (autoStartWaves && waves != null && waves.Length > 0)
            StartCoroutine(WaveSequence());
    }

    private IEnumerator WaveSequence()
    {
        while (true)
        {
            for (int i = 0; i < waves.Length; i++)
            {
                currentWaveIndex = i;
                WaveSpawnConfig wave = waves[i];

                // 周回補正を反映（UIなどで参照するならここで）
                ApplyLoopScalingToWave(wave, loopCount);

                if (showDebugInfo) Debug.Log($"Start Wave {wave.waveName} Loop:{loopCount + 1}");

                yield return StartCoroutine(RunWave(wave));

                // 次ウェーブへの待ち
                if (i < waves.Length - 1 || loopWaves)
                    yield return new WaitForSeconds(wave.nextWaveDelay);
            }

            // 全Wave終わり -> 周回++
            loopCount++;
            if (!loopWaves) break; // ループしないなら抜ける
        }
    }

    private void ApplyLoopScalingToWave(WaveSpawnConfig wave, int loop)
    {
        // wave自体は Scriptable ではないので念のため直接変えるのは注意だが、
        // inspector設定を上書きしてよいなら次のように補正できます（軽微な補正のみ）。
        // 基本は Spawn 時に計算する方式を取っているのでここでは極力触らない。
    }

    private IEnumerator RunWave(WaveSpawnConfig wave)
    {
        isWaveActive = true;
        spawnTimer = 0f; // すぐ spawn
        waveTimer = wave.waveDuration;
        currentEnemyCount = 0;

        // ボスウェーブなら boss を生成して、ボスの SpawnPhase に合わせて雑魚を生成するロジックにする
        if (wave.bossWave && wave.bossPrefab != null)
        {
            // spawn boss at top center outside screen
            Vector3 bossSpawn = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.2f, Camera.main.nearClipPlane + wave.spawnDistance));
            bossSpawn.z = 0f;
            var bossGO = Instantiate(wave.bossPrefab, bossSpawn, Quaternion.identity);
            currentBoss = bossGO.GetComponent<BossEnemy>();
            if (currentBoss != null)
            {
                // loopIndex を boss に通知（ボス側で扱う）
                currentBoss.SetLoopIndex(loopCount + 1);
                // subscribe to spawn-phase events
                currentBoss.OnSpawnPhaseChanged += OnBossSpawnPhaseChanged;
            }
            // wait until boss appears on screen (or some time) -> here just wait a short time then enter boss loop
            yield return new WaitForSeconds(0.5f);
        }

        // ウェーブの持続時間。bossWaveの時は「bossが生きている間」や「指定duration」など自由に変えられるようにする。
        if (!wave.bossWave)
        {
            // 非ボスウェーブ：指定時間だけ spawn interval 管理で出す
            while (waveTimer > 0f)
            {
                waveTimer -= Time.deltaTime;
                UpdateWaveSpawning(wave);
                yield return null;
            }
        }
        else
        {
            // ボスウェーブ：ボスが存在する間（または waveDuration 経過）雑魚を出すが、設定により spawnOnlyDuringBossSpawnPhase を使う
            while ((currentBoss != null && !currentBoss.GetIsDead()) || waveTimer > 0f)
            {
                // waveTimerを減らしておく（オプション）
                waveTimer -= Time.deltaTime;

                // spawn only when boss allows spawns if configured
                UpdateWaveSpawning(wave);

                // boss が null か死んだら break するようにしておく
                if (currentBoss == null || currentBoss.GetIsDead())
                    break;

                yield return null;
            }

            // ボス終了処理：解除
            if (currentBoss != null)
            {
                currentBoss.OnSpawnPhaseChanged -= OnBossSpawnPhaseChanged;
                currentBoss = null;
                bossAllowsSpawns = false;
            }
        }

        isWaveActive = false;
        if (showDebugInfo) Debug.Log($"{wave.waveName} ended.");
    }

    // Boss から SpawnPhase イベントが来たとき
    private void OnBossSpawnPhaseChanged(bool spawnPhaseOn)
    {
        bossAllowsSpawns = spawnPhaseOn;
    }

    // 毎フレーム/ウェーブ中に呼ぶ spawn 更新
    private void UpdateWaveSpawning(WaveSpawnConfig wave)
    {
        if (!isWaveActive || wave == null) return;

        // ボスウェーブで spawnOnlyDuringBossSpawnPhase が true の場合は bossAllowsSpawns を見て制御
        if (wave.bossWave && wave.spawnOnlyDuringBossSpawnPhase && !bossAllowsSpawns)
            return;

        spawnTimer -= Time.deltaTime;

        // 現在のシーン内の敵数をカウント（軽めの処理）
        UpdateEnemyCount();

        if (spawnTimer <= 0f && currentEnemyCount < wave.maxEnemiesAtOnce)
        {
            SpawnRandomEnemyFromWave(wave);
            spawnTimer = wave.spawnInterval;
        }
    }

    private void UpdateEnemyCount()
    {
        // 軽量化：FindObjectsOfTypeを使うのは重めだが簡便。必要ならPooling/カウントで置換。
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        currentEnemyCount = enemies.Length;
    }

    // 画面外にスポーンする座標取得
    private Vector3 GetSpawnPositionOutsideCamera(float spawnDistance)
    {
        // Viewport の外側（左/右/上/下のいずれか）にランダムに出す
        float side = Random.value;
        Vector3 viewport;
        if (side < 0.25f)
        {
            // left
            viewport = new Vector3(-0.05f, Random.Range(-0.1f, 1.1f), Camera.main.nearClipPlane + spawnDistance);
        }
        else if (side < 0.5f)
        {
            // right
            viewport = new Vector3(1.05f, Random.Range(-0.1f, 1.1f), Camera.main.nearClipPlane + spawnDistance);
        }
        else if (side < 0.75f)
        {
            // top
            viewport = new Vector3(Random.Range(-0.1f, 1.1f), 1.05f, Camera.main.nearClipPlane + spawnDistance);
        }
        else
        {
            // bottom
            viewport = new Vector3(Random.Range(-0.1f, 1.1f), -0.05f, Camera.main.nearClipPlane + spawnDistance);
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewport);
        worldPos.z = 0f;
        return worldPos;
    }

    // ウェーブの敵から重み・確率を使って 1体選ぶ（選ばれないことがある場合は null を返す）
    private EnemySpawnData GetRandomEnemyFromWave(WaveSpawnConfig wave)
    {
        if (wave == null || wave.enemies == null || wave.enemies.Length == 0) return null;

        // まず重み合計を計算
        float totalWeight = 0f;
        foreach (var e in wave.enemies) totalWeight += Mathf.Max(0f, e.weight);

        if (totalWeight <= 0f) return null;

        // 重みで選択（ルーレット）だが、選ばれた後 spawnChance によるフィルタがある
        for (int attempt = 0; attempt < 6; attempt++) // 最大試行回数を決めて無限ループ回避
        {
            float rv = Random.Range(0f, totalWeight);
            float acc = 0f;
            foreach (var e in wave.enemies)
            {
                acc += Mathf.Max(0f, e.weight);
                if (rv <= acc)
                {
                    // spawnChance 判定
                    if (Random.value <= e.spawnChance)
                        return e;
                    else
                        break; // 試行失敗 -> 再試行
                }
            }
        }

        // フォールバック：最初の spawnChance を満たす敵を返す
        foreach (var e in wave.enemies)
            if (Random.value <= e.spawnChance) return e;

        return null;
    }

    // 実際に敵を生成する
    private void SpawnRandomEnemyFromWave(WaveSpawnConfig wave)
    {
        var selected = GetRandomEnemyFromWave(wave);
        if (selected == null || selected.prefab == null) return;

        // レベル計算（base + per-loop増加 + global増加）
        int minLv = selected.baseMinLevel + (selected.minLevelIncreasePerLoop * loopCount) + (levelIncreasePerLoop * loopCount);
        int maxLv = selected.baseMaxLevel + (selected.maxLevelIncreasePerLoop * loopCount) + (levelIncreasePerLoop * loopCount);

        minLv = Mathf.Clamp(minLv, 1, 10);
        maxLv = Mathf.Clamp(maxLv, minLv, 10);

        int chosenLevel = Random.Range(minLv, maxLv + 1);

        Vector3 spawnPos = GetSpawnPositionOutsideCamera(wave.spawnDistance);
        GameObject go = Instantiate(selected.prefab, spawnPos, Quaternion.identity);

        ApplyEnemyEnhancements(go, selected, chosenLevel);

        currentEnemyCount++;
        if (showDebugInfo)
            Debug.Log($"Spawned {selected.prefab.name} Lv.{chosenLevel} at {spawnPos}");
    }

    private void ApplyEnemyEnhancements(GameObject enemy, EnemySpawnData data, int level)
    {
        if (enemy == null || data == null) return;

        EnemyBase eb = enemy.GetComponent<EnemyBase>();
        if (eb != null)
        {
            eb.SetMaxHp(level);
            eb.SetMoveSpeed(eb.GetMoveSpeed() * data.speedMultiplier);
        }

        enemy.transform.localScale = enemy.transform.localScale * data.sizeMultiplier;

        // 視覚的エフェクトやレベル表示
        ApplyVisualLevelEffects(enemy, level);
    }

    #region Visual/Debug helpers
    private void ApplyVisualLevelEffects(GameObject enemy, int level)
    {
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (level >= 3) sr.color = Color.Lerp(sr.color, Color.red, (level - 2) * 0.15f);
            else if (level >= 2) sr.color = Color.Lerp(sr.color, Color.yellow, 0.3f);
        }

        if (level > 1)
        {
            AddLevelDisplay(enemy, level);
        }
    }

    private void AddLevelDisplay(GameObject enemy, int level)
    {
        GameObject levelDisplay = new GameObject("LevelDisplay");
        levelDisplay.transform.SetParent(enemy.transform);
        levelDisplay.transform.localPosition = Vector3.up * 0.7f;
        levelDisplay.transform.localScale = Vector3.one * 0.5f;
        TextMesh textMesh = levelDisplay.AddComponent<TextMesh>();
        textMesh.text = $"Lv.{level}";
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        levelDisplay.transform.rotation = Camera.main.transform.rotation;
    }
    #endregion

    // 公開ユーティリティ
    public List<EnemyBase> GetInScreenEnemyes()
    {
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        List<EnemyBase> inScreen = new List<EnemyBase>();
        foreach (var e in allEnemies)
        {
            Vector3 sp = Camera.main.WorldToScreenPoint(e.transform.position);
            if (sp.z > 0 && sp.x > 0 && sp.x < Screen.width && sp.y > 0 && sp.y < Screen.height)
                inScreen.Add(e);
        }
        return inScreen;
    }

    #region Editor / debug tools
    void OnGUI()
    {
        if (!showDebugInfo) return;
        GUILayout.BeginArea(new Rect(10, 10, 420, 240));
        GUILayout.Label($"=== Wave Spawner Debug ===");
        GUILayout.Label($"Loop: {loopCount + 1}");
        GUILayout.Label($"Wave: {(waves != null && currentWaveIndex < waves.Length ? waves[currentWaveIndex].waveName : "None")}");
        GUILayout.Label($"Enemies Alive: {currentEnemyCount}");
        GUILayout.EndArea();
    }
    #endregion
}
