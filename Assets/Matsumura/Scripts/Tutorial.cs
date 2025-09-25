using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    private bool changeSceneFlag = false;
    private SceneLoader loader;

    // Start is called before the first frame update
    void Start()
    {
        loader = GetComponent<SceneLoader>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!changeSceneFlag)
        {
            loader.LoadNewScene("Game");
        }
    }
}
