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
        //// オブジェクトの座標取得
        //Vector2 pos = gameObject.transform.position;

        //// 入力は左クリックで射撃、マウスカーソルに向かってプレイヤーが向かっていく、スキルは多分キーボードの入力
        //Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float deltaTime = Time.deltaTime;

        //// マウスとプレイヤーとの距離を計算
        //Vector2 distance =  mousePos - pos;
        //float length = distance.magnitude;
        //direction = distance.normalized;

        //// マウスとの距離が遠いほどスピードをあげるためただし、１まで
        //length = Mathf.Clamp(length, 0, 1);

        //// 角度を変更
        //float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0, 0, -angle);

        // 動き
        Move(deltaTime);

        // 角度を変更
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

        // マウスとプレイヤーとのベクトルを算出
        Vector2 vector = mousePos - pos;
        // 方向取り出す direction は参照するためにメンバ
        direction = vector.normalized;

        // マウスとプレイヤーのベクトルの長さ　
        // これを使って、マウスとプレイヤーが離れていればスピード
        float length = vector.magnitude;
        length = Mathf.Clamp(length, 0, 1);

        pos += direction * (speed * length) * deltaTime;
        transform.position = pos;
    }

    private void UpdateShot()
    {
        isShot = false;

        // マウス左クリックで射撃
        if(Input.GetMouseButtonDown(0))
        {
            isShot = true;
            Debug.Log("Shot");
        }
    }
}
