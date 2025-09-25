using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{

    [SerializeField]
    protected float hp = 1f;
    [SerializeField,Header("移動速度")]
    protected float moveSpeed = 2.0f;

    protected Transform player;
    protected Rigidbody2D playerrb;

    void Awake()
    {
        playerrb = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    // 各敵ごとに実装
    protected virtual void Move()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    public virtual void TakeDamage(int damage, Vector2 knockback)
    {
        hp -= damage;
        transform.position += (Vector3)knockback;
        if (hp <= 0) Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
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
            // 自分は消える
            Die();
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
            //}
            // 自分は消える
            Die();
        }
    }
}
