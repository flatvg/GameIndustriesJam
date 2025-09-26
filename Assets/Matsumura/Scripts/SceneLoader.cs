using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadNewScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    // �V�[���ǂݍ��݊֐�
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // ���[�h�J�n
        AsyncOperation async = SceneManager.LoadSceneAsync("Game");

        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            if (async.progress >= 0.9f)
            {
                Debug.Log("All ok");
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    async.allowSceneActivation = true;
                }

            }
        }

        return null;
    }
}
