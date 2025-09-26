using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    [SerializeField]
    public string sceneName;

    //[SerializeField] private SceneTransition sceneTransition;

    private void Awake()
    {
        //設定されているシーンが存在するか確認
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("sceneNameが設定されていません。");
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"シーン'{sceneName}'は存在しません。シーン名を確認するか、ビルド設定にシーンを追加してください。");
            return;
        }
    }

    // Startは最初のフレームが更新される前に呼び出される
    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(Change);
        }
    }

    // 更新は1フレームにつき1回呼び出される
    void Update()
    {

    }

    //クリックされたら
    private void Change()
    {
        Debug.Log("OnClick");

        SceneManager.LoadScene(sceneName);
        return;

        //if (string.IsNullOrEmpty(sceneName)) return;

        //sceneTransition.ChangeScene(
        //    sceneName
        //);

        //UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
