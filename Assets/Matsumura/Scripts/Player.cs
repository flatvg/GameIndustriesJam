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
        // オブジェクトの座標取得
        Vector2 pos = gameObject.transform.position;

        // 入力は左クリックで射撃、マウスカーソルに向かってプレイヤーが向かっていく、スキルは多分キーボードの入力
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dt = Time.deltaTime;

        // マウスとプレイヤーとの距離を計算
        Vector2 distance =  mousePos - pos;
        float length = distance.magnitude;
        Vector2 dir = distance.normalized;

        // マウスとの距離が遠いほどスピードをあげるためただし、１まで
        length = Mathf.Clamp(length, 0, 1);

        // 角度を変更
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);

        UpdateShot();

        pos += dir * 4 * length * dt;
        gameObject.transform.position = pos;
    }

    private void UpdateShot()
    {
        isShot = false;

        // マウス左クリックで射撃
        if(Input.GetMouseButtonDown(0))
        {
            isShot = true;
        }
    }
}
