using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class BeamBullet : SkillBulletBase
{
    [SerializeField] private float duration = 2.0f; // 補間にかける秒数
    [SerializeField] private float targetY = 3.0f; // 目標のyスケール値
    private SpriteFadeOut fadeOut;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1); // 補間カーブ

    CapsuleCollider2D col;

    private void Awake()
    {
        col = GetComponent<CapsuleCollider2D>();
        col.direction = CapsuleDirection2D.Vertical;
        fadeOut = GetComponent<SpriteFadeOut>();
        Shot();
        fadeOut.Play();
    }

    protected override void Shot()
    {
        StartCoroutine(ScaleYCoroutine(targetY, duration));
    }

    protected override void Move()
    {
        if (bindPoint != null)
            transform.position = bindPoint.position;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if(renderer != null)
        {
            if (renderer.color.a < 0.1f)
                Destroy(gameObject);
        }
    }

    protected override void OnHitEnemy(EnemyBase enemy)
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        // 一定の透明度以下は当たり判定を行わない
        if (renderer.color.a > 0.3f)
        {
            Vector2 knockBack = isShot ? Vector2.zero : enemy.transform.position - manager.player.transform.position;
            enemy.TakeDamage(damage, knockBack);
        }
    }

    protected override void OnWithOutScreen()
    {
        // 何もしない
    }

    private IEnumerator ScaleYCoroutine(float targetY, float duration)
    {
        // 開始時の長さ＆中心から、回転後のローカル上方向に対して「根本」を計算する
        float startLen = transform.localScale.y;
        Vector3 startScale = transform.localScale;

        // ワールドでの根本位置（pivotが中央想定なので半分下げる）
        Vector3 rootWorld = transform.position /*- transform.up * (startLen * 0.5f)*/;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float k = curve.Evaluate(u);

            // 長さ（=ローカルYスケール相当）を補間
            float len = Mathf.LerpUnclamped(startLen, targetY, k);
            transform.localScale = new Vector3(startScale.x, len, startScale.z);

            // 回転後ローカル上方向に沿って中心を再配置（根本固定で伸ばす）
            transform.position = rootWorld + transform.up * len;

            yield return null;
        }
    }
}
