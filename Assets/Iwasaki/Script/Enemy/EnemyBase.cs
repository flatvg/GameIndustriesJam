using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using Color = UnityEngine.Color;

public class EnemyBase : MonoBehaviour
{

    [SerializeField, Header("基本パラメータ")]
    protected int hp = 1;
    [SerializeField]
    protected int maxHp = 1;
    [SerializeField,Header("移動速度")]
    protected float moveSpeed = 2.0f;

    [SerializeField, Header("無敵時間設定")]
    protected float invincibilityDuration = 1.0f; // 生成時の無敵時間
    [SerializeField]
    protected float damageInvincibilityDuration = 0.2f; // ダメージ後の無敵時間

    [SerializeField, Header("視覚効果")]
    protected Color invincibleColor = Color.red;
    protected Color normalColor = Color.white;

    // 無敵状態管理
    protected bool isInvincible = false;
    protected float invincibilityTimer = 0f;
    protected bool isSpawnInvincible = true; // 生成時無敵フラグ
    protected bool isDead = false;

    // 一時的な速度バフ
    protected float tempSpeedMultiplier = 1f;
    protected float tempSpeedBuffTimer = 0f;

    protected Transform player;
    protected Rigidbody2D playerrb;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    public static readonly Color[] enemyColors =
    {
        Color.black,
        Color.blue,
        Color.green,
        Color.magenta,
        Color.yellow,
        Color.red,
    };

    public float GetMaxHp()
    {
        return maxHp;

    }
    public void SetMaxHp(int HP)
    {
        maxHp = HP;
    }
    public float GetMoveSpeed()
    {
        return moveSpeed;

    }
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Player参照の取得
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            //プレイヤーコンポーネント取得
            playerrb = playerObj.GetComponent<Rigidbody2D>();
            player = GameObject.FindWithTag("Player").transform;

        }

        // 自身のコンポーネント取得
        rb = GetComponent<Rigidbody2D>();
        rb.drag = 2f;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初期設定
        maxHp = hp;
        if (spriteRenderer != null)
        {
            if (maxHp <= enemyColors.Length - 1)
            {
                normalColor = enemyColors[maxHp];
                spriteRenderer.color = normalColor;
            }
            else
            {
                normalColor = spriteRenderer.color;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        OnInit();

        StartInvincibility(invincibilityDuration);
    }
    // 各敵ごとに実装
    protected virtual void OnInit()
    {

    }

    // 各敵ごとに実装
    protected virtual void Move()
    {

    }

    // Update is called once per frame
    protected void Update()
    {

        // 無敵時間の更新
        UpdateInvincibility();

        Move();
    }

    //無敵時間の更新
    protected virtual void UpdateInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            // 点滅効果
            if (spriteRenderer != null && !isSpawnInvincible)
            {
                float alpha = Mathf.Sin(Time.time * 20f) * 0.5f + 0.5f;
                Color color = Color.Lerp(normalColor, invincibleColor, alpha * 0.5f);
                spriteRenderer.color = color;
            }

            if (invincibilityTimer <= 0f)
            {
                EndInvincibility();
            }
        }
    }
    /// 無敵時間開始
    protected virtual void StartInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
    }

    /// 無敵時間終了
    protected virtual void EndInvincibility()
    {
        isInvincible = false;
        isSpawnInvincible = false; // 生成時無敵フラグもリセット
        invincibilityTimer = 0f;

        // 色を元に戻す
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    // 一時的な速度バフの更新
    protected virtual void UpdateTempSpeedBuff()
    {
        if (tempSpeedBuffTimer > 0f)
        {
            tempSpeedBuffTimer -= Time.deltaTime;

            if (tempSpeedBuffTimer <= 0f)
            {
                tempSpeedMultiplier = 1f;
            }
        }
    }

    // 一時的な速度バフを適用
    public virtual void ApplyTemporarySpeedBuff(float multiplier, float duration)
    {
        tempSpeedMultiplier = multiplier;
        tempSpeedBuffTimer = duration;
    }

    // 現在の実効移動速度を取得
    protected float GetEffectiveMoveSpeed()
    {
        return moveSpeed * tempSpeedMultiplier;
    }

    public virtual bool TakeDamage(int damage, Vector2 knockback)
    {
        // 無敵時間中はダメージを受けない
        if (isInvincible) return false;
        hp -= damage;
        if (rb != null)
        {
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
        else
        {
            transform.position += (Vector3)knockback;
        }
        // ダメージ後の無敵時間
        if (hp > 0 && damage > 0)
        {
            StartInvincibility(damageInvincibilityDuration);
            return false;
        }
        else if(hp <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    protected virtual void Die()
    {
        isDead = true;
        // 死亡エフェクト等の処理をここに追加可能
        OnDeath();

        Destroy(gameObject);
    }

    protected virtual void OnDeath()
    {
        // 個別の敵でオーバーライドして独自の処理を追加
        // 例：爆発エフェクト、アイテムドロップ、分裂等
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // プレイヤーにダメージを与える
            //PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            //if (playerController != null)
            //{
            //    Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            //    playerController.TakeDamage(1, knockbackDir * 2f);
            //}
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // プレイヤーにダメージを与える
            //PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            //if (playerController != null)
            //{
            //    Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            //    playerController.TakeDamage(1, knockbackDir * 2f);
            //
        }
    }
    protected virtual void OnDrawGizmosSelected()
    {
        // HP表示用のバー
        Gizmos.color = Color.red;
        Vector3 healthBarPos = transform.position + Vector3.up * 1.5f;
        float healthPercent = maxHp > 0 ? hp / maxHp : 0;
        Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f,
            healthBarPos - Vector3.right * 0.5f + Vector3.right * healthPercent);

        // 移動方向表示
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Vector3 direction = (player.position - transform.position).normalized;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
    }
}
