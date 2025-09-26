using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionEnemy : EnemyBase
{
    [SerializeField]
    private float separationRadius = 1f; // 他の敵との距離を保つ
    [SerializeField]
    private float leaderFollowRadius = 1f; // リーダーを追従する距離

    [SerializeField]
    private float size = 0.5f; // 通常の色

    private SwarmLeader leader = null;
    private float baseMoveSpeed; // 基本移動速度
    private float currentSpeedMultiplier = 1f; // 現在の速度倍率
    private float buffTimer = 0f; // バフ残り時間

    protected override void OnInit()
    {
        // 群れ敵の設定
        hp = 1; // 体力低め
        baseMoveSpeed = moveSpeed;
        moveSpeed = Random.Range(baseMoveSpeed * 0.8f, baseMoveSpeed * 1.2f); // 速度にばらつき
        transform.localScale = Vector3.one * size; // サイズ小さめ
    }

    protected override void Move()
    {
        if (player != null && !isDead)
        {
            Vector2 moveDirection = Vector2.zero;

            // リーダーが生きている場合のリーダー追従行動
            if (leader != null && leader.gameObject != null)
            {
                float distanceToLeader = Vector2.Distance(transform.position, leader.transform.position);

                // リーダーから離れすぎている場合はリーダーに近づく
                if (distanceToLeader > leaderFollowRadius)
                {
                    Vector2 toLeader = (leader.transform.position - transform.position).normalized;
                    moveDirection += toLeader * 0.7f; // リーダー追従力
                }
            }

            // プレイヤーに向かう力
            Vector2 toPlayer = (player.position - transform.position).normalized;
            moveDirection += toPlayer;

            // 他の敵から離れる力（分離行動）
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, separationRadius);
            foreach (var enemy in nearbyEnemies)
            {
                if (enemy.gameObject != gameObject && enemy.CompareTag("Enemy"))
                {
                    Vector2 awayFromEnemy = (transform.position - enemy.transform.position).normalized;
                    moveDirection += awayFromEnemy * 0.5f;
                }
            }

            // バフ効果を適用した移動速度で移動
            float currentMoveSpeed = moveSpeed * currentSpeedMultiplier;
            transform.Translate(moveDirection.normalized * currentMoveSpeed * Time.deltaTime);

            // バフ時間の更新
            UpdateBuff();
        }
    }

    // リーダーを設定
    public void SetLeader(SwarmLeader newLeader)
    {
        leader = newLeader;
    }

    // リーダーからのバフを適用
    public void ApplyLeaderBuff(float multiplier)
    {
        currentSpeedMultiplier = multiplier;
        buffTimer = 1f; // 1秒間バフ効果持続

        // バフ中の視覚効果（少し明るく）
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(normalColor, Color.white, 0.3f);
        }
    }

    // バフ効果の更新
    private void UpdateBuff()
    {
        if (buffTimer > 0f)
        {
            buffTimer -= Time.deltaTime;

            if (buffTimer <= 0f)
            {
                // バフ終了
                currentSpeedMultiplier = 1f;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = normalColor;
                }
            }
        }
    }

    // リーダーが死んだ時の処理
    public void OnLeaderDeath()
    {
        leader = null;
        currentSpeedMultiplier = 1f; // バフ解除
        buffTimer = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    // 部下が死んだ時にリーダーに通知
    protected override void OnDeath()
    {
        if (leader != null)
        {
            leader.RemoveSubordinate(this);
        }

        base.OnDeath();
    }
}

