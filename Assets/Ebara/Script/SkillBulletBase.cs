using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillBulletBase : MonoBehaviour
{
    [SerializeField] Vector3 moveDirection = Vector3.zero; // 移動方向
    [SerializeField] float moveSpeed = 1f;                 // 移動速度
    [SerializeField] float outMargin = 0.05f;              // 外に出た際の処理
    public Transform bindPoint;
    public int damage = 1;                                // 敵に与えるダメージ
    public bool isShot = false;

    public BulletManager manager; // マネージャーへの参照

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // 移動処理
        Move();

        // 画面外に出たときの処理
        if(IsOutOfScreen(Camera.main))
        {
            OnWithOutScreen();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 敵と当たった時
        if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Boss")
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                OnHitEnemy(enemy);
            }
        }
    }

    // スキルを打つ処理
    protected virtual void Shot()
    {
        isShot = true;
    }

    // 敵と当たった時の処理
    protected virtual void OnHitEnemy(EnemyBase enemy)
    {
        Vector2 knockBack = isShot ? Vector2.zero : enemy.transform.position - manager.player.transform.position;
        enemy.TakeDamage(damage, knockBack);
    }

    // 画面買いに出た際の処理
    protected virtual void OnWithOutScreen()
    {
        Destroy(gameObject);
    }

    // 移動処理
    protected virtual void Move()
    {
        if (isShot)
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private bool IsOutOfScreen(Camera cam)
    {
        if (cam == null) return false;
        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        // カメラ背面に回ったら即アウト
        if (vp.z < 0f) return true;

        // 余白を考慮して範囲外ならアウト
        return (vp.x < -outMargin || vp.x > 1f + outMargin ||
                vp.y < -outMargin || vp.y > 1f + outMargin);
    }
}