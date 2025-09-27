using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamController : MonoBehaviour
{
    public float duration = 0.6f;

    private void OnEnable()
    {
        Invoke(nameof(DestroyBeam), duration);
    }

    private void DestroyBeam()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var player = col.GetComponent<Player>();
        if (player != null)
        {
            //ここでプレイヤーにダメージを与えるor即ゲームオーバーにする
            
        }
    }
}

