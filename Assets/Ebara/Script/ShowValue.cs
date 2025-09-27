using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowValue : MonoBehaviour
{
    [SerializeField] TMP_Text label; // TextMeshProUGUI‚Å‚àOK
    private int score = 0;

    void Awake()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>(true); // Žq‚©‚çŽ©“®Žæ“¾
        if (!label) { Debug.LogError("TMP_Text‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ‚Å‚µ‚½"); enabled = false; return; }
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
