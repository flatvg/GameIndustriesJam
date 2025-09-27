// プレイヤーが目かそウじゃないか
#define PLAYER_EYE

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 4;

    private SpriteRenderer sprRenderer;
    private Animator anim;
    private float length = 0;
    private Vector2 spriteSize;
    private Rigidbody2D rb;
    private CameraShaker camShaker;
    private List<GameObject> objects = new List<GameObject>();

    public bool isDeath { get; private set; }

    public BulletManager manaComp;
    public bool isShot { get; private set; }
    public Vector2 direction { get; private set; }
    public bool isSpecialMove { get; private set; }
    public bool enableMove { get; private set; }

    // 予測線関係
    public GameObject circlePrefab;
    [SerializeField] float Interval = 2;
    [SerializeField] int circleCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        enableMove = false;
        isShot = false;
        isDeath = false;
        isSpecialMove = false;
        gameObject.transform.position = Vector3.zero;
        anim = GetComponent<Animator>();
        sprRenderer = gameObject.GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        camShaker = gameObject.GetComponent<CameraShaker>();

        for (int i = 0; i < circleCount; i++)
        {
            objects.Add(GameObject.Instantiate(circlePrefab, Vector3.zero, Quaternion.identity));
        }
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
            //UpdateSkill3_3();

        }
        TrendLine();

#if !PLAYER_EYE
        // アニメ
        UpdateAnim();
#endif
        InCamera();

        //// テスト
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDeath = true;
        }
    }

    private float prevAngle = 0;
    private float anglerSpeed = 90f;
    private void Turn(float deltaTime, in Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);

        //Quaternion targetAngle = Quaternion.Euler(0, 0, -angle);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, anglerSpeed * Time.deltaTime);
    }

    //private void UpdateSkill3_3()
    //{
    //    if (Input.GetKeyDown(KeyCode.A))
    //    {
    //        // Lv.3 以上のものがいくつあるか数える
    //        int level3OverCount = 0;
    //        List<int> level3Index = new List<int>();
    //        for (int i = 0; i < 5; ++i)
    //        {
    //            if (manaComp.bullets[i].level >= 2)
    //            {
    //                ++level3OverCount;
    //                level3Index.Add(i);
    //            }
    //        }

    //        // Lv.3 以上のものが 3つないのでスキル打てない
    //        if (level3OverCount < 2) return;

    //        // レベルを消費
    //        for (int i = 0; i < 3; ++i)
    //        {
    //            manaComp.bullets[level3Index[i]].level = 1;
    //        }

    //        // 敵を最大５体レーザービームで倒す
    //        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    //        List<Vector2> enemyPos = new List<Vector2>();
    //        foreach (GameObject enemy in enemies)
    //        {
    //            enemyPos.Add(enemy.transform.position);
    //        }
    //        // プレイヤーに一番近い敵を見つけ出す。
    //        int mostNearEnemyIndex = 0;
    //        float mostNearEnemyLength = 1000;
    //        Vector2 playerPosition = transform.position;
    //        for (int i = 0; i < enemyPos.Count; ++i)
    //        {
    //            float distance = Vector2.Distance(playerPosition, enemyPos[i]);

    //            if (distance < mostNearEnemyLength)
    //            {
    //                mostNearEnemyLength = distance;
    //                mostNearEnemyIndex = i;
    //            }
    //        }

    //        GetComponent<ConnectTwoPoints>().CreateLineBetween(playerPosition, enemyPos[mostNearEnemyIndex]);
    //        Debug.Log("Use Skill");
    //    }
    //}

    // ============================================================
    //                        移動処理関数
    // ============================================================
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
        {
            enableMove = false;
            length = 0;
        }
        else
            enableMove = true;

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

        if (Input.GetKeyDown(KeyCode.A))
        {
            // TODO 必殺攻撃
            if (manaComp.UseSkill2_2())
            {
                ShotSpecialMove(0.5f, 0.3f, 1f);
            }
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            if (manaComp.UseSkill3_3())
            {
                ShotSpecialMove(0.5f, 0.3f, 1f);
            }
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            if (manaComp.UseSkill5_5())
            {
                ShotSpecialMove(0.5f, 0.3f, 1f);
            }
        }
    }

    // ============================================================
    //                     死亡処理(フラグだけ)
    // ============================================================
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
        Vector3 pos = transform.position;
        // カメラの境界を計算
        float topY = Camera.main.transform.position.y + Camera.main.orthographicSize;
        float bottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;
        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float rightX = Camera.main.transform.position.x + halfWidth;
        float leftX = Camera.main.transform.position.x - halfWidth;

        // スプライトの半サイズ
        float halfHeight = spriteSize.y;
        float halfSpriteWidth = spriteSize.x;

        // Y軸制限
        pos.y = Mathf.Clamp(pos.y, bottomY + halfHeight, topY - halfHeight);

        // X軸制限
        pos.x = Mathf.Clamp(pos.x, leftX + halfSpriteWidth, rightX - halfSpriteWidth);

        // 位置を更新
        transform.position = pos;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("hit enemy");
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // ちょっと無理やり？
            rb.simulated = false;
            isDeath = true;
        }
    }

    private void ShotSpecialMove(float duration, float magnitude, float ratio)
    {
        camShaker.Shake(0.2f, 0.1f);
        StartCoroutine(BeamTimeEffect(duration, ratio));
    }


    private float time = 0;
    private IEnumerator BeamTimeEffect(float duration, float ratio)
    {
        float elapsed = 0.0f;
        Time.timeScale = 0.1f;
        while (Time.timeScale < 1)
        {
            time += 0.2f * Time.unscaledDeltaTime;
            Time.timeScale += time * Time.deltaTime * 1.5f;

            elapsed += Time.deltaTime;
            yield return null;
        }
        Time.timeScale = 1;
    }

    // ============================================================
    //                          予測線関数
    // ============================================================
    private void TrendLine()
    {
        Vector2 startPos = transform.position;
        int i = 1;
        foreach (var obj  in objects)
        {
            obj.transform.position = startPos + direction.normalized * Interval * i;
            i++;

            if(isDeath)
                obj.gameObject.SetActive(false);
        }
    }
}
