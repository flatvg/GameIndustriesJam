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

    // �N���b�N�ŏI��
    public void Quit()
    {
        Debug.Log("Quit clicked");

#if UNITY_EDITOR
        // �G�f�B�^�Đ����~�߂�
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ���s�t�@�C���ł̓A�v���I��
        Application.Quit();
#endif
    }
}
