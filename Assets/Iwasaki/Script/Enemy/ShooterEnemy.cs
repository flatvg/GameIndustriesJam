using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterEnemy : EnemyBase
{
    public GameObject bulletPrefab;
    public float fireInterval = 7f;
    public float orbitRadius = 5f;       // 希望半径
    public float orbitSpeed = 1000f;       // 角速度（度/秒）
    private float fireTimer;
    private float angle;

    protected override void OnInit()
    {
        angle = Random.Range(0f, 360f);
    }

    protected override void Move()
    {
        // カメラの半径（画面内に収まる最大半径）を計算
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // プレイヤーを中心にして安全な半径（四辺から少し内側）
        float maxRadius = Mathf.Min(camWidth, camHeight) * 0.5f - 0.5f;
        float actualRadius = Mathf.Min(orbitRadius, maxRadius);

        // 角度を更新
        angle += orbitSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        // 座標計算
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * actualRadius;
        transform.position = player.position + offset;

        // プレイヤー方向を向く
        Vector3 dir = (player.position - transform.position).normalized;
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

        // 射撃
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = fireInterval;
            Shoot(dir);
        }
    }

    void Shoot(Vector3 dir)
    {
        var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = dir * 5f;
    }
}
