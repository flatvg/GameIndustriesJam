using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class BossBeamController : MonoBehaviour
{
    [Header("警告UI")]
    public Canvas warningCanvas;
    public Image verticalWarningPrefab;
    public Image horizontalWarningPrefab;

    [Header("ビームPrefab")]
    public GameObject verticalBeamPrefab;
    public GameObject horizontalBeamPrefab;

    private Image[] verticalWarnings = new Image[2];
    private Image[] horizontalWarnings = new Image[3];
    private Transform canvasTransform;

    private void Awake()
    {
        canvasTransform = warningCanvas.transform;

        // 縦
        for (int i = 0; i < 2; i++)
        {
            verticalWarnings[i] = Instantiate(verticalWarningPrefab, canvasTransform);
            RectTransform rt = verticalWarnings[i].rectTransform;
            rt.anchorMin = new Vector2(i * 0.5f, 0);
            rt.anchorMax = new Vector2((i + 1) * 0.5f, 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            verticalWarnings[i].gameObject.SetActive(false);
        }

        // 横
        for (int i = 0; i < 3; i++)
        {
            horizontalWarnings[i] = Instantiate(horizontalWarningPrefab, canvasTransform);
            RectTransform rt = horizontalWarnings[i].rectTransform;
            rt.anchorMin = new Vector2(0, i / 3f);
            rt.anchorMax = new Vector2(1, (i + 1) / 3f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            horizontalWarnings[i].gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowWarning(Image[] warnings, float duration)
    {
        foreach (var w in warnings) w.gameObject.SetActive(true);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            foreach (var w in warnings)
            {
                float alpha = Mathf.PingPong(Time.time * 2f, 0.5f) + 0.3f;
                w.color = new Color(1, 0, 0, alpha);
            }
            yield return null;
        }
        foreach (var w in warnings) w.gameObject.SetActive(false);
    }

    private GameObject SpawnBeam(GameObject prefab, RectTransform rect)
    {
        Vector3 worldPos = rect.position;
        var beam = Instantiate(prefab, worldPos, Quaternion.identity);
        //BeamDamage bd = beam.GetComponent<BeamDamage>();
        //if (bd != null) bd.Init(this); // ダメージスクリプトに初期化
        return beam;
    }

    public void FireVerticalBeam(int index)
    {
        SpawnBeam(verticalBeamPrefab, verticalWarnings[index].rectTransform);
    }

    public void FireHorizontalBeam(int index)
    {
        SpawnBeam(horizontalBeamPrefab, horizontalWarnings[index].rectTransform);
    }

    public void FireVerticalAndHorizontalBeam()
    {
        for (int i = 0; i < 2; i++) FireVerticalBeam(i);
        for (int i = 0; i < 3; i++) FireHorizontalBeam(i);
    }

    public IEnumerator FireBeamWithWarning(int loopNumber, float warnDuration, float beamDuration)
    {
        if (loopNumber == 1)
        {
            yield return ShowWarning(verticalWarnings, warnDuration);
            FireVerticalBeam(0);
            FireVerticalBeam(1);
        }
        else if (loopNumber == 2)
        {
            yield return ShowWarning(horizontalWarnings, warnDuration);
            for (int i = 0; i < 3; i++) FireHorizontalBeam(i);
        }
        else if (loopNumber == 3)
        {
            yield return ShowWarning(verticalWarnings, warnDuration);
            yield return ShowWarning(horizontalWarnings, warnDuration);
            FireVerticalAndHorizontalBeam();
        }
        yield return new WaitForSeconds(beamDuration);
    }
}
