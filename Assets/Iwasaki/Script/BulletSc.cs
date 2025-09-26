using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BulletSc : MonoBehaviour
{
    // Update is called once per frame
    public float margin = 0.2f; // ��ʊO�ɂǂꂭ�炢�]�T�������i�r���[�|�[�g���W�j

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // ��ʊO����
        if (IsOutOfScreenWithMargin())
        {
            Destroy(gameObject);
        }
    }

    bool IsOutOfScreenWithMargin()
    {
        // �J�������擾
        Camera cam = Camera.main;
        if (cam == null) return false;

        // ���[���h���W���r���[�|�[�g���W
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        // ��ʊO + margin ���ǂ���
        if (viewportPos.x < -margin || viewportPos.x > 1 + margin ||
            viewportPos.y < -margin || viewportPos.y > 1 + margin)
        {
            return true;
        }
        return false;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // Player��Shield�ȊO�͖���
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Shield"))
        {
            // �R���C�_�[�𖳎�������ꍇ
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other.collider);
            return;
        }

        if (other.gameObject.CompareTag("Shield"))
        {
            Destroy(gameObject);
        }

        if (other.gameObject.CompareTag("Player"))
        {
            // �v���C���[�Ƀ_���[�W��^����
            //PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            //if (playerController != null)
            //{
            //    Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            //    playerController.TakeDamage(1, knockbackDir * 2f);
            //
            Destroy(gameObject);
            SceneManager.LoadScene("Result");
        }
    }
}
