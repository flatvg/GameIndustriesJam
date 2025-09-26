using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterEnemy : EnemyBase
{
    public GameObject bulletPrefab;
    public float fireInterval = 7f;
    public float orbitRadius = 5f;       // ��]���a
    public float orbitSpeed = 1000f;       // �p���x�i�x/�b�j
    private float fireTimer;
    private float angle;

    protected override void OnInit()
    {
        angle = Random.Range(0f, 360f);
    }

    protected override void Move()
    {
        // �J�����̔��a�i��ʓ��Ɏ��܂�ő唼�a�j���v�Z
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // �v���C���[�𒆐S�ɂ��Ĉ��S�Ȕ��a�i�l�ӂ��班�������j
        float maxRadius = Mathf.Min(camWidth, camHeight) * 0.5f - 0.5f;
        float actualRadius = Mathf.Min(orbitRadius, maxRadius);

        // �p�x���X�V
        angle += orbitSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        // ���W�v�Z
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * actualRadius;
        transform.position = player.position + offset;

        // �v���C���[����������
        Vector3 dir = (player.position - transform.position).normalized;
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

        // �ˌ�
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
