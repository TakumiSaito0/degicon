using UnityEngine;
using UnityEngine.UI; // UI����p

public class LifeManager : MonoBehaviour
{
    public int health = 3; // �n�[�g��3��
    public int maxHealth = 3; // �ő�̗�

    // �n�[�g�摜�̔z��iInspector�Őݒ�j
    public Image[] heartImages;

    // �ʏ�̃n�[�g�摜�iInspector�Őݒ�j
    public Sprite heartFullSprite;

    // �_���[�W���̃n�[�g�摜�iInspector�Őݒ�j
    public Sprite heartEmptySprite;

    // �V���O���g���C���X�^���X
    public static LifeManager instance;

    void Awake()
    {
        // �V���O���g���p�^�[���̎���
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (health <= 0) return; // ���Ɏ��S���Ă���ꍇ�͉������Ȃ�

        health -= damage;
        health = Mathf.Max(health, 0);

        UpdateHearts();

        Debug.Log($"�_���[�W���󂯂܂����B���݂̗̑�: {health}");

        // �̗͂�0�ɂȂ����ꍇ�̏���
        if (health <= 0)
        {
            OnPlayerDeath();
        }
    }

    public void Heal(int healAmount = 1)
    {
        health += healAmount;
        health = Mathf.Min(health, maxHealth);
        UpdateHearts();
        Debug.Log($"�񕜂��܂����B���݂̗̑�: {health}");
    }

    void OnPlayerDeath()
    {
        Debug.Log("�v���C���[�����S���܂���");
        // �����ɃQ�[���I�[�o�[������ǉ�
    }

    // �n�[�g�摜�̕\���X�V
    void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < health)
            {
                heartImages[i].sprite = heartFullSprite;
            }
            else
            {
                heartImages[i].sprite = heartEmptySprite;
            }
        }
    }

    void Start()
    {
        UpdateHearts(); // �����\��
    }

    // ���݂̗̑͂��擾
    public int GetHealth()
    {
        return health;
    }

    // �̗͂����^�����`�F�b�N
    public bool IsFullHealth()
    {
        return health >= maxHealth;
    }

    // ���S���Ă��邩�`�F�b�N
    public bool IsDead()
    {
        return health <= 0;
    }
}