using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectTwoPoints : MonoBehaviour
{
    public Sprite lineSprite; // �X�v���C�g�摜���C���X�y�N�^�[����ݒ�

    public void CreateLineBetween(Vector2 start, Vector2 end)
    {
        // �V���� GameObject ���쐬
        GameObject lineObject = new GameObject("Line");

        // �X�v���C�g�����_���[�ǉ�
        SpriteRenderer sr = lineObject.AddComponent<SpriteRenderer>();
        sr.sprite = lineSprite;

        // �ʒu�ݒ�i�����ɔz�u�j
        Vector2 center = (start + end) / 2f;
        lineObject.transform.position = center;

        // �p�x�ݒ�
        Vector2 direction = end - start;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        // �����ɍ��킹�ăX�P�[�����O
        float length = direction.magnitude;
        lineObject.transform.localScale = new Vector3(length, 1, 1); // �X�v���C�g�̉�����1�Ȃ�OK
    }
}
