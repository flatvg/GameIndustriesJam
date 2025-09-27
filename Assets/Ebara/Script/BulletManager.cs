using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BulletManager : MonoBehaviour
{
    [Header("リング設定")]
    [Min(0f)] public float radius = 2.0f;                  // プレイヤーからの距離（回転時）
    [SerializeField, Min(1)] int pointCount = 5;           // 弾の数（可変）
    public int bulletDamage = 1;                               // 弾のダメージ

    [SerializeField] float angleSpeed = 100.0f;            // 親の回転速度(Z)
    public GameObject bulletPrefab;                        // 生成する弾
    public Player player;                                  // プレイヤー
    public EnemySpawnaer spawnaer;
    public int chainCount = 5;
    public float thunderInterval = 0.05f;
    Coroutine chainRoutine;

    [SerializeField] float spriteAlignDeg = 270f;          // スプライトに適用するz軸のオフセット
    [SerializeField] bool isDrawDebugTriangle = false;     // デバッグ用三角形描画フラグ

    readonly List<Transform> points = new();               // リング上ポイント
    public List<Bullet> bullets = new();                 // 生成した弾

    private float rot;                                     // 累積角

    List<Bullet> bulletBuffer = new(); // スキルで使用するバレット

    public GameObject beamPrefab;

    private ConnectTwoPoints connecter;

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

        connecter = GetComponent<ConnectTwoPoints>();
    }

    void Update()
    {
        // プレイヤーに追従
        if (player != null)
            transform.position = player.transform.position;

        // 親オブジェクトを回す（従来通り）
        transform.Rotate(0.0f, 0.0f, angleSpeed * Time.deltaTime);

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

        if (Input.GetKeyDown(KeyCode.A))
        {
            //Vector2 start = new Vector2(player.transform.position.x, player.transform.position.y);
            //Vector2 end = start + (player.direction * 5);
            //connecter.CreateLineBetween(start, end);
            //GetComponent<ScreenFlash>().FlashSeconds(0.03f, 0.08f); // テスト成功
            //UseSkill2_2(); // テスト成功
            //UseSkill3_3(); // テスト成功
            UseSkill5_5();
        }
    }

    // 回転位置を再生成
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
                b.manager = this;      // マネージャーをセット
                b.bindPoint = pt;      // 回転状態で追従
                b.isShot = false;      // 初期は回転状態
                bullets.Add(b);
            }
        }
    }

    // 回転位置を更新
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

    private bool IsUseSkill(int level, int count)
    {
        int c = 0;
        List<Bullet> skillBullets = new List<Bullet>();
        foreach (var bullet in bullets)
        {
            if (bullet.level >= level)
            {
                skillBullets.Insert(c++, bullet);
            }
        }
        if (c < count) return false;

        for (int i = 0; i < count; i++)
        {
            var v = skillBullets[i];
            if (v != null)
            {
                v.level = 1;
            }
        }
        skillBullets.Clear();

        return true;
    }

    // スキル(強力な攻撃)を使用
    public void UseSkill2_2()
    {
        if (!IsUseSkill(2, 2)) return;

        Vector2 targetPos = (Vector2)player.transform.position + (player.direction * radius);

        float a = Mathf.Atan2(player.direction.y, player.direction.x) * Mathf.Rad2Deg;
        a -= 90f;
        GameObject obj = GameObject.Instantiate(beamPrefab, targetPos, Quaternion.Euler(0f, 0f, a));
        BeamBullet b = obj.GetComponent<BeamBullet>();
        b.manager = this;
    }

    public void UseSkill3_3()
    {
        if (!IsUseSkill(3, 3)) return;

        Vector2 targetPos = (Vector2)player.transform.position + (player.direction * radius);

        // 伝播リスト（最初はプレイヤー）
        List<Transform> chainPoints = new List<Transform> { player.transform };

        // 画面内の敵を Transform リストへ（Transform／Component／GameObject 何でも対応）
        var raw = spawnaer.GetInScreenEnemyes() as System.Collections.IEnumerable;
        var candidates = new List<Transform>();
        if (raw != null)
        {
            foreach (var e in raw)
            {
                if (e is Transform t) candidates.Add(t);
                else if (e is Component cpt && cpt) candidates.Add(cpt.transform);
                else if (e is GameObject go && go) candidates.Add(go.transform);
            }
        }
        if (candidates.Count == 0) return;

        // 伝播の流れを構築：毎回現在位置から半径内で最も近い敵を選ぶ
        Transform current = player.transform;
        float hopRadius = radius * 5;               // 1ホップの最大距離（必要なら調整）
        int maxJumps = chainCount;                  // 何回跳ねるか（必要数だけ）
        var used = new HashSet<Transform> { current };

        for (int j = 0; j < maxJumps; j++)
        {
            Transform next = null;
            float bestSq = hopRadius * hopRadius;

            for (int i = 0; i < candidates.Count; i++)
            {
                var t = candidates[i];
                if (t == null || used.Contains(t)) continue;

                float d2 = (t.position - current.position).sqrMagnitude;
                if (d2 <= bestSq)
                {
                    bestSq = d2;
                    next = t;
                }
            }

            if (next == null) break; // 半径内に対象なしで終了
            chainPoints.Add(next);
            used.Add(next);
            current = next;
        }

        // 敵には一応確定ダメージを与える
        foreach (Transform t in chainPoints)
        {
            var e = t.GetComponent<EnemyBase>();
            e?.TakeDamage(3, Vector2.zero);
        }

        // 各リンク間に可視ライン（雷）を生成
        for (int i = 0; i < chainPoints.Count - 1; i++)
        {
            Vector2 a = chainPoints[i].position;
            Vector2 b = chainPoints[i + 1].position;
            connecter.CreateLineBetween(a, b);
        }
    }

    public void UseSkill5_5()
    {
        //if (!IsUseSkill(5, 5)) return;

        GetComponent<ScreenFlash>().FlashSeconds(0.06f, 0.16f);

        var enemies = spawnaer.GetInScreenEnemyes();
        foreach (var enemy in enemies)
        {
            enemy.TakeDamage(5, Vector2.zero);
        }
    }

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
        if (bullets.Count == 0)
        {
            Debug.Log("Bullet Count Is 0.");
            return;
        }
        if (direction.sqrMagnitude < Mathf.Epsilon)
        {
            Debug.Log("Shot Direction is tiny.");
            return;
        }

        Vector2 center = transform.position;
        Vector2 targetPos = center + direction.normalized * radius;

        float best = float.MaxValue;
        Bullet pick = null;

        foreach (var b in bullets)
        {
            if (b.isShot) continue;
            float dist = Vector2.Distance(targetPos, (Vector2)b.transform.position);
            if (dist < best)
            {
                best = dist;
                pick = b;
            }
        }

        if (pick != null)
        {
            Vector2 d = center - targetPos;
            float deg = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            if (deg < 0f) deg += 360f;
            pick.Shot(direction.normalized, deg);
        }
        else
        {
            Debug.Log("Shotble Bullet Not Found.");
        }
    }

    // プレイヤー死亡時処理
    public void OnDeath()
    {
        foreach (var b in bullets)
        {
            if (b.isShot) continue;
            Vector2 d = b.transform.position - (b.transform.forward * radius);
            float deg = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            if (deg < 0f) deg += 360f;
            b.Shot(d, deg);
        }
    }

    // ギズモ描画
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