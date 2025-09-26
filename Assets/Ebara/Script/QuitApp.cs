using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitApp : MonoBehaviour
{
    void Start()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(Quit);
        }
    }

    // クリックで終了
    public void Quit()
    {
        Debug.Log("Quit clicked");

#if UNITY_EDITOR
        // エディタ再生を止める
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 実行ファイルではアプリ終了
        Application.Quit();
#endif
    }
}
