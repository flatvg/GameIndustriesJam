using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 4;

    public bool isShot { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // �I�u�W�F�N�g�̍��W�擾
        Vector2 pos = gameObject.transform.position;

        // ���͍͂��N���b�N�Ŏˌ��A�}�E�X�J�[�\���Ɍ������ăv���C���[���������Ă����A�X�L���͑����L�[�{�[�h�̓���
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dt = Time.deltaTime;

        // �}�E�X�ƃv���C���[�Ƃ̋������v�Z
        Vector2 distance =  mousePos - pos;
        float length = distance.magnitude;
        Vector2 dir = distance.normalized;

        // �}�E�X�Ƃ̋����������قǃX�s�[�h�������邽�߂������A�P�܂�
        length = Mathf.Clamp(length, 0, 1);

        // �p�x��ύX
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);

        UpdateShot();

        pos += dir * 4 * length * dt;
        gameObject.transform.position = pos;
    }

    private void UpdateShot()
    {
        isShot = false;

        // �}�E�X���N���b�N�Ŏˌ�
        if(Input.GetMouseButtonDown(0))
        {
            isShot = true;
        }
    }
}
