using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSc : MonoBehaviour
{
    // Update is called once per frame
    public float margin = 0.2f; // ��ʊO�ɂǂꂭ�炢�]�T�������i�r���[�|�[�g���W�j

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // ��ʊO����
        if (IsOutOfScreenWithMargin())
        {
            Destroy(gameObject);
        }
    }

    bool IsOutOfScreenWithMargin()
    {
        // �J�������擾
        Camera cam = Camera.main;
        if (cam == null) return false;

        // ���[���h���W���r���[�|�[�g���W
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        // ��ʊO + margin ���ǂ���
        if (viewportPos.x < -margin || viewportPos.x > 1 + margin ||
            viewportPos.y < -margin || viewportPos.y > 1 + margin)
        {
            return true;
        }
        return false;
    }
}
