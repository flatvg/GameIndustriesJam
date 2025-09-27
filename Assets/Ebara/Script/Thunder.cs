using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Thunder : MonoBehaviour
{
    public float lifeTime = 1.5f;
    public int damage = 3;


    [Header("フェード設定")]
    [Min(0.01f)] public float fadeDuration = 0.5f; // 消える直前のフェード時間
    public bool disableColliderOnFade = true;      // フェード中に当たり判定を止める

    SpriteRenderer[] _spriteRenderers;
    Collider2D[] _colliders;

    void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        _colliders = GetComponentsInChildren<Collider2D>(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(HandleDectructor());
    }


    private IEnumerator HandleDectructor()
    {
        yield return new WaitForSeconds(lifeTime);

        // フェード中は当たり判定を無効化（任意）
        if (disableColliderOnFade && _colliders != null)
        {
            foreach (var c in _colliders) if (c) c.enabled = false;
        }

        // 透明度を下げる
        yield return StartCoroutine(FadeOut());

        Destroy(gameObject);
    }

    private IEnumerator FadeOut()
    {
        if (fadeDuration <= 0f) yield break;

        // 初期カラーを取得
        var startColors = new Dictionary<SpriteRenderer, Color>(_spriteRenderers.Length);
        foreach (var sr in _spriteRenderers)
        {
            if (sr) startColors[sr] = sr.color;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            foreach (var kv in startColors)
            {
                var sr = kv.Key;
                var baseCol = kv.Value;
                if (sr) sr.color = new Color(baseCol.r, baseCol.g, baseCol.b, a);
            }
            yield return null;
        }

        // 最終的に完全に透明へ
        foreach (var kv in startColors)
        {
            var sr = kv.Key;
            var baseCol = kv.Value;
            if (sr) sr.color = new Color(baseCol.r, baseCol.g, baseCol.b, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Boss")
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, Vector2.zero);
            }
        }

        if (collision.gameObject.tag == "Enemy")
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                Vector2 knockBack = enemy.transform.position - transform.position;
                enemy.TakeDamage(damage, knockBack);
            }
        }
    }
}
