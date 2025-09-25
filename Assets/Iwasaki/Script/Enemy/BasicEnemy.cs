using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : EnemyBase
{

    protected override void Move()
    {
        // 単純にプレイヤーへ向かう
        Transform p = GameObject.FindWithTag("Player").transform;
        Vector3 dir = (p.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
