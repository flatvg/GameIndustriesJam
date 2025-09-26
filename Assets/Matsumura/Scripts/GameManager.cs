using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool isStart = false;

    public Player playerComp;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (playerComp == null)
        {
            Debug.Log("playerComp is null");
            return;
        }

        if(playerComp.isDeath)
        {
            SceneManager.LoadScene("Result");
        }
    }
}
