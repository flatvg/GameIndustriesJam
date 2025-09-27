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
        public float weight; // �o���d��(%)
    }

    [Header("�G���X�g")]
    [SerializeField] private EnemySpawnData[] enemies;
    [Header("�X�|�[���ݒ�")]
    [SerializeField,Header("�����Ԋu")] private float spawnInterval = 2f;  // �����Ԋu�i�b�j
    [SerializeField, Header("�v���C���[����̋���")] private float spawnDistance = 10f;    // �v���C���[����ǂ̂��炢�O���ɏo����

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
        // �d�ݕt�������_����prefab����
        GameObject enemyPrefab = GetRandomEnemyPrefab();

        // �v���C���[���̓����_���Ȉʒu
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
        return enemies[0].prefab; // �O�̂���
    }
}
