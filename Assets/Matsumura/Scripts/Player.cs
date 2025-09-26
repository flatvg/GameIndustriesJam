using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 4;

    private SpriteRenderer sprRenderer;
    private Animator anim;
    private float length = 0;

    public bool isDeath { get; private set; }

    public BulletManager manaComp;
    public bool isShot { get; private set; }
    public Vector2 direction { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        isShot = false;
        isDeath = false;
        gameObject.transform.position = Vector3.zero;
        anim = GetComponent<Animator>();
        sprRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;

        if (!isDeath)
        {
            // ����
            Move(deltaTime);

            // �p�x��ύX
            Turn(deltaTime, direction);

            // �ˌ�����(���͂ƃt���O����)
            UpdateShot();
        }

        // �A�j��
        UpdateAnim();

        // �e�X�g
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDeath = true;
        }
    }

    private void Turn(float deltaTime, in Vector2 direction)
    {
        //float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0, 0, -angle);
    }
    private void Move(float deltaTime)
    {
        Vector2 pos = transform.position;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // �}�E�X�ƃv���C���[�Ƃ̃x�N�g�����Z�o
        Vector2 vector = mousePos - pos;
        // �������o�� direction �͎Q�Ƃ��邽�߂Ƀ����o
        direction = vector.normalized;

        // �X�v���C�g�̍��E����
        if (vector.x < 0)
            sprRenderer.flipX = true;
        else if (vector.x > 0)
            sprRenderer.flipX = false;

        // �}�E�X�ƃv���C���[�̃x�N�g���̒����@
        // ������g���āA�}�E�X�ƃv���C���[������Ă���΃X�s�[�h
        length = vector.magnitude;
        length = Mathf.Clamp(length, 0, 1);

        pos += direction * (speed * length) * deltaTime;
        transform.position = pos;
    }

    private void UpdateShot()
    {
        isShot = false;

        // �}�E�X���N���b�N�Ŏˌ�
        if (Input.GetMouseButtonDown(0))
        {
            manaComp?.Shot(direction);
            isShot = true;
            Debug.Log("Shot");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDeath)
            isDeath = true;
    }

    private void UpdateAnim()
    {
        anim.SetBool("Walk", Mathf.Abs(length) > 0.2f);
        anim.SetBool("Death", isDeath);
    }
}
