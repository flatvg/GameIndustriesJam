using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Thunder : MonoBehaviour
{
    public float lifeTime = 1.0f;
    public int damage = 3;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(HandleDectructor());
        // カメラシェイクを行う
    }


    private IEnumerator HandleDectructor()
    {
        yield return new WaitForSeconds(lifeTime);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
