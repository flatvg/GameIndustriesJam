using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// �Q��̃��[�_�[ - �F�Ⴂ�ŉ�ʓ��̓G�ɑ��x�o�t��t�^
public class SwarmLeader : EnemyBase
{
    [SerializeField]
    private float buffRadius = 10f; // �o�t���ʔ͈�
    [SerializeField]
    private float speedBuffMultiplier = 1.3f; // ���x�o�t�{��
    [SerializeField]
    private float buffUpdateInterval = 0.5f; // �o�t�X�V�Ԋu
    [SerializeField]
    private Color leaderColor = Color.yellow; // ���[�_�[�̐F

    private List<MinionEnemy> subordinates = new List<MinionEnemy>();
    private float buffTimer = 0f;

    protected override void OnInit()
    {
        // ���[�_�[�̌����ڐݒ�
        if (spriteRenderer != null)
        {
            normalColor = leaderColor;
            spriteRenderer.color = leaderColor;
        }

        // �T�C�Y�������傫��
        transform.localScale = Vector3.one * 1.2f;
    }

    protected override void Move()
    {
        if (player != null && !isDead)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            // ����I�Ƀo�t���X�V
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                ApplySpeedBuffToNearbyEnemies();
                buffTimer = buffUpdateInterval;
            }
        }
    }

    // ������ǉ�
    public void AddSubordinate(MinionEnemy subordinate)
    {
        if (subordinate != null)
        {
            subordinates.Add(subordinate);
            subordinate.SetLeader(this);
        }
    }

    // �������폜
    public void RemoveSubordinate(MinionEnemy subordinate)
    {
        if (subordinates.Contains(subordinate))
        {
            subordinates.Remove(subordinate);
        }
    }

    // ��ʓ��̓G�ɑ��x�o�t��t�^
    private void ApplySpeedBuffToNearbyEnemies()
    {
        // ��ʓ��̑S�Ă̓G���擾
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();

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
                // �o�t���ʂ�K�p
                MinionEnemy swarmEnemy = enemy as MinionEnemy;
                if (swarmEnemy != null)
                {
                    swarmEnemy.ApplyLeaderBuff(speedBuffMultiplier);
                }
                else
                {
                    // ���̓G�^�C�v�ɂ��y���o�t��K�p
                    enemy.ApplyTemporarySpeedBuff(speedBuffMultiplier * 0.7f, buffUpdateInterval * 1.5f);
                }
            }
        }
    }

    // ���[�_�[�����񂾎��̏���
    protected override void OnDeath()
    {
        // �����c���������̃o�t������
        foreach (MinionEnemy subordinate in subordinates)
        {
            if (subordinate != null)
            {
                subordinate.OnLeaderDeath();
            }
        }

        base.OnDeath();
    }

    // �f�o�b�O�p�F�o�t�͈͕\��
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // �o�t�͈͂�\��
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCircle(transform.position, buffRadius);
    }
}
