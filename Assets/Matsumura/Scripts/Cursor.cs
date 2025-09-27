using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    public Player playerComp;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.visible = false;   
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameCursor();
    }

    private void UpdateGameCursor()
    {
        if (playerComp != null)
        {
            if (Input.GetMouseButton(0))
            {
                transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            }
            else
                transform.localScale = new Vector3(1, 1, 1);

            if (playerComp.enableMove)
            {
                transform.GetChild(2).gameObject.SetActive(true);
                transform.GetChild(3).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(2).gameObject.SetActive(false);
                transform.GetChild(3).gameObject.SetActive(false);
            }

            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = -1;
            transform.position = position;

            Vector3 playerDir = playerComp.direction;
            // Šp“x
            float angle = Mathf.Atan2(playerDir.x, playerDir.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, -angle);
        }
    }
}
