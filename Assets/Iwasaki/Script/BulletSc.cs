using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSc : MonoBehaviour
{
    // Update is called once per frame
    public float margin = 0.2f; // 画面外にどれくらい余裕を持つか（ビューポート座標）

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // 画面外判定
        if (IsOutOfScreenWithMargin())
        {
            Destroy(gameObject);
        }
    }

    bool IsOutOfScreenWithMargin()
    {
        // カメラを取得
        Camera cam = Camera.main;
        if (cam == null) return false;

        // ワールド座標→ビューポート座標
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        // 画面外 + margin かどうか
        if (viewportPos.x < -margin || viewportPos.x > 1 + margin ||
            viewportPos.y < -margin || viewportPos.y > 1 + margin)
        {
            return true;
        }
        return false;
    }
}
