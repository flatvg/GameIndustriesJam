using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectTwoPoints : MonoBehaviour
{
    public Sprite lineSprite; // スプライト画像をインスペクターから設定

    public void CreateLineBetween(Vector2 start, Vector2 end)
    {
        // 新しい GameObject を作成
        GameObject lineObject = new GameObject("Line");

        // スプライトレンダラー追加
        SpriteRenderer sr = lineObject.AddComponent<SpriteRenderer>();
        sr.sprite = lineSprite;

        // 位置設定（中央に配置）
        Vector2 center = (start + end) / 2f;
        lineObject.transform.position = center;

        // 角度設定
        Vector2 direction = end - start;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 長さに合わせてスケーリング
        float length = direction.magnitude;
        lineObject.transform.localScale = new Vector3(length, 1, 1); // スプライトの横幅が1ならOK

        var thounder = lineObject.AddComponent<Thunder>();

        var col = lineObject.AddComponent<CapsuleCollider2D>();
        col.isTrigger = true;
        col.direction = CapsuleDirection2D.Horizontal;
        col.size = sr.sprite.bounds.size;
        col.offset = Vector2.zero;
    }
}
