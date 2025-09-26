using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using Color = UnityEngine.Color;

public class EnemyBase : MonoBehaviour
{

    [SerializeField, Header("��{�p�����[�^")]
    protected int hp = 1;
    [SerializeField]
    protected int maxHp = 1;
    [SerializeField,Header("�ړ����x")]
    protected float moveSpeed = 2.0f;

    [SerializeField, Header("���G���Ԑݒ�")]
    protected float invincibilityDuration = 1.0f; // �������̖��G����
    [SerializeField]
    protected float damageInvincibilityDuration = 0.2f; // �_���[�W��̖��G����

    [SerializeField, Header("���o����")]
    protected Color invincibleColor = Color.red;
    protected Color normalColor = Color.white;

    // ���G��ԊǗ�
    protected bool isInvincible = false;
    protected float invincibilityTimer = 0f;
    protected bool isSpawnInvincible = true; // ���������G�t���O
    protected bool isDead = false;

    // �ꎞ�I�ȑ��x�o�t
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
        // Player�Q�Ƃ̎擾
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            //�v���C���[�R���|�[�l���g�擾
            playerrb = playerObj.GetComponent<Rigidbody2D>();
            player = GameObject.FindWithTag("Player").transform;

        }

        // ���g�̃R���|�[�l���g�擾
        rb = GetComponent<Rigidbody2D>();
        rb.drag = 2f;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // �����ݒ�
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
    // �e�G���ƂɎ���
    protected virtual void OnInit()
    {

    }

    // �e�G���ƂɎ���
    protected virtual void Move()
    {

    }

    // Update is called once per frame
    protected void Update()
    {

        // ���G���Ԃ̍X�V
        UpdateInvincibility();

        Move();
    }

    //���G���Ԃ̍X�V
    protected virtual void UpdateInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            // �_�Ō���
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
    /// ���G���ԊJ�n
    protected virtual void StartInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
    }

    /// ���G���ԏI��
    protected virtual void EndInvincibility()
    {
        isInvincible = false;
        isSpawnInvincible = false; // ���������G�t���O�����Z�b�g
        invincibilityTimer = 0f;

        // �F�����ɖ߂�
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    // �ꎞ�I�ȑ��x�o�t�̍X�V
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

    // �ꎞ�I�ȑ��x�o�t��K�p
    public virtual void ApplyTemporarySpeedBuff(float multiplier, float duration)
    {
        tempSpeedMultiplier = multiplier;
        tempSpeedBuffTimer = duration;
    }

    // ���݂̎����ړ����x���擾
    protected float GetEffectiveMoveSpeed()
    {
        return moveSpeed * tempSpeedMultiplier;
    }

    public virtual bool TakeDamage(int damage, Vector2 knockback)
    {
        // ���G���Ԓ��̓_���[�W���󂯂Ȃ�
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
        // �_���[�W��̖��G����
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
        // ���S�G�t�F�N�g���̏����������ɒǉ��\
        OnDeath();

        Destroy(gameObject);
    }

    protected virtual void OnDeath()
    {
        // �ʂ̓G�ŃI�[�o�[���C�h���ēƎ��̏�����ǉ�
        // ��F�����G�t�F�N�g�A�A�C�e���h���b�v�A������
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // �v���C���[�Ƀ_���[�W��^����
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
            // �v���C���[�Ƀ_���[�W��^����
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
        // HP�\���p�̃o�[
        Gizmos.color = Color.red;
        Vector3 healthBarPos = transform.position + Vector3.up * 1.5f;
        float healthPercent = maxHp > 0 ? hp / maxHp : 0;
        Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f,
            healthBarPos - Vector3.right * 0.5f + Vector3.right * healthPercent);

        // �ړ������\��
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Vector3 direction = (player.position - transform.position).normalized;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
    }
}
