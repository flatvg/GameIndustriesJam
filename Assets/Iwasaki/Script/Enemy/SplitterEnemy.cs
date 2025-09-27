using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SplitterEnemy : EnemyBase
{
    [SerializeField,Header("���􂷂�G")]
    private GameObject miniEnemyPrefab;

    private bool UnderLv = false;

    protected override void Move()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * GetEffectiveMoveSpeed() * Time.deltaTime;
    }

    protected override void Die()
    {
        if (!UnderLv)
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
        }

        base.Die();
    }

    protected override void OnHitCol(Collision2D other)
    {
        var b = other.gameObject.GetComponent<Bullet>();
        if (b != null)
        {
            if (b.level > maxHp + 1)
            {
                UnderLv = true;
            }
        }
    }
}
