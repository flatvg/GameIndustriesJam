using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterEnemy : EnemyBase
{
    [SerializeField,Header("���􂷂�G")]
    private GameObject miniEnemyPrefab;

    protected override void Move()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    protected override void Die()
    {
        // ���􂵂ď������G��2�̐���
        if (miniEnemyPrefab != null)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 randDir = Random.insideUnitCircle.normalized;
                Vector3 spawnPos = transform.position + (Vector3)(randDir * 0.5f);
                Instantiate(miniEnemyPrefab, spawnPos, Quaternion.identity);
            }
        }
        base.Die();
    }
}
