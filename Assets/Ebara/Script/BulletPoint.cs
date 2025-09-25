using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// このスクリプトをプレイヤーにアッタチする想定
public class BulletPoint : MonoBehaviour
{
    [Header("配置パラメータ")]
    [Min(0f)] public float radius = 2.0f;                 // 円の半径
    [Range(0f, 360f)] public float startOffsetDeg = 0f;   // 最初の点のオフセット角
    public bool alignToFacing = true;                     // プレイヤーのZ回転に合わせる

    [SerializeField] Vector2 shotDirection = Vector2.zero;

    [Header("回転パラメータ")]
    public float angularSpeedDeg = 90f; // 1秒あたりの回転角（+で反時計回り）

    [SerializeField] Vector2 shotPos = Vector2.zero;

    const float StepDeg = 72f; // 72度間隔（=5点）
    const int PointCount = 5;

    private List<Bullet> bullets;     // 生成した弾を保持
    private float _rotDeg;            // 累積回転角

    public GameObject bulletPrefab;

    void Awake()
    {
        bullets = new List<Bullet>(PointCount);
    }

    void Start()
    {
        if (!bulletPrefab)
        {
            Debug.LogError("[BulletPoint] bulletPrefab が未設定っス！");
            return;
        }

        // 最初の配置で弾を生成
        var points = GetPoints2D();
        for (int i = 0; i < points.Count; i++)
        {
            var pos3 = new Vector3(points[i].x, points[i].y, 0f);
            GameObject obj = Instantiate(bulletPrefab, pos3, Quaternion.identity);
            Bullet bullet = obj.GetComponent<Bullet>();
            // Bullet コンポが無くても動かしたいなら Transform を直接保持でもOKっス
            if (bullet != null) bullets.Add(bullet);
            else bullets.Add(null); // スロット数を合わせる
        }
    }

    void Update()
    {
        // 角度を積算して0-360に正規化
        _rotDeg = Mathf.Repeat(_rotDeg + angularSpeedDeg * Time.deltaTime, 360f);

        var points = GetPoints2D();

        // 生成済みの弾を新しい円周上の位置に移動
        for (int i = 0; i < points.Count && i < bullets.Count; i++)
        {
            if (bullets[i] == null) continue;
            var t = bullets[i].transform;
            t.position = new Vector3(points[i].x, points[i].y, t.position.z);
            // 向きも回したければ↓を適宜使うっス（任意）
            // t.right = (t.position - transform.position).normalized; // 接線/放射方向など好みで
        }
    }

    public void Shot(Vector2 direction)
    {
        Vector2 targetPos = new Vector2(transform.position.x, transform.position.y) + (direction.normalized * radius);
    }

    /// <summary>
    /// 円周上の座標を返す（72度間隔、回転反映、2D）
    /// </summary>
    public List<Vector2> GetPoints2D()
    {
        var result = new List<Vector2>(PointCount);

        Vector2 center = transform.position;

        // 基準角（開始オフセット + 累積回転 + プレイヤーの向き）
        float baseDeg = startOffsetDeg + _rotDeg;
        if (alignToFacing) baseDeg += transform.eulerAngles.z;

        for (int i = 0; i < PointCount; i++)
        {
            float deg = baseDeg + i * StepDeg;   // 0,72,144,216,288 (+オフセット)
            float rad = deg * Mathf.Deg2Rad;

            float x = center.x + radius * Mathf.Cos(rad);
            float y = center.y + radius * Mathf.Sin(rad);
            result.Add(new Vector2(x, y));
        }
        return result;
    }

    // シーン上で可視化（オプション）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var points = Application.isPlaying ? GetPoints2D() : PreviewPointsInEditor();
        foreach (var p in points)
        {
            Gizmos.DrawWireSphere(new Vector3(p.x, p.y, 0f), 0.05f);
        }
        DrawCircleGizmo();
    }

    // エディタプレビュー用（再生前は回転0でプレビュー）
    List<Vector2> PreviewPointsInEditor()
    {
        float saved = _rotDeg;
        _rotDeg = 0f;
        var pts = GetPoints2D();
        _rotDeg = saved;
        return pts;
    }

    void DrawCircleGizmo()
    {
        const int seg = 60;
        Vector3 prev = transform.position + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= seg; i++)
        {
            float t = (float)i / seg * Mathf.PI * 2f;
            Vector3 curr = transform.position + new Vector3(Mathf.Cos(t) * radius, Mathf.Sin(t) * radius, 0f);
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }
    }
}
