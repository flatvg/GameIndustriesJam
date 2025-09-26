using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectTwoPoints : MonoBehaviour
{
    public Sprite lineSprite; // スプライト画像をインスペクターから設定
    public Material UVMaterial;

    public void CreateLineBetween(Vector2 start, Vector2 end)
    {
        //// 新しい GameObject を作成
        //GameObject lineObject = new GameObject("Line");

        //// スプライトレンダラー追加
        //SpriteRenderer sr = lineObject.AddComponent<SpriteRenderer>();
        //sr.sprite = lineSprite;

        //// 位置設定（中央に配置）
        //Vector2 center = (start + end) / 2f;
        //lineObject.transform.position = center;

        //// 角度設定
        //Vector2 direction = end - start;
        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //lineObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        //// 長さに合わせてスケーリング
        //float length = direction.magnitude;
        //lineObject.transform.localScale = new Vector3(length, 1, 1); // スプライトの横幅が1ならOK



        GameObject lineObject = new GameObject("Line");
        var sr = lineObject.AddComponent<SpriteRenderer>();
        var thunder = lineObject.AddComponent<Thunder>();
        sr.sprite = lineSprite;

        // UVスクロール設定
        if (UVMaterial != null)
        {
            sr.material = UVMaterial;
        }

        Vector2 center = (start + end) * 0.5f;
        lineObject.transform.position = center;

        Vector2 direction = end - start;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        float length = direction.magnitude;

        // スプライトの“元のワールド幅/高さ”
        Vector2 spriteSize = sr.sprite.bounds.size;
        // 目標の太さ
        float targetThickness = spriteSize.y * 0.75f; // そのままなら1倍、細くしたいなら任意の値

        // 横方向：目標長さ / 元幅、縦方向：目標太さ / 元高さ
        lineObject.transform.localScale = new Vector3(
            length / Mathf.Max(1e-6f, spriteSize.x),
            targetThickness / Mathf.Max(1e-6f, spriteSize.y),
            1f
        );

        //カプセルコライダー生成、見た目と合わせる
        var col = lineObject.AddComponent<CapsuleCollider2D>();
        col.isTrigger = true;
        col.direction = CapsuleDirection2D.Horizontal;
        col.size = sr.sprite.bounds.size;
        col.offset = Vector2.zero;
    }
}
