using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("�����ݒ�")]
    [SerializeField,Header("��������G��")] private GameObject minionPrefab; 
    [SerializeField,Header("�����p�x�i�b")] private float spawnInterval = 1.5f; 
    [SerializeField,Header("1�񂠂���̏�����")] private int spawnCount = 3;  
    [SerializeField,Header("�����ʒu�̔��a")] private float spawnRadius = 3f;

    [Header("�ړ��ݒ�")]
    [SerializeField,Header("�ړ��|�C���g")] private Vector2[] movePoints;

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
        rb.drag = 500f; // ������}����
        rb.angularDrag = 500f; // ��]��}����
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

    }

    protected override void Move()
    {
        // �ړ��i�܂��~�܂��Ă��Ȃ��������j
        if (!isStopped)
        {
            MoveAlongPoints();
        }

        // ��~��͏������������s
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

        // �ڕW�n�_�ɋ߂Â����玟�̃|�C���g��
        if (Vector3.Distance(transform.position, (Vector3)targetPoint) < 0.1f)
        {
            currentPointIndex++;

            // �ŏI�|�C���g�ɒ��������~
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
            // �r���[�|�[�g���W�i0�`1�j�͈͓̔����烉���_���Ɍ���
            float vx = Random.Range(0.1f, 0.9f); // �[�������ꍇ��0.1�`0.9
            float vy = Random.Range(0.1f, 0.9f);

            // �J�����������w��
            Vector3 spawnPos = cam.ViewportToWorldPoint(
                new Vector3(vx, vy, Mathf.Abs(cam.transform.position.z - transform.position.z))
            );
            spawnPos.z = 0f;

            Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        }
    }
}