using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Windows;

public class Bullet : MonoBehaviour
{
    [SerializeField] Vector3 moveDirection = Vector3.zero; // 移動方向
    [SerializeField] float moveSpeed = 1f;                 // 移動速度
    [SerializeField] float outMargin = 0.05f;              // 画面外かを判別する際に余白
    [SerializeField] float coolDownTime = 1.5f;            // 画面外に出た際のクールダウン時間
    public bool isShot = false;                            // 発射しているか
    public Transform bindPoint;                            // 回転時の参照店

    int level = 0; // 弾のレベル
    //Player player; // プレイヤーへの参照

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isShot)
        {
            // ポイントが存在しているか確認
            if (bindPoint != null)
            {
                transform.position = bindPoint.position;
                transform.rotation = bindPoint.rotation;
            }
        }
        else
        {
            // 移動
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        // 画面外に出たか判定
        if (IsOutOfScreen(Camera.main))
        {
            // 遅延制御
            StartCoroutine(HandleOutOfScreenLater());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 敵と当たった時
        if(collision.collider.gameObject.tag == "Enemy")
        {

        }
    }

    public void Shot(Vector2 direction, float deg)
    {
        // 位置をプレイヤーの前に設定
        // ref BulletPoint point = player.bulletPoint.radius;
        // transform.position = player.transform.position + (direction * radius);
        transform.rotation = Quaternion.Euler(0, 0, deg + 90);
        moveDirection = direction;
        isShot = true;
    }

    // 画面外に出た際に制御
    private IEnumerator HandleOutOfScreenLater()
    {
        yield return new WaitForSeconds(coolDownTime);

        // 回転状態にする
        isShot = false;
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
