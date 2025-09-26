using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

// ���x���A�b�v�̃L����
// 1 or -> 4 -> 8 -> 16 -> 32

public class Bullet : MonoBehaviour
{
    [SerializeField] Vector3 moveDirection = Vector3.zero; // �ړ�����
    [SerializeField] float moveSpeed = 1f;                 // �ړ����x
    [SerializeField] float outMargin = 0.05f;              // ��ʊO���𔻕ʂ���ۂɗ]��
    [SerializeField] float coolDownTime = 1.5f;            // ��ʊO�ɏo���ۂ̃N�[���_�E������
    [SerializeField] float offsetDeg = 90f;                // ���ˎ��̉�]�I�t�Z�b�g
    public bool isShot = false;                            // ���˂��Ă��邩
    public Transform bindPoint;                            // ��]���̎Q�ƓX

    private int attackPower = 0;
    private int pirceCount = 0;   // ��x���˂ł̃q�b�g��
    private int hitCount = 0;     // �݌v�q�b�g����
    public int level = 1;         // �e�̃��x��
    public BulletManager manager; // �}�l�[�W���[�ւ̎Q��

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
        // ���x���ݒ�
        if (level < KillCount.Length)
        {
            if (hitCount >= KillCount[level])
            {
                level++;
                hitCount = 0;
            }
        }

        // ���x���ɉ����ĐF��ύX
        GetComponent<SpriteRenderer>().color = tirangelColors[level];

        if (!isShot)
        {
            // �|�C���g�����݂��Ă��邩�m�F
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
            // �ړ�
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // ��ʊO�ɏo��������
            if (IsOutOfScreen(Camera.main))
            {
                if (running == null)
                {
                    if (pirceCount == 0)
                    {
                        // �N�ɂ��������Ă��Ȃ��̂Ń��x�����Z�b�g
                        running = StartCoroutine(HandleOutOfScreenLater());
                    }
                    else
                    {
                        // �N������ɂ������Ă���̂Ń��x�����Z�b�g���s�Ȃ�Ȃ�
                        pirceCount = 0;
                        isShot = false;
                    }
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �{�X�Ƃ���������
        //if (collision.gameObject.tag == "Boss")
        //{
        //    EnemyBoss boss = collision.gameObject.GetComponent<EnemyBoss>();
        //    if (boss != null)
        //    {
        //        // ������΂��Ȃ�
        //        if (boss.TakeDamage(isShot ? level : 0, Vector2.zero))
        //        {
        //            hitCount++;
        //            pirceCount++;
        //            isShot = false;
        //            // �R���[�`�����f
        //            if (running != null)
        //            {
        //                StopCoroutine(running);
        //                running = null;
        //            }
        //        }
        //    }
        //    return;
        //}

        // �G�Ɠ���������
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
                    // �ђʐ����
                    if (pirceCount >= level)
                    {
                        isShot = false;
                        // �R���[�`�����f
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
        if (isShot) return; // ���łɔ��˂���Ă���̂ŃX�L�b�v

        pirceCount = 0; // �ђʃJ�E���g���Z�b�g
        // �ʒu���v���C���[�̑O�ɐݒ�
        transform.position = (Vector2)manager.player.transform.position + (direction * manager.radius);
        transform.rotation = Quaternion.Euler(0, 0, deg + offsetDeg);
        moveDirection = direction;
        isShot = true;
    }

    // ��ʊO�ɏo���ۂɐ���
    private IEnumerator HandleOutOfScreenLater()
    {
        yield return new WaitForSeconds(coolDownTime);

        // ���x�����Z�b�g
        level = 1;
        // ��]��Ԃɂ���
        isShot = false;
        running = null;
    }

    // ��ʊO�ɏo����
    private bool IsOutOfScreen(Camera cam)
    {
        if (cam == null) return false;
        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        // �J�����w�ʂɉ�����瑦�A�E�g
        if (vp.z < 0f) return true;

        // �]�����l�����Ĕ͈͊O�Ȃ�A�E�g
        return (vp.x < -outMargin || vp.x > 1f + outMargin ||
                vp.y < -outMargin || vp.y > 1f + outMargin);
    }
}
