using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 群れのリーダー - 色違いで画面内の敵に速度バフを付与
public class SwarmLeader : EnemyBase
{
    [SerializeField]
    private float buffRadius = 10f; // バフ効果範囲
    [SerializeField]
    private float speedBuffMultiplier = 1.3f; // 速度バフ倍率
    [SerializeField]
    private float buffUpdateInterval = 0.5f; // バフ更新間隔
    [SerializeField]
    private Color leaderColor = Color.yellow; // リーダーの色

    private List<MinionEnemy> subordinates = new List<MinionEnemy>();
    private float buffTimer = 0f;

    protected override void OnInit()
    {
        // リーダーの見た目設定
        if (spriteRenderer != null)
        {
            normalColor = leaderColor;
            spriteRenderer.color = leaderColor;
        }

        // サイズを少し大きく
        transform.localScale = Vector3.one * 1.2f;
    }

    protected override void Move()
    {
        if (player != null && !isDead)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            // 定期的にバフを更新
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                ApplySpeedBuffToNearbyEnemies();
                buffTimer = buffUpdateInterval;
            }
        }
    }

    // 部下を追加
    public void AddSubordinate(MinionEnemy subordinate)
    {
        if (subordinate != null)
        {
            subordinates.Add(subordinate);
            subordinate.SetLeader(this);
        }
    }

    // 部下を削除
    public void RemoveSubordinate(MinionEnemy subordinate)
    {
        if (subordinates.Contains(subordinate))
        {
            subordinates.Remove(subordinate);
        }
    }

    // 画面内の敵に速度バフを付与
    private void ApplySpeedBuffToNearbyEnemies()
    {
        // 画面内の全ての敵を取得
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();

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
                // バフ効果を適用
                MinionEnemy swarmEnemy = enemy as MinionEnemy;
                if (swarmEnemy != null)
                {
                    swarmEnemy.ApplyLeaderBuff(speedBuffMultiplier);
                }
                else
                {
                    // 他の敵タイプにも軽いバフを適用
                    enemy.ApplyTemporarySpeedBuff(speedBuffMultiplier * 0.7f, buffUpdateInterval * 1.5f);
                }
            }
        }
    }

    // リーダーが死んだ時の処理
    protected override void OnDeath()
    {
        // 生き残った部下のバフを解除
        foreach (MinionEnemy subordinate in subordinates)
        {
            if (subordinate != null)
            {
                subordinate.OnLeaderDeath();
            }
        }

        base.OnDeath();
    }

    // デバッグ用：バフ範囲表示
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // バフ範囲を表示
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCircle(transform.position, buffRadius);
    }
}
