using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnaer : MonoBehaviour
{

    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject prefab;
        [Range(0f, 100f)]
        public float weight; // 出現重み(%)
    }

    [Header("敵リスト")]
    [SerializeField] private EnemySpawnData[] enemies;
    [Header("スポーン設定")]
    [SerializeField,Header("生成間隔")] private float spawnInterval = 2f;  // 生成間隔（秒）
    [SerializeField, Header("プレイヤーからの距離")] private float spawnDistance = 10f;    // プレイヤーからどのくらい外側に出すか

    private float spawnTimer;
    protected Transform player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        // 重み付きランダムでprefab決定
        GameObject enemyPrefab = GetRandomEnemyPrefab();

        // プレイヤー周囲ランダムな位置
        Vector2 randDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = player.position + (Vector3)(randDir * spawnDistance);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    private GameObject GetRandomEnemyPrefab()
    {
        float totalWeight = 0f;
        foreach (var e in enemies) totalWeight += e.weight;

        float r = Random.Range(0f, totalWeight);
        float accum = 0f;
        foreach (var e in enemies)
        {
            accum += e.weight;
            if (r <= accum)
                return e.prefab;
        }
        return enemies[0].prefab; // 念のため
    }
}
