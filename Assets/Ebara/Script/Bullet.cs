using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Bullet : MonoBehaviour
{
    [SerializeField] Vector3 moveDirection = Vector3.zero; // �ړ�����
    [SerializeField] float moveSpeed = 1f;                 // �ړ����x
    [SerializeField] float outMargin = 0.05f;              // ��ʊO���𔻕ʂ���ۂɗ]��
    [SerializeField] float coolDownTime = 1.5f;            // ��ʊO�ɏo���ۂ̃N�[���_�E������
    public bool isShot = false;                            // ���˂��Ă��邩
    public Transform bindPoint;                            // ��]���̎Q�ƓX

    int level = 0; // �e�̃��x��
    //Player player; // �v���C���[�ւ̎Q��

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isShot)
        {
            // �|�C���g�����݂��Ă��邩�m�F
            if (bindPoint != null)
            {
                transform.position = bindPoint.position;
                transform.rotation = bindPoint.rotation;
            }
        }
        else
        {
            // �ړ�
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        // ��ʊO�ɏo�������f
        if (IsOutOfScreen(Camera.main))
        {
            // �x������
            StartCoroutine(HandleOutOfScreenLater());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // �G�ɓ���������
        if (collision.gameObject.tag == "Enemy")
        {

        }
    }

    public void Shot(Vector2 direction, float deg)
    {
        // �ʒu���v���C���[�̑O�ɐݒ�
        // ref BulletPoint point = player.bulletPoint.radius;
        // transform.position = player.transform.position + (direction * radius);
        transform.rotation = Quaternion.Euler(0, 0, deg + 90);
        moveDirection = direction;
        isShot = true;
    }

    // ��ʊO�ɏo���ۂɐ���
    IEnumerator HandleOutOfScreenLater()
    {
        yield return new WaitForSeconds(coolDownTime);

        // ��]��Ԃɂ���
        isShot = false;
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
