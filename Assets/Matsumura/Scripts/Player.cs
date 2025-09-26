// プレイヤーが目かそウじゃないか
#define PLAYER_EYE

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 4;

    private SpriteRenderer sprRenderer;
    private Animator anim;
    private float length = 0;
    private Vector2 spriteSize;

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
        
        spriteSize = sprRenderer.size;
        child = transform.Find("eye");
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
#if PLAYER_EYE
            Turn(deltaTime, direction);
#endif

            // 射撃処理(入力とフラグだけ)
            UpdateShot();

            // スキル ※全然できてないから 今のところ無視してて
            UpdateSkill3_3();
        }

#if !PLAYER_EYE
        // アニメ
        UpdateAnim();
#endif
        InCamera();

        // テスト
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDeath = true;
        }
    }

    private void Turn(float deltaTime, in Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    private void UpdateSkill3_3()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // Lv.3 以上のものがいくつあるか数える
            int level3OverCount = 0;
            List<int> level3Index = new List<int>();
            for (int i = 0; i < 5; ++i)
            {
                if (manaComp.bullets[i].level >= 2)
                {
                    ++level3OverCount;
                    level3Index.Add(i);
                }
            }

            // Lv.3 以上のものが 3つないのでスキル打てない
            if (level3OverCount < 2) return;

            // レベルを消費
            for (int i = 0; i < 3; ++i)
            {
                manaComp.bullets[level3Index[i]].level = 1;
            }

            // 敵を最大５体レーザービームで倒す
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            List<Vector2> enemyPos = new List<Vector2>();
            foreach (GameObject enemy in enemies)
            {
                enemyPos.Add(enemy.transform.position);
            }
            // プレイヤーに一番近い敵を見つけ出す。
            int mostNearEnemyIndex = 0;
            float mostNearEnemyLength = 1000;
            Vector2 playerPosition = transform.position;
            for(int i = 0; i<enemyPos.Count; ++i)
            {
                float distance = Vector2.Distance(playerPosition, enemyPos[i]);

                if (distance < mostNearEnemyLength)
                {
                    mostNearEnemyLength = distance;
                    mostNearEnemyIndex = i;
                }
            }

            GetComponent<ConnectTwoPoints>().CreateLineBetween(playerPosition, enemyPos[mostNearEnemyIndex]);
            Debug.Log("Use Skill");
        }
    }

    private void Move(float deltaTime)
    {
        Vector2 pos = transform.position;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // マウスとプレイヤーとのベクトルを算出
        Vector2 vector = mousePos - pos;
        // 方向取り出す direction は参照するためにメンバ
        direction = vector.normalized;

#if !PLAYER_EYE
        // スプライトの左右処理
        if (vector.x < 0)
            sprRenderer.flipX = true;
        else if (vector.x > 0)
            sprRenderer.flipX = false;
#endif

        // マウスとプレイヤーのベクトルの長さ　
        // これを使って、マウスとプレイヤーが離れていればスピード
        length = vector.magnitude;
        length = Mathf.Clamp(length, 0, 3);

        if (length < 1.2f)
            length = 0;

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

    private void InCamera()
    {
        float topY = Camera.main.transform.position.y + Camera.main.orthographicSize;
        float bottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;
        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float rightX = Camera.main.transform.position.x + halfWidth;
        float leftX = Camera.main.transform.position.x - halfWidth;

        if (transform.position.y > topY - spriteSize.y / 2) transform.position = new Vector3(transform.position.x, topY - spriteSize.y / 2, transform.position.z);
        if (transform.position.y < bottomY + spriteSize.y / 2) transform.position = new Vector3(transform.position.x, bottomY + spriteSize.y / 2, transform.position.z);
        if(transform.position.x > rightX - spriteSize.x /2) transform.position = new Vector2(rightX - spriteSize.x /2, transform.position.y);
        if(transform.position.x < leftX + spriteSize.x /2) transform.position = new Vector2(leftX + spriteSize.x /2, transform.position.y);
    }
}
