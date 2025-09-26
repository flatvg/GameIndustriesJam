using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BulletPoint : MonoBehaviour
{
    [Header("リング設定")]
    [Min(0f)] public float radius = 2.0f;                  // プレイヤーからの距離（回転時）
    [SerializeField, Min(1)] int pointCount = 5;           // 弾の数（可変）

    [SerializeField] float angleSpeed = 100.0f;            // 親の回転速度(Z)
    public GameObject bulletPrefab;                        // 生成する弾

    [SerializeField] float spriteAlignDeg = 270f;          // スプライトに適用するz軸のオフセット
    [SerializeField] bool isDrawDebugTriangle = false;     // デバッグ用三角形描画フラグ

    readonly List<Transform> points = new();               // リング上ポイント
    readonly List<Bullet> bullets = new();                 // 生成した弾
    private float rotDeg;                                  // 累積回転（必要なら使用）

    // 直近値（変更検知用）
    int lastPointCount;
    float lastRadius;
    bool lastIsDrawDebugTriangle;

    void Awake()
    {
        RebuildRing();     // 初期生成
        HandleDebugTriangle(true);
        lastPointCount = pointCount;
        lastRadius = radius;
        lastIsDrawDebugTriangle = isDrawDebugTriangle;
    }

    void Update()
    {
        // 親オブジェクトを回す（従来通り）
        transform.Rotate(0f, 0f, angleSpeed * Time.deltaTime);

        // インスペクタやコードからの変更を検知して反映
        if (pointCount != lastPointCount)
        {
            RebuildRing();
            lastPointCount = pointCount;
        }
        else if (!Mathf.Approximately(radius, lastRadius))
        {
            UpdatePointPositions();
            lastRadius = radius;
        }

        // デバッグ用三角形描画制御
        HandleDebugTriangle();
    }

    void RebuildRing()
    {
        // 既存の弾とポイントを整理
        for (int i = 0; i < bullets.Count; i++)
        {
            if (bullets[i]) Destroy(bullets[i].gameObject);
        }
        bullets.Clear();

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i]) Destroy(points[i].gameObject);
        }
        points.Clear();

        // 新規にポイントと弾を作成
        for (int i = 0; i < pointCount; i++)
        {
            var pt = new GameObject($"BulletPoint_{i}").transform;
            pt.SetParent(transform, false);
            points.Add(pt);
        }
        UpdatePointPositions(); // 半径に応じてリング配置

        // 弾の生成とバインド
        for (int i = 0; i < pointCount; i++)
        {
            var pt = points[i];
            var obj = Instantiate(bulletPrefab, pt.position, pt.rotation);
            // 回転にz軸に対してオフセットを適用
            //obj.transform.rotation *= Quaternion.Euler(0f, 0f, spriteAlignDeg);
            var b = obj.GetComponent<Bullet>();
            if (b != null)
            {
                b.bindPoint = pt; // 回転状態で追従
                b.isShot = false; // 初期は回転状態
                bullets.Add(b);
            }
        }
    }

    void UpdatePointPositions()
    {
        if (points.Count == 0) return;

        float step = 360f / Mathf.Max(1, pointCount);

        // 親の現在角度
        float parentZ = transform.eulerAngles.z;

        for (int i = 0; i < points.Count; i++)
        {
            // ワールドでの“放射角”を計算（親の回転はここでは入れない）
            float worldAngle = i * step; // 必要なら基準オフセットを足す

            // 位置はローカルで円配置（親が回れば一緒に回る）
            float rad = worldAngle * Mathf.Deg2Rad;
            Vector3 local = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            points[i].localPosition = local;

            // 向き：ワールドで worldAngle を向かせたいので、
            // 子の localRotation = worldAngle - 親角 + スプライト補正
            float localFace = worldAngle - parentZ + spriteAlignDeg;
            points[i].localRotation = Quaternion.Euler(0, 0, localFace);
        }
    }

    //void TryShotFromClick(Vector2 clickScreenPos)
    //{
    //    // UI 上は無視（任意）
    //    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

    //    var cam = Camera.main;
    //    if (!cam) return;

    //    // 自分のスクリーン座標
    //    Vector3 selfScreen = cam.WorldToScreenPoint(transform.position);
    //    Vector2 dirScreen = clickScreenPos - (Vector2)selfScreen;
    //    if (dirScreen.sqrMagnitude < 1e-8f) return;

    //    // スクリーン → ワールド（XY前提）
    //    float zDist = Mathf.Abs(transform.position.z - cam.transform.position.z);
    //    Vector3 clickWorld = cam.ScreenToWorldPoint(new Vector3(clickScreenPos.x, clickScreenPos.y, zDist));
    //    Vector2 dirWorld = (Vector2)(clickWorld - transform.position);

    //    Shot(dirWorld);
    //}

    // デバッグ用三角形描画制御
    void HandleDebugTriangle(bool forceChange = false)
    {
        if (lastIsDrawDebugTriangle != isDrawDebugTriangle || forceChange)
        {
            lastIsDrawDebugTriangle = isDrawDebugTriangle;
            foreach (Transform t in GetComponentInChildren<Transform>(true))
            {
                if (t == transform) continue; // 自身は除外
                SpriteRenderer renderer = t.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = isDrawDebugTriangle;
                }
            }
        }
    }

    // 最も近い未発射弾を撃つ
    public void Shot(Vector2 direction)
    {
        if (bullets.Count == 0) return;
        if (direction.sqrMagnitude < Mathf.Epsilon) return;

        Vector2 center = transform.position;
        Vector2 targetPos = center + direction.normalized * radius;

        float best = float.MaxValue;
        Bullet pick = null;

        foreach (var b in bullets)
        {
            if (!b || b.isShot) continue;
            float dist = Vector2.Distance(targetPos, b.transform.position);
            if (dist < best) { best = dist; pick = b; }
        }

        if (pick != null)
        {
            Vector2 d = center - targetPos;
            float deg = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            if (deg < 0f) deg += 360f;
            pick.Shot(direction.normalized, deg);
        }
    }

    // --- ギズモ（任意） ---
    void OnDrawGizmosSelected()
    {
        // 円
        Gizmos.color = Color.cyan;
        const int seg = 60;
        Vector3 prev = transform.position + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= seg; i++)
        {
            float t = (float)i / seg * Mathf.PI * 2f;
            Vector3 curr = transform.position + new Vector3(Mathf.Cos(t) * radius, Mathf.Sin(t) * radius, 0f);
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }
        // 点
        Gizmos.color = Color.yellow;
        float step = 360f / Mathf.Max(1, pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            float rad = (i * step) * Mathf.Deg2Rad;
            Vector3 p = transform.position + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            Gizmos.DrawWireSphere(p, 0.05f);
        }
    }
}