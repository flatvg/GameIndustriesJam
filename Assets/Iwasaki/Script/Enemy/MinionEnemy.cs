using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionEnemy : EnemyBase
{
    [SerializeField]
    private float separationRadius = 1f; // ���̓G�Ƃ̋�����ۂ�
    [SerializeField]
    private float leaderFollowRadius = 1f; // ���[�_�[��Ǐ]���鋗��

    [SerializeField]
    private float size = 0.5f; // �ʏ�̐F

    private SwarmLeader leader = null;
    private float baseMoveSpeed; // ��{�ړ����x
    private float currentSpeedMultiplier = 1f; // ���݂̑��x�{��
    private float buffTimer = 0f; // �o�t�c�莞��

    protected override void OnInit()
    {
        // �Q��G�̐ݒ�
        hp = 1; // �̗͒��
        baseMoveSpeed = moveSpeed;
        moveSpeed = Random.Range(baseMoveSpeed * 0.8f, baseMoveSpeed * 1.2f); // ���x�ɂ΂��
        transform.localScale = Vector3.one * size; // �T�C�Y������
    }

    protected override void Move()
    {
        if (player != null && !isDead)
        {
            Vector2 moveDirection = Vector2.zero;

            // ���[�_�[�������Ă���ꍇ�̃��[�_�[�Ǐ]�s��
            if (leader != null && leader.gameObject != null)
            {
                float distanceToLeader = Vector2.Distance(transform.position, leader.transform.position);

                // ���[�_�[���痣�ꂷ���Ă���ꍇ�̓��[�_�[�ɋ߂Â�
                if (distanceToLeader > leaderFollowRadius)
                {
                    Vector2 toLeader = (leader.transform.position - transform.position).normalized;
                    moveDirection += toLeader * 0.7f; // ���[�_�[�Ǐ]��
                }
            }

            // �v���C���[�Ɍ�������
            Vector2 toPlayer = (player.position - transform.position).normalized;
            moveDirection += toPlayer;

            // ���̓G���痣���́i�����s���j
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, separationRadius);
            foreach (var enemy in nearbyEnemies)
            {
                if (enemy.gameObject != gameObject && enemy.CompareTag("Enemy"))
                {
                    Vector2 awayFromEnemy = (transform.position - enemy.transform.position).normalized;
                    moveDirection += awayFromEnemy * 0.5f;
                }
            }

            // �o�t���ʂ�K�p�����ړ����x�ňړ�
            float currentMoveSpeed = moveSpeed * currentSpeedMultiplier;
            transform.Translate(moveDirection.normalized * currentMoveSpeed * Time.deltaTime);

            // �o�t���Ԃ̍X�V
            UpdateBuff();
        }
    }

    // ���[�_�[��ݒ�
    public void SetLeader(SwarmLeader newLeader)
    {
        leader = newLeader;
    }

    // ���[�_�[����̃o�t��K�p
    public void ApplyLeaderBuff(float multiplier)
    {
        currentSpeedMultiplier = multiplier;
        buffTimer = 1f; // 1�b�ԃo�t���ʎ���

        // �o�t���̎��o���ʁi�������邭�j
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(normalColor, Color.white, 0.3f);
        }
    }

    // �o�t���ʂ̍X�V
    private void UpdateBuff()
    {
        if (buffTimer > 0f)
        {
            buffTimer -= Time.deltaTime;

            if (buffTimer <= 0f)
            {
                // �o�t�I��
                currentSpeedMultiplier = 1f;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = normalColor;
                }
            }
        }
    }

    // ���[�_�[�����񂾎��̏���
    public void OnLeaderDeath()
    {
        leader = null;
        currentSpeedMultiplier = 1f; // �o�t����
        buffTimer = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    // ���������񂾎��Ƀ��[�_�[�ɒʒm
    protected override void OnDeath()
    {
        if (leader != null)
        {
            leader.RemoveSubordinate(this);
        }

        base.OnDeath();
    }
}

