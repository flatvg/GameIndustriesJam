using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool isStart = false;
    private float exitTimer = 5;
    private float tick = 0f;

    public OnDeath deathComp;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (deathComp == null)
        {
            Debug.Log("playerComp is null");
            return;
        }

        if(deathComp.isDied)
        {
            exitTimer -= Time.deltaTime;

            if(exitTimer < 0)
                SceneManager.LoadScene("Result");
        }

        tick += Time.deltaTime;

        while (tick >= 1f)
        {
            tick -= 1f;
            GameSession.Instance.timeScore++;
        }
    }
}
