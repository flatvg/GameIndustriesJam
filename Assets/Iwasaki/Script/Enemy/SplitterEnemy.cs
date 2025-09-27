using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SplitterEnemy : EnemyBase
{
    [SerializeField,Header("•ª—ô‚·‚é“G")]
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
            // •ª—ô‚µ‚Ä¬‚³‚¢“G‚ğ2‘Ì¶¬
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
