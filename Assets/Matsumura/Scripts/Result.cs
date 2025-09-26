using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Result : MonoBehaviour
{
    private List<RectTransform> rects = new List<RectTransform>();

    [SerializeField] private List<Button> buttons = new List<Button>();
    // Start is called before the first frame update
    void Start()
    {
        foreach (var button in buttons)
        {
            rects.Add(button.GetComponent<RectTransform>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        int num = 0;
        foreach(var rect in rects)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos/*Camera.main.ScreenToWorldPoint(Input.mousePosition)*/))
            {
                Debug.Log("rect" + num);
            }
            ++num;
        }
    }
}
