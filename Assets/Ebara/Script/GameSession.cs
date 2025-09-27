using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    // シーンをまたいで持ちたい値
    public int timeScore = 0;
    public int attackScore = 0;
    public int totalScore = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // スコアリセット
    public void ResetScore()
    {
        timeScore = 0;
        attackScore = 0;
        totalScore = 0;
    }

    // おまけ：シーン遷移のフック
    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    void OnSceneLoaded(Scene s, LoadSceneMode m) { /* 新シーン初期化 */ }
}
