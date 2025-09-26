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
        [Range(0f, 100f)]
        public float weight = 10f; // 出現重み(%)

        [Header("レベル設定")]
        [Range(1, 10)]
        public int minLevel = 1;        // 最小レベル
        [Range(1, 10)]
        public int maxLevel = 1;        // 最大レベル

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
        public float spawnDistance = 10f;       // プレイヤーからの距離
        public int maxEnemiesAtOnce = 5;        // 同時最大敵数
        [Header("ウェーブ継続時間")] public float waveDuration = 30f; // （秒）

        [Header("敵構成")]
        public EnemySpawnData[] enemies;        // このウェーブの敵リスト

        [Header("次のウェーブ")]
        public float nextWaveDelay = 5f;        // 次ウェーブまでの間隔
    }

    [Header("ウェーブ設定")]
    [SerializeField] private WaveSpawnConfig[] waves;

    [Header("全体設定")]
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private bool loopWaves = false;       // 最後のウェーブをループするか

    [Header("デバッグ")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool showDebugInfo = true;

    // EnemySpawner クラスに追加
    [SerializeField,Header("周回ごとの最小レベル上昇値")] private int levelIncreasePerLoop = 1;
    [SerializeField,Header("BossHP強化倍率")] private float bossHpMultiplierPerLoop = 1.5f;
    [SerializeField,Header("レベル上限時に敵数増加")] private int extraEnemiesPerLoop = 2;

    private Transform player;
    private float spawnTimer;
    private float waveTimer;
    private bool isWaveActive = false;
    private int currentEnemyCount = 0;
    private int loopCount = 0; // 何周目か

    // UI表示用
    private WaveSpawnConfig currentWave => currentWaveIndex < waves.Length ? waves[currentWaveIndex] : null;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag.");
            return;
        }

        if (autoStartWaves && waves.Length > 0)
        {
            StartCoroutine(WaveSequence());
        }
    }

    void Update()
    {
        // アクティブなウェーブ中の敵生成
        if (isWaveActive && currentWave != null)
        {
            UpdateWaveSpawning();
        }

        // 現在の敵数カウント更新
        UpdateEnemyCount();
    }

    // ウェーブシーケンス実行
    private IEnumerator WaveSequence()
    {
        while (true)
        {
            while (true)
            {
                for (int i = 0; i < waves.Length; i++)
                {
                    currentWaveIndex = i;
                    WaveSpawnConfig wave = waves[i];

                    // 周回補正を適用
                    ApplyLoopScalingToWave(wave);

                    Debug.Log($"Starting {wave.waveName} Loop:{loopCount + 1}");

                    yield return StartCoroutine(RunWave(wave));

                    if (i < waves.Length - 1 || loopWaves)
                    {
                        yield return new WaitForSeconds(wave.nextWaveDelay);
                    }
                }

                // ループ終了後
                loopCount++;
                currentWaveIndex = 0;
            }
        }
    }

    // 周回補正
    private void ApplyLoopScalingToWave(WaveSpawnConfig wave)
    {
        // 敵レベルと最大数強化
        for (int i = 0; i < wave.enemies.Length; i++)
        {
            var e = wave.enemies[i];

            // レベル補正
            e.minLevel = Mathf.Min(e.minLevel + loopCount * levelIncreasePerLoop, e.maxLevel);
            e.maxLevel = Mathf.Min(e.maxLevel + loopCount * levelIncreasePerLoop, 10); // レベル上限10など

            // レベルが上げられない場合、敵数増加
            if (e.minLevel >= 10)
            {
                wave.maxEnemiesAtOnce += extraEnemiesPerLoop * loopCount;
            }
        }

        // Bossの場合HP補正
        foreach (var e in wave.enemies)
        {
            if (e.prefab.CompareTag("Boss")) // Bossタグつけておく
            {
                BossEnemy boss = e.prefab.GetComponent<BossEnemy>();
                if (boss != null)
                {
                    boss.SetHpMultiplier(Mathf.Pow(bossHpMultiplierPerLoop, loopCount));
                }
            }
        }
    }

    public List<EnemyBase> GetInScreenEnemyes()
    {

        // 画面内の全ての敵を取得
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();

        List<EnemyBase> inScreenEnemyes = new List<EnemyBase>();
        foreach (EnemyBase enemy in allEnemies)
        {
            if (enemy == this || enemy == null || enemy.gameObject == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            // カメラに映っているかチェック
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(enemy.transform.position);
            bool isOnScreen = screenPoint.x > 0 && screenPoint.x < Screen.width &&
                              screenPoint.y > 0 && screenPoint.y < Screen.height &&
                              screenPoint.z > 0;

            if (isOnScreen)
            {
                //画面内の敵リストに追加
                inScreenEnemyes.Add(enemy);
            }
        }

        return inScreenEnemyes;
    }

    // 単一ウェーブの実行
    private IEnumerator RunWave(WaveSpawnConfig wave)
    {
        isWaveActive = true;
        spawnTimer = 0f; // 即座に最初の敵を生成
        waveTimer = wave.waveDuration;

        // ウェーブ継続時間中スポーンを続ける
        while (waveTimer > 0f)
        {
            waveTimer -= Time.deltaTime;
            yield return null;
        }

        isWaveActive = false;

        if (showDebugInfo)
            Debug.Log($"{wave.waveName} spawning ended. Waiting for enemies to clear...");

        // 残り敵が倒されるまで待機（オプション）
        // yield return new WaitUntil(() => currentEnemyCount <= 0);
    }

    // ウェーブ中の敵生成処理
    private void UpdateWaveSpawning()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && currentEnemyCount < currentWave.maxEnemiesAtOnce)
        {
            SpawnRandomEnemy();
            spawnTimer = currentWave.spawnInterval;
        }
    }
    private Vector3 GetSpawnPositionOutsideCamera(float spawnDistance)
    {
        // 画面外のビューポート座標（0?1の外）をランダムに決定
        float x = Random.value < 0.5f ? -0.1f : 1.1f; // 左か右の外側
        float y = Random.Range(-0.1f, 1.1f);          // 上下はランダム
        if (Random.value < 0.5f)
        {
            // 上下の外側にする
            y = Random.value < 0.5f ? -0.1f : 1.1f;
            x = Random.Range(-0.1f, 1.1f);
        }

        // ビューポート座標→ワールド座標に変換
        Vector3 viewPos = new Vector3(x, y, Camera.main.nearClipPlane + spawnDistance);
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewPos);
        worldPos.z = 0; // 2Dの場合zを0に固定

        return worldPos;
    }

    // ランダム敵生成
    private void SpawnRandomEnemy()
    {

        if (currentWave == null || currentWave.enemies.Length == 0) return;

        EnemySpawnData selectedEnemy = GetRandomEnemyFromWave(currentWave);
        if (selectedEnemy?.prefab == null) return;

        int randomLevel = Random.Range(selectedEnemy.minLevel, selectedEnemy.maxLevel + 1);

        // 画面外スポーン位置に変更
        Vector3 spawnPos = GetSpawnPositionOutsideCamera(currentWave.spawnDistance);

        GameObject enemy = Instantiate(selectedEnemy.prefab, spawnPos, Quaternion.identity);
        ApplyEnemyEnhancements(enemy, selectedEnemy, randomLevel);

        currentEnemyCount++;

        if (showDebugInfo)
            Debug.Log($"Spawned {selectedEnemy.prefab.name} Lv.{randomLevel} (Weight: {selectedEnemy.weight}%)");
    }

    // ウェーブから重み付きランダムで敵を選択
    private EnemySpawnData GetRandomEnemyFromWave(WaveSpawnConfig wave)
    {
        float totalWeight = 0f;
        foreach (var enemy in wave.enemies)
        {
            totalWeight += enemy.weight;
        }

        if (totalWeight <= 0f) return null;

        float randomValue = Random.Range(0f, totalWeight);
        float accumulator = 0f;

        foreach (var enemy in wave.enemies)
        {
            accumulator += enemy.weight;
            if (randomValue <= accumulator)
            {
                return enemy;
            }
        }

        return wave.enemies[0]; // フォールバック
    }
    //敵の強化適用
    private void ApplyEnemyEnhancements(GameObject enemy, EnemySpawnData data, int level)
    {
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            // レベル = HP (Lv1 = HP1, Lv2 = HP2, etc.)
            enemyBase.SetMaxHp(level);

            // その他のパラメータ
            enemyBase.SetMoveSpeed(enemyBase.GetMoveSpeed() * data.speedMultiplier);

            // サイズ変更
            enemy.transform.localScale *= data.sizeMultiplier;
        }

        // レベルに応じた視覚的変化（オプション）
        ApplyVisualLevelEffects(enemy, level);
    }

    // 現在の敵数をカウント更新
    private void UpdateEnemyCount()
    {
        // 実際のシーン内の敵数をカウント
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        currentEnemyCount = enemies.Length;
    }

    // 次のウェーブを手動開始
    [ContextMenu("Start Next Wave")]
    public void StartNextWave()
    {
        if (!isWaveActive && currentWaveIndex < waves.Length - 1)
        {
            currentWaveIndex++;
            StartCoroutine(RunWave(waves[currentWaveIndex]));
        }
    }

    // 現在のウェーブを停止
    [ContextMenu("Stop Current Wave")]
    public void StopCurrentWave()
    {
        isWaveActive = false;
        StopAllCoroutines();
    }

    // レベルに応じた視覚効果を適用
    private void ApplyVisualLevelEffects(GameObject enemy, int level)
    {
        SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // レベルに応じて色を変化
            float colorIntensity = 1.0f + (level - 1) * 0.2f; // レベル1=通常色、レベル2=1.2倍明度...
            Color baseColor = spriteRenderer.color;

            // レベルが高いほど赤みを増す
            if (level >= 3)
            {
                spriteRenderer.color = Color.Lerp(baseColor, Color.red, (level - 2) * 0.15f);
            }
            else if (level >= 2)
            {
                spriteRenderer.color = Color.Lerp(baseColor, Color.yellow, 0.3f);
            }
            // レベル1は元の色のまま
        }

        // レベル表示用のテキスト追加（オプション）
        if (level > 1)
        {
            AddLevelDisplay(enemy, level);
        }
    }

    // 敵の上にレベル表示を追加
    private void AddLevelDisplay(GameObject enemy, int level)
    {
        // 子オブジェクトとしてテキスト表示を作成
        GameObject levelDisplay = new GameObject("LevelDisplay");
        levelDisplay.transform.SetParent(enemy.transform);
        levelDisplay.transform.localPosition = Vector3.up * 0.7f;
        levelDisplay.transform.localScale = Vector3.one * 0.5f;

        // TextMeshがない場合はTextMeshProを使用するか、Canvas+Textを使用
        // 簡易版として3DTextを使用
        TextMesh textMesh = levelDisplay.AddComponent<TextMesh>();
        textMesh.text = $"Lv.{level}";
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        // カメラの方を向くように
        levelDisplay.transform.rotation = Camera.main.transform.rotation;
    }
    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label($"=== Wave Spawner Debug ===");
        GUILayout.Label($"Current Wave: {(currentWave != null ? currentWave.waveName : "None")}");
        GUILayout.Label($"Wave Index: {currentWaveIndex + 1}/{waves.Length}");
        GUILayout.Label($"Active: {isWaveActive}");
        GUILayout.Label($"Enemies Alive: {currentEnemyCount}");

        if (isWaveActive && currentWave != null)
        {
            GUILayout.Label($"Wave Time Left: {waveTimer:F1}s");
            GUILayout.Label($"Next Spawn: {spawnTimer:F1}s");
        }

        if (GUILayout.Button("Next Wave"))
        {
            StartNextWave();
        }

        if (GUILayout.Button("Stop Wave"))
        {
            StopCurrentWave();
        }

        GUILayout.EndArea();
    }

}
