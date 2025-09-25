using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterEnemy : EnemyBase
{
    [SerializeField,Header("•ª—ô‚·‚é“G")]
    private GameObject miniEnemyPrefab;

    protected override void Move()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    protected override void Die()
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
        base.Die();
    }
}
