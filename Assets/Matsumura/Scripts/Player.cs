using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 4;

    public bool isShot { get; private set; }
    public Vector2 direction { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //// �I�u�W�F�N�g�̍��W�擾
        //Vector2 pos = gameObject.transform.position;

        //// ���͍͂��N���b�N�Ŏˌ��A�}�E�X�J�[�\���Ɍ������ăv���C���[���������Ă����A�X�L���͑����L�[�{�[�h�̓���
        //Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float deltaTime = Time.deltaTime;

        //// �}�E�X�ƃv���C���[�Ƃ̋������v�Z
        //Vector2 distance =  mousePos - pos;
        //float length = distance.magnitude;
        //direction = distance.normalized;

        //// �}�E�X�Ƃ̋����������قǃX�s�[�h�������邽�߂������A�P�܂�
        //length = Mathf.Clamp(length, 0, 1);

        //// �p�x��ύX
        //float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0, 0, -angle);

        // ����
        Move(deltaTime);

        // �p�x��ύX
        Turn(deltaTime, direction);

        UpdateShot();

        //pos += direction * 4 * length * deltaTime;
        //gameObject.transform.position = pos;
    }

    private void Turn(float deltaTime, in Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);
    }
    private void Move(float deltaTime)
    {
        Vector2 pos = transform.position;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // �}�E�X�ƃv���C���[�Ƃ̃x�N�g�����Z�o
        Vector2 vector = mousePos - pos;
        // �������o�� direction �͎Q�Ƃ��邽�߂Ƀ����o
        direction = vector.normalized;

        // �}�E�X�ƃv���C���[�̃x�N�g���̒����@
        // ������g���āA�}�E�X�ƃv���C���[������Ă���΃X�s�[�h
        float length = vector.magnitude;
        length = Mathf.Clamp(length, 0, 1);

        pos += direction * (speed * length) * deltaTime;
        transform.position = pos;
    }

    private void UpdateShot()
    {
        isShot = false;

        // �}�E�X���N���b�N�Ŏˌ�
        if(Input.GetMouseButtonDown(0))
        {
            isShot = true;
            Debug.Log("Shot");
        }
    }
}
