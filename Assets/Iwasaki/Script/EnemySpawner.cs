using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnaer : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        [Header("��{�ݒ�")]
        public GameObject prefab;
        [Range(0f, 100f)] public float weight = 10f; // ���Ώd��

        [Header("���x���ݒ�")]
        [Tooltip("���̓G�̊�{�ŏ�Lv")]
        public int baseMinLevel = 1;
        [Tooltip("���̓G�̊�{�ő�Lv")]
        public int baseMaxLevel = 1;
        [Tooltip("���񂲂Ƃɑ�����ŏ�Lv�i��: 1�Ȃ� loopIndex=1���� +1�j")]
        public int minLevelIncreasePerLoop = 0;
        [Tooltip("���񂲂Ƃɑ�����ő�Lv")]
        public int maxLevelIncreasePerLoop = 0;
        [Tooltip("�X�|�[�����̊m���t�B���^�[�i0~1�j�B�d�݂őI�΂�Ă����̊m���Ŏ��ۂɃX�|�[������j")]
        [Range(0f, 1f)]
        public float spawnChance = 1f;

        [Header("���̑��̋����ݒ�")]
        public float speedMultiplier = 1.0f;
        public float sizeMultiplier = 1.0f;
    }

    [System.Serializable]
    public class WaveSpawnConfig
    {
        [Header("�E�F�[�u���")]
        public string waveName = "Wave";
        public int waveNumber = 1;

        [Header("�X�|�[���ݒ�")]
        public float spawnInterval = 2f;        // �����Ԋu�i�b�j
        public float spawnDistance = 10f;       // �v���C���[����̋��� (Viewport�O�ɏo����������)
        public int maxEnemiesAtOnce = 5;        // �����ő�G��
        [Header("�E�F�[�u�p�����ԁi�b�j")]
        public float waveDuration = 30f;

        [Header("�G�\��")]
        public EnemySpawnData[] enemies;        // ���̃E�F�[�u�̓G���X�g

        [Header("���̃E�F�[�u�܂ł̒x��")]
        public float nextWaveDelay = 5f;

        [Header("Boss�ݒ� (�{�X�E�F�[�u�Ɏg��)")]
        public bool bossWave = false;
        public GameObject bossPrefab;           // bossWave=true �̂Ƃ��ɃX�|�[������ Boss Prefab
        [Tooltip("bossWave �̎��A�{�X�� SpawnPhase �̂ݎG�����X�|�[������")]
        public bool spawnOnlyDuringBossSpawnPhase = true;
    }

    [Header("�E�F�[�u�ݒ�")]
    [SerializeField] private WaveSpawnConfig[] waves;

    [Header("�S�̐ݒ�")]
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private bool loopWaves = true; // �Ō��Wave�����[�v
    [SerializeField, Header("���񂲂Ƃ̍ŏ����x���㏸�l(���g�p�Ȃ�0)")]
    private int levelIncreasePerLoop = 0; // �S�̃x�[�X�␳�i�ʂ̑����ݒ�ƍ��킹�Ďg���j
    [SerializeField, Header("Boss HP �����{���i���[�v���Ɓj")]
    private float bossHpMultiplierPerLoop = 1.5f;
    [SerializeField, Header("���x��������ɓG������ (�ǉ��G/����)")]
    private int extraEnemiesPerLoop = 2;

    [Header("�f�o�b�O")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool showDebugInfo = true;

    // internal
    private Transform player;
    private bool isWaveActive = false;
    private float spawnTimer = 0f;
    private float waveTimer = 0f;
    private int loopCount = 0; // 0 �͍ŏ��̃��[�v�i�\���p�� loopIndex = loopCount+1�j
    private int currentEnemyCount = 0;

    // boss �Ǘ�
    private BossEnemy currentBoss = null;
    private bool bossAllowsSpawns = false; // �{�X�� SpawnPhase �����Ă邩

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

                // ����␳�𔽉f�iUI�ȂǂŎQ�Ƃ���Ȃ炱���Łj
                ApplyLoopScalingToWave(wave, loopCount);

                if (showDebugInfo) Debug.Log($"Start Wave {wave.waveName} Loop:{loopCount + 1}");

                yield return StartCoroutine(RunWave(wave));

                // ���E�F�[�u�ւ̑҂�
                if (i < waves.Length - 1 || loopWaves)
                    yield return new WaitForSeconds(wave.nextWaveDelay);
            }

            // �SWave�I��� -> ����++
            loopCount++;
            if (!loopWaves) break; // ���[�v���Ȃ��Ȃ甲����
        }
    }

    private void ApplyLoopScalingToWave(WaveSpawnConfig wave, int loop)
    {
        // wave���̂� Scriptable �ł͂Ȃ��̂ŔO�̂��ߒ��ڕς���̂͒��ӂ����A
        // inspector�ݒ���㏑�����Ă悢�Ȃ玟�̂悤�ɕ␳�ł��܂��i�y���ȕ␳�̂݁j�B
        // ��{�� Spawn ���Ɍv�Z�������������Ă���̂ł����ł͋ɗ͐G��Ȃ��B
    }

    private IEnumerator RunWave(WaveSpawnConfig wave)
    {
        isWaveActive = true;
        spawnTimer = 0f; // ���� spawn
        waveTimer = wave.waveDuration;
        currentEnemyCount = 0;

        // �{�X�E�F�[�u�Ȃ� boss �𐶐����āA�{�X�� SpawnPhase �ɍ��킹�ĎG���𐶐����郍�W�b�N�ɂ���
        if (wave.bossWave && wave.bossPrefab != null)
        {
            // spawn boss at top center outside screen
            Vector3 bossSpawn = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.2f, Camera.main.nearClipPlane + wave.spawnDistance));
            bossSpawn.z = 0f;
            var bossGO = Instantiate(wave.bossPrefab, bossSpawn, Quaternion.identity);
            currentBoss = bossGO.GetComponent<BossEnemy>();
            if (currentBoss != null)
            {
                // loopIndex �� boss �ɒʒm�i�{�X���ň����j
                currentBoss.SetLoopIndex(loopCount + 1);
                // subscribe to spawn-phase events
                currentBoss.OnSpawnPhaseChanged += OnBossSpawnPhaseChanged;
            }
            // wait until boss appears on screen (or some time) -> here just wait a short time then enter boss loop
            yield return new WaitForSeconds(0.5f);
        }

        // �E�F�[�u�̎������ԁBbossWave�̎��́uboss�������Ă���ԁv��u�w��duration�v�Ȃǎ��R�ɕς�����悤�ɂ���B
        if (!wave.bossWave)
        {
            // ��{�X�E�F�[�u�F�w�莞�Ԃ��� spawn interval �Ǘ��ŏo��
            while (waveTimer > 0f)
            {
                waveTimer -= Time.deltaTime;
                UpdateWaveSpawning(wave);
                yield return null;
            }
        }
        else
        {
            // �{�X�E�F�[�u�F�{�X�����݂���ԁi�܂��� waveDuration �o�߁j�G�����o�����A�ݒ�ɂ�� spawnOnlyDuringBossSpawnPhase ���g��
            while ((currentBoss != null && !currentBoss.GetIsDead()) || waveTimer > 0f)
            {
                // waveTimer�����炵�Ă����i�I�v�V�����j
                waveTimer -= Time.deltaTime;

                // spawn only when boss allows spawns if configured
                UpdateWaveSpawning(wave);

                // boss �� null �����񂾂� break ����悤�ɂ��Ă���
                if (currentBoss == null || currentBoss.GetIsDead())
                    break;

                yield return null;
            }

            // �{�X�I�������F����
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

    // Boss ���� SpawnPhase �C�x���g�������Ƃ�
    private void OnBossSpawnPhaseChanged(bool spawnPhaseOn)
    {
        bossAllowsSpawns = spawnPhaseOn;
    }

    // ���t���[��/�E�F�[�u���ɌĂ� spawn �X�V
    private void UpdateWaveSpawning(WaveSpawnConfig wave)
    {
        if (!isWaveActive || wave == null) return;

        // �{�X�E�F�[�u�� spawnOnlyDuringBossSpawnPhase �� true �̏ꍇ�� bossAllowsSpawns �����Đ���
        if (wave.bossWave && wave.spawnOnlyDuringBossSpawnPhase && !bossAllowsSpawns)
            return;

        spawnTimer -= Time.deltaTime;

        // ���݂̃V�[�����̓G�����J�E���g�i�y�߂̏����j
        UpdateEnemyCount();

        if (spawnTimer <= 0f && currentEnemyCount < wave.maxEnemiesAtOnce)
        {
            SpawnRandomEnemyFromWave(wave);
            spawnTimer = wave.spawnInterval;
        }
    }

    private void UpdateEnemyCount()
    {
        // �y�ʉ��FFindObjectsOfType���g���̂͏d�߂����ȕցB�K�v�Ȃ�Pooling/�J�E���g�Œu���B
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        currentEnemyCount = enemies.Length;
    }

    // ��ʊO�ɃX�|�[��������W�擾
    private Vector3 GetSpawnPositionOutsideCamera(float spawnDistance)
    {
        // Viewport �̊O���i��/�E/��/���̂����ꂩ�j�Ƀ����_���ɏo��
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

    // �E�F�[�u�̓G����d�݁E�m�����g���� 1�̑I�ԁi�I�΂�Ȃ����Ƃ�����ꍇ�� null ��Ԃ��j
    private EnemySpawnData GetRandomEnemyFromWave(WaveSpawnConfig wave)
    {
        if (wave == null || wave.enemies == null || wave.enemies.Length == 0) return null;

        // �܂��d�ݍ��v���v�Z
        float totalWeight = 0f;
        foreach (var e in wave.enemies) totalWeight += Mathf.Max(0f, e.weight);

        if (totalWeight <= 0f) return null;

        // �d�݂őI���i���[���b�g�j�����A�I�΂ꂽ�� spawnChance �ɂ��t�B���^������
        for (int attempt = 0; attempt < 6; attempt++) // �ő厎�s�񐔂����߂Ė������[�v���
        {
            float rv = Random.Range(0f, totalWeight);
            float acc = 0f;
            foreach (var e in wave.enemies)
            {
                acc += Mathf.Max(0f, e.weight);
                if (rv <= acc)
                {
                    // spawnChance ����
                    if (Random.value <= e.spawnChance)
                        return e;
                    else
                        break; // ���s���s -> �Ď��s
                }
            }
        }

        // �t�H�[���o�b�N�F�ŏ��� spawnChance �𖞂����G��Ԃ�
        foreach (var e in wave.enemies)
            if (Random.value <= e.spawnChance) return e;

        return null;
    }

    // ���ۂɓG�𐶐�����
    private void SpawnRandomEnemyFromWave(WaveSpawnConfig wave)
    {
        var selected = GetRandomEnemyFromWave(wave);
        if (selected == null || selected.prefab == null) return;

        // ���x���v�Z�ibase + per-loop���� + global�����j
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

        // ���o�I�G�t�F�N�g�⃌�x���\��
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

    // ���J���[�e�B���e�B
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
