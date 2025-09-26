using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

// レベルアップのキル数
// 1 or -> 4 -> 8 -> 16 -> 32

public class Bullet : MonoBehaviour
{
    [SerializeField] Vector3 moveDirection = Vector3.zero; // 移動方向
    [SerializeField] float moveSpeed = 1f;                 // 移動速度
    [SerializeField] float outMargin = 0.05f;              // 画面外かを判別する際に余白
    [SerializeField] float coolDownTime = 1.5f;            // 画面外に出た際のクールダウン時間
    [SerializeField] float offsetDeg = 90f;                // 発射時の回転オフセット
    public bool isShot = false;                            // 発射しているか
    public Transform bindPoint;                            // 回転時の参照店

    private int attackPower = 0;
    private int pirceCount = 0;   // 一度発射でのヒット数
    private int hitCount = 0;     // 累計ヒットする
    public int level = 1;         // 弾のレベル
    public BulletManager manager; // マネージャーへの参照

    Coroutine running;
    public static readonly int[] KillCount =
    {
        0,
        1,
        4,
        8,
        16,
        32
    };

    public static readonly Color[] tirangelColors =
    {
        Color.black,
        Color.blue,
        Color.green,
        Color.magenta,
        Color.yellow,
        Color.red,
    };

    // Start is called before the first frame update
    void Start()
    {
        running = null;
    }

    // Update is called once per frame
    void Update()
    {
        // レベル設定
        if (level < KillCount.Length)
        {
            if (hitCount >= KillCount[level])
            {
                level++;
                hitCount = 0;
            }
        }

        // レベルに応じて色を変更
        GetComponent<SpriteRenderer>().color = tirangelColors[level];

        if (!isShot)
        {
            // ポイントが存在しているか確認
            if (bindPoint != null)
            {
                Player player = manager.player;
                transform.position = bindPoint.position;
                transform.rotation = bindPoint.rotation;
            }
            else
            {
                Debug.Log("Bind Point Not Found.");
            }
        }
        else
        {
            // 移動
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // 画面外に出たか判定
            if (IsOutOfScreen(Camera.main))
            {
                if (running == null)
                {
                    if (pirceCount == 0)
                    {
                        // 誰にもあったていないのでレベルリセット
                        running = StartCoroutine(HandleOutOfScreenLater());
                    }
                    else
                    {
                        // 誰かしらにあったているのでレベルリセットを行なわない
                        pirceCount = 0;
                        isShot = false;
                    }
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ボスとあたった時
        //if (collision.gameObject.tag == "Boss")
        //{
        //    EnemyBoss boss = collision.gameObject.GetComponent<EnemyBoss>();
        //    if (boss != null)
        //    {
        //        // 吹き飛ばさない
        //        if (boss.TakeDamage(isShot ? level : 0, Vector2.zero))
        //        {
        //            hitCount++;
        //            pirceCount++;
        //            isShot = false;
        //            // コルーチン中断
        //            if (running != null)
        //            {
        //                StopCoroutine(running);
        //                running = null;
        //            }
        //        }
        //    }
        //    return;
        //}

        // 敵と当たった時
        if (collision.gameObject.tag == "Enemy")
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                Vector2 knockBack = isShot ? Vector2.zero : enemy.transform.position - manager.player.transform.position;
                if (enemy.TakeDamage(isShot ? level : 0, knockBack))
                {
                    hitCount++;
                    pirceCount++;
                    // 貫通数上限
                    if (pirceCount >= level)
                    {
                        isShot = false;
                        // コルーチン中断
                        if (running != null)
                        {
                            StopCoroutine(running);
                            running = null;
                        }
                    }
                }
            }
        }
    }

    public void Shot(Vector2 direction, float deg)
    {
        if (isShot) return; // すでに発射されているのでスキップ

        pirceCount = 0; // 貫通カウントリセット
        // 位置をプレイヤーの前に設定
        transform.position = (Vector2)manager.player.transform.position + (direction * manager.radius);
        transform.rotation = Quaternion.Euler(0, 0, deg + offsetDeg);
        moveDirection = direction;
        isShot = true;
    }

    // 画面外に出た際に制御
    private IEnumerator HandleOutOfScreenLater()
    {
        yield return new WaitForSeconds(coolDownTime);

        // レベルリセット
        level = 1;
        // 回転状態にする
        isShot = false;
        running = null;
    }

    // 画面外に出たか
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
