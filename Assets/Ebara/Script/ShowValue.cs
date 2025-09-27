using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowValue : MonoBehaviour
{
    [SerializeField] TMP_Text label; // TextMeshProUGUI�ł�OK
    private int score = 0;

    void Awake()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>(true); // �q���玩���擾
        if (!label) { Debug.LogError("TMP_Text��������܂���ł���"); enabled = false; return; }
    }

    void Start()
    {
        UpdateScoreText();
    }

    public void SetScore(int s)
    {
        score = s;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        label.SetText("Score: {0}", GameSession.Instance.attackScore + GameSession.Instance.timeScore);
    }
}
