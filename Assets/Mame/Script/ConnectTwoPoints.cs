using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectTwoPoints : MonoBehaviour
{
    public Sprite lineSprite; // �X�v���C�g�摜���C���X�y�N�^�[����ݒ�
    public Material UVMaterial;

    public void CreateLineBetween(Vector2 start, Vector2 end)
    {
        //// �V���� GameObject ���쐬
        //GameObject lineObject = new GameObject("Line");

        //// �X�v���C�g�����_���[�ǉ�
        //SpriteRenderer sr = lineObject.AddComponent<SpriteRenderer>();
        //sr.sprite = lineSprite;

        //// �ʒu�ݒ�i�����ɔz�u�j
        //Vector2 center = (start + end) / 2f;
        //lineObject.transform.position = center;

        //// �p�x�ݒ�
        //Vector2 direction = end - start;
        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //lineObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        //// �����ɍ��킹�ăX�P�[�����O
        //float length = direction.magnitude;
        //lineObject.transform.localScale = new Vector3(length, 1, 1); // �X�v���C�g�̉�����1�Ȃ�OK



        GameObject lineObject = new GameObject("Line");
        var sr = lineObject.AddComponent<SpriteRenderer>();
        var thunder = lineObject.AddComponent<Thunder>();
        sr.sprite = lineSprite;

        // UV�X�N���[���ݒ�
        if (UVMaterial != null)
        {
            sr.material = UVMaterial;
        }

        Vector2 center = (start + end) * 0.5f;
        lineObject.transform.position = center;

        Vector2 direction = end - start;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        float length = direction.magnitude;

        // �X�v���C�g�́g���̃��[���h��/�����h
        Vector2 spriteSize = sr.sprite.bounds.size;
        // �ڕW�̑���
        float targetThickness = spriteSize.y * 0.75f; // ���̂܂܂Ȃ�1�{�A�ׂ��������Ȃ�C�ӂ̒l

        // �������F�ڕW���� / �����A�c�����F�ڕW���� / ������
        lineObject.transform.localScale = new Vector3(
            length / Mathf.Max(1e-6f, spriteSize.x),
            targetThickness / Mathf.Max(1e-6f, spriteSize.y),
            1f
        );

        //�J�v�Z���R���C�_�[�����A�����ڂƍ��킹��
        var col = lineObject.AddComponent<CapsuleCollider2D>();
        col.isTrigger = true;
        col.direction = CapsuleDirection2D.Horizontal;
        col.size = sr.sprite.bounds.size;
        col.offset = Vector2.zero;
    }
}
