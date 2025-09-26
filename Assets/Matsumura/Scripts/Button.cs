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
        //�ݒ肳��Ă���V�[�������݂��邩�m�F
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("sceneName���ݒ肳��Ă��܂���B");
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"�V�[��'{sceneName}'�͑��݂��܂���B�V�[�������m�F���邩�A�r���h�ݒ�ɃV�[����ǉ����Ă��������B");
            return;
        }
    }

    // Start�͍ŏ��̃t���[�����X�V�����O�ɌĂяo�����
    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(Change);
        }
    }

    // �X�V��1�t���[���ɂ�1��Ăяo�����
    void Update()
    {

    }

    //�N���b�N���ꂽ��
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
