using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDeath : MonoBehaviour
{
    [SerializeField] int circleCount = 10;

    private Player playerComp;
    private bool onDeath = false;
    private ParticleSystem effect;
    private Vector3 originalScale;
    private Vector3 peakScale;
    private float deltaTime = 0;
    private float duration = 0.5f;

    [Header("���S���Ɍ��̑傫���̉��{�ɂȂ�̂��̒l")] public float correctionValue = 2f;

    public ParticleSystem particlePrefab;
    public bool isDied = false; // ���S����������������

    // Start is called before the first frame update
    void Start()
    {
        playerComp = GetComponent<Player>();

        originalScale = transform.localScale;
        peakScale = originalScale * correctionValue;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerComp != null)
        {

            // ����ōŏ������ʂ�
            if (!onDeath)
            {
                if (playerComp.isDeath)
                {
                    playerComp.manaComp.OnDeath();
                    effect = Instantiate(particlePrefab, transform.position, Quaternion.Euler(90f, 0f, 0f));
                    effect.Play();
                    onDeath = true;
                }
            }
            else
            {
                deltaTime += Time.deltaTime;
                float t = deltaTime / duration;
                transform.localScale = Vector3.Lerp(peakScale, Vector3.zero, t);

            }

            // �G�t�F�N�g��������
            if (effect != null)
            {
                Destroy(effect.gameObject, effect.main.duration + 2);
                isDied = true;
            }
        }
    }
}
