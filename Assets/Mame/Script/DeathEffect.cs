using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [SerializeField]
    public float scaleSpeed = 5.0f;
    [SerializeField]
    public float minScale = 0.0f;
    [SerializeField]
    public float maxScale = 0.5f;

    bool isGenerate = false;
    float alpha = 1.0f;

    [SerializeField]
    public float deleteSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.localScale = new Vector2(minScale, minScale);

        alpha  = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // ê∂ê¨ èôÅXÇ…ëÂÇ´Ç≠Ç∑ÇÈ
        if (isGenerate == false)
        {
            float scale = gameObject.transform.localScale.x;
            scale += scaleSpeed * Time.deltaTime;

            if (scale >= maxScale)
            {
                scale = maxScale;
                isGenerate = true;
            }

            gameObject.transform.localScale = new Vector2(scale, scale);
        }

        // è¡ñ≈ èôÅXÇ…ìßñæÇ…Ç»ÇÈ
        alpha -= deleteSpeed * Time.deltaTime;
        if (alpha <= 0.0f) alpha = 0.0f;
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
    }
}
