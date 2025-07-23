using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI����p

public class LifeManager : MonoBehaviour
{
    public int health = 3; // �n�[�g��3��
    public int maxHealth = 3; // �ő�̗�

    // �n�[�g�摜�̔z��iInspector�Őݒ�j
    public Image[] heartImages;

    [SerializeField] private GameObject gameOverPanel; // Inspector�ŃQ�[���I�[�o�[UI���Z�b�g
   
    [SerializeField] private Light mainLight;

    // �ʏ�̃n�[�g�摜�iInspector�Őݒ�j
    public Sprite heartFullSprite;

    // �_���[�W���̃n�[�g�摜�iInspector�Őݒ�j
    public Sprite heartEmptySprite;

    // �V���O���g���C���X�^���X
    public static LifeManager instance;

    private float originalIntensity;

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

    void Start()
    {
        UpdateHearts();
        if (mainLight != null) originalIntensity = mainLight.intensity;
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
       
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // �Q�[���I�[�o�[UI��\��
        }
        if (mainLight != null) mainLight.intensity = 0.3f; // �Â�����
        Time.timeScale = 0f; // �Q�[�����~
    }
    // ���g���C�{�^������Ăяo��
    public void OnRetryButton()
    {
        if (mainLight != null) mainLight.intensity = originalIntensity; // ���ɖ߂�
        Time.timeScale = 1f; // ���Ԃ����ɖ߂�
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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