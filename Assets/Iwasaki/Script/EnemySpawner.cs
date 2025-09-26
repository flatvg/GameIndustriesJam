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
        [Range(0f, 100f)]
        public float weight = 10f; // �o���d��(%)

        [Header("���x���ݒ�")]
        [Range(1, 10)]
        public int minLevel = 1;        // �ŏ����x��
        [Range(1, 10)]
        public int maxLevel = 1;        // �ő僌�x��

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
        public float spawnDistance = 10f;       // �v���C���[����̋���
        public int maxEnemiesAtOnce = 5;        // �����ő�G��
        [Header("�E�F�[�u�p������")] public float waveDuration = 30f; // �i�b�j

        [Header("�G�\��")]
        public EnemySpawnData[] enemies;        // ���̃E�F�[�u�̓G���X�g

        [Header("���̃E�F�[�u")]
        public float nextWaveDelay = 5f;        // ���E�F�[�u�܂ł̊Ԋu
    }

    [Header("�E�F�[�u�ݒ�")]
    [SerializeField] private WaveSpawnConfig[] waves;

    [Header("�S�̐ݒ�")]
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private bool loopWaves = false;       // �Ō�̃E�F�[�u�����[�v���邩

    [Header("�f�o�b�O")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool showDebugInfo = true;

    // EnemySpawner �N���X�ɒǉ�
    [SerializeField,Header("���񂲂Ƃ̍ŏ����x���㏸�l")] private int levelIncreasePerLoop = 1;
    [SerializeField,Header("BossHP�����{��")] private float bossHpMultiplierPerLoop = 1.5f;
    [SerializeField,Header("���x��������ɓG������")] private int extraEnemiesPerLoop = 2;

    private Transform player;
    private float spawnTimer;
    private float waveTimer;
    private bool isWaveActive = false;
    private int currentEnemyCount = 0;
    private int loopCount = 0; // �����ڂ�

    // UI�\���p
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
        // �A�N�e�B�u�ȃE�F�[�u���̓G����
        if (isWaveActive && currentWave != null)
        {
            UpdateWaveSpawning();
        }

        // ���݂̓G���J�E���g�X�V
        UpdateEnemyCount();
    }

    // �E�F�[�u�V�[�P���X���s
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

                    // ����␳��K�p
                    ApplyLoopScalingToWave(wave);

                    Debug.Log($"Starting {wave.waveName} Loop:{loopCount + 1}");

                    yield return StartCoroutine(RunWave(wave));

                    if (i < waves.Length - 1 || loopWaves)
                    {
                        yield return new WaitForSeconds(wave.nextWaveDelay);
                    }
                }

                // ���[�v�I����
                loopCount++;
                currentWaveIndex = 0;
            }
        }
    }

    // ����␳
    private void ApplyLoopScalingToWave(WaveSpawnConfig wave)
    {
        // �G���x���ƍő吔����
        for (int i = 0; i < wave.enemies.Length; i++)
        {
            var e = wave.enemies[i];

            // ���x���␳
            e.minLevel = Mathf.Min(e.minLevel + loopCount * levelIncreasePerLoop, e.maxLevel);
            e.maxLevel = Mathf.Min(e.maxLevel + loopCount * levelIncreasePerLoop, 10); // ���x�����10�Ȃ�

            // ���x�����グ���Ȃ��ꍇ�A�G������
            if (e.minLevel >= 10)
            {
                wave.maxEnemiesAtOnce += extraEnemiesPerLoop * loopCount;
            }
        }

        // Boss�̏ꍇHP�␳
        foreach (var e in wave.enemies)
        {
            if (e.prefab.CompareTag("Boss")) // Boss�^�O���Ă���
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

        // ��ʓ��̑S�Ă̓G���擾
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();

        List<EnemyBase> inScreenEnemyes = new List<EnemyBase>();
        foreach (EnemyBase enemy in allEnemies)
        {
            if (enemy == this || enemy == null || enemy.gameObject == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            // �J�����ɉf���Ă��邩�`�F�b�N
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(enemy.transform.position);
            bool isOnScreen = screenPoint.x > 0 && screenPoint.x < Screen.width &&
                              screenPoint.y > 0 && screenPoint.y < Screen.height &&
                              screenPoint.z > 0;

            if (isOnScreen)
            {
                //��ʓ��̓G���X�g�ɒǉ�
                inScreenEnemyes.Add(enemy);
            }
        }

        return inScreenEnemyes;
    }

    // �P��E�F�[�u�̎��s
    private IEnumerator RunWave(WaveSpawnConfig wave)
    {
        isWaveActive = true;
        spawnTimer = 0f; // �����ɍŏ��̓G�𐶐�
        waveTimer = wave.waveDuration;

        // �E�F�[�u�p�����Ԓ��X�|�[���𑱂���
        while (waveTimer > 0f)
        {
            waveTimer -= Time.deltaTime;
            yield return null;
        }

        isWaveActive = false;

        if (showDebugInfo)
            Debug.Log($"{wave.waveName} spawning ended. Waiting for enemies to clear...");

        // �c��G���|�����܂őҋ@�i�I�v�V�����j
        // yield return new WaitUntil(() => currentEnemyCount <= 0);
    }

    // �E�F�[�u���̓G��������
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
        // ��ʊO�̃r���[�|�[�g���W�i0?1�̊O�j�������_���Ɍ���
        float x = Random.value < 0.5f ? -0.1f : 1.1f; // �����E�̊O��
        float y = Random.Range(-0.1f, 1.1f);          // �㉺�̓����_��
        if (Random.value < 0.5f)
        {
            // �㉺�̊O���ɂ���
            y = Random.value < 0.5f ? -0.1f : 1.1f;
            x = Random.Range(-0.1f, 1.1f);
        }

        // �r���[�|�[�g���W�����[���h���W�ɕϊ�
        Vector3 viewPos = new Vector3(x, y, Camera.main.nearClipPlane + spawnDistance);
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewPos);
        worldPos.z = 0; // 2D�̏ꍇz��0�ɌŒ�

        return worldPos;
    }

    // �����_���G����
    private void SpawnRandomEnemy()
    {

        if (currentWave == null || currentWave.enemies.Length == 0) return;

        EnemySpawnData selectedEnemy = GetRandomEnemyFromWave(currentWave);
        if (selectedEnemy?.prefab == null) return;

        int randomLevel = Random.Range(selectedEnemy.minLevel, selectedEnemy.maxLevel + 1);

        // ��ʊO�X�|�[���ʒu�ɕύX
        Vector3 spawnPos = GetSpawnPositionOutsideCamera(currentWave.spawnDistance);

        GameObject enemy = Instantiate(selectedEnemy.prefab, spawnPos, Quaternion.identity);
        ApplyEnemyEnhancements(enemy, selectedEnemy, randomLevel);

        currentEnemyCount++;

        if (showDebugInfo)
            Debug.Log($"Spawned {selectedEnemy.prefab.name} Lv.{randomLevel} (Weight: {selectedEnemy.weight}%)");
    }

    // �E�F�[�u����d�ݕt�������_���œG��I��
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

        return wave.enemies[0]; // �t�H�[���o�b�N
    }
    //�G�̋����K�p
    private void ApplyEnemyEnhancements(GameObject enemy, EnemySpawnData data, int level)
    {
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            // ���x�� = HP (Lv1 = HP1, Lv2 = HP2, etc.)
            enemyBase.SetMaxHp(level);

            // ���̑��̃p�����[�^
            enemyBase.SetMoveSpeed(enemyBase.GetMoveSpeed() * data.speedMultiplier);

            // �T�C�Y�ύX
            enemy.transform.localScale *= data.sizeMultiplier;
        }

        // ���x���ɉ��������o�I�ω��i�I�v�V�����j
        ApplyVisualLevelEffects(enemy, level);
    }

    // ���݂̓G�����J�E���g�X�V
    private void UpdateEnemyCount()
    {
        // ���ۂ̃V�[�����̓G�����J�E���g
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        currentEnemyCount = enemies.Length;
    }

    // ���̃E�F�[�u���蓮�J�n
    [ContextMenu("Start Next Wave")]
    public void StartNextWave()
    {
        if (!isWaveActive && currentWaveIndex < waves.Length - 1)
        {
            currentWaveIndex++;
            StartCoroutine(RunWave(waves[currentWaveIndex]));
        }
    }

    // ���݂̃E�F�[�u���~
    [ContextMenu("Stop Current Wave")]
    public void StopCurrentWave()
    {
        isWaveActive = false;
        StopAllCoroutines();
    }

    // ���x���ɉ��������o���ʂ�K�p
    private void ApplyVisualLevelEffects(GameObject enemy, int level)
    {
        SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // ���x���ɉ����ĐF��ω�
            float colorIntensity = 1.0f + (level - 1) * 0.2f; // ���x��1=�ʏ�F�A���x��2=1.2�{���x...
            Color baseColor = spriteRenderer.color;

            // ���x���������قǐԂ݂𑝂�
            if (level >= 3)
            {
                spriteRenderer.color = Color.Lerp(baseColor, Color.red, (level - 2) * 0.15f);
            }
            else if (level >= 2)
            {
                spriteRenderer.color = Color.Lerp(baseColor, Color.yellow, 0.3f);
            }
            // ���x��1�͌��̐F�̂܂�
        }

        // ���x���\���p�̃e�L�X�g�ǉ��i�I�v�V�����j
        if (level > 1)
        {
            AddLevelDisplay(enemy, level);
        }
    }

    // �G�̏�Ƀ��x���\����ǉ�
    private void AddLevelDisplay(GameObject enemy, int level)
    {
        // �q�I�u�W�F�N�g�Ƃ��ăe�L�X�g�\�����쐬
        GameObject levelDisplay = new GameObject("LevelDisplay");
        levelDisplay.transform.SetParent(enemy.transform);
        levelDisplay.transform.localPosition = Vector3.up * 0.7f;
        levelDisplay.transform.localScale = Vector3.one * 0.5f;

        // TextMesh���Ȃ��ꍇ��TextMeshPro���g�p���邩�ACanvas+Text���g�p
        // �ȈՔłƂ���3DText���g�p
        TextMesh textMesh = levelDisplay.AddComponent<TextMesh>();
        textMesh.text = $"Lv.{level}";
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        // �J�����̕��������悤��
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
