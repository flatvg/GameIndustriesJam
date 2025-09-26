using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("召喚設定")]
    [SerializeField,Header("召喚する雑魚")] private GameObject minionPrefab; 
    [SerializeField,Header("生成頻度（秒")] private float spawnInterval = 1.5f; 
    [SerializeField,Header("1回あたりの召喚数")] private int spawnCount = 3;  
    [SerializeField,Header("召喚位置の半径")] private float spawnRadius = 3f;

    [Header("移動設定")]
    [SerializeField,Header("移動ポイント")] private Vector2[] movePoints;

    private float spawnTimer;
    private int currentPointIndex = 0;
    private bool isStopped = false;

    private float hpMultiplier = 1f;
    public void SetHpMultiplier(float multiplier)
    {
        hpMultiplier = multiplier;
        SetMaxHp((int)(GetMaxHp() * hpMultiplier));
    }

    protected override void OnInit()
    {
        spawnTimer = spawnInterval;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 1000f;
        rb.drag = 500f; // 慣性を抑える
        rb.angularDrag = 500f; // 回転を抑える
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

    }

    protected override void Move()
    {
        // 移動（まだ止まっていない時だけ）
        if (!isStopped)
        {
            MoveAlongPoints();
        }

        // 停止後は召喚処理を実行
        if (isStopped)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnMinions();
                spawnTimer = spawnInterval;
            }
        }


    }

    private void MoveAlongPoints()
    {
        if (movePoints.Length == 0) return;

        Vector2 targetPoint = movePoints[currentPointIndex];
        Vector3 direction = ((Vector3)targetPoint - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // 目標地点に近づいたら次のポイントへ
        if (Vector3.Distance(transform.position, (Vector3)targetPoint) < 0.1f)
        {
            currentPointIndex++;

            // 最終ポイントに着いたら停止
            if (currentPointIndex >= movePoints.Length)
            {
                currentPointIndex = movePoints.Length - 1;
                isStopped = true;
            }
        }
    }

    private void SpawnMinions()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            // ビューポート座標（0～1）の範囲内からランダムに決定
            float vx = Random.Range(0.1f, 0.9f); // 端を避ける場合は0.1～0.9
            float vy = Random.Range(0.1f, 0.9f);

            // カメラ距離を指定
            Vector3 spawnPos = cam.ViewportToWorldPoint(
                new Vector3(vx, vy, Mathf.Abs(cam.transform.position.z - transform.position.z))
            );
            spawnPos.z = 0f;

            Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        }
    }
}