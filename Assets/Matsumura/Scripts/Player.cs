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
            // 動き
            Move(deltaTime);

            // 角度を変更
            Turn(deltaTime, direction);

            // 射撃処理(入力とフラグだけ)
            UpdateShot();
        }

        // アニメ
        UpdateAnim();

        // テスト
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

        // マウスとプレイヤーとのベクトルを算出
        Vector2 vector = mousePos - pos;
        // 方向取り出す direction は参照するためにメンバ
        direction = vector.normalized;

        // スプライトの左右処理
        if (vector.x < 0)
            sprRenderer.flipX = true;
        else if (vector.x > 0)
            sprRenderer.flipX = false;

        // マウスとプレイヤーのベクトルの長さ　
        // これを使って、マウスとプレイヤーが離れていればスピード
        length = vector.magnitude;
        length = Mathf.Clamp(length, 0, 1);

        pos += direction * (speed * length) * deltaTime;
        transform.position = pos;
    }

    private void UpdateShot()
    {
        isShot = false;

        // マウス左クリックで射撃
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
