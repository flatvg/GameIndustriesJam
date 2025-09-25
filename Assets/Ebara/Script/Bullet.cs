using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Bullet : MonoBehaviour
{
    [SerializeField]Vector3 moveDirection = Vector3.zero; // 移動方向
    [SerializeField]float moveSpeed = 0.0f;               // 移動速度
    [SerializeField]bool isAttack = false;                // 発射しているか

    [SerializeField] float bulletInterval = 360 / 5;      // 弾の間隔

    //Player player; // プレイヤーへ斧参照

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isAttack) return;

        // 移動
        transform.position += moveDirection * moveSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 敵に当たった時
        if(collision.gameObject.tag == "Enemy")
        {

        }
    }

    public void Shot(Vector2 direction)
    {
        moveDirection = direction;
        isAttack = true;
    }
}
