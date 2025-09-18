using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [Header("UI�Q��")]
    [SerializeField] private Image backgroundWhite;
    [SerializeField] private Image backgroundBlack;
    [SerializeField] private Image[] skillIconsWhite; // �Ԑ��΁i�����[�h�p�j
    [SerializeField] private Image[] skillIconsBlack; // �Ԑ��΁i�����[�h�p�j
    [SerializeField] private Text[] cooldownTextsWhite; // �Ԑ��΁i�����[�h�p�j
    [SerializeField] private Text[] cooldownTextsBlack; // �Ԑ��΁i�����[�h�p�j
    [SerializeField] private Text modeText;
    [SerializeField] private Button modeChangeButton;

    private ModeType currentMode = ModeType.White;
    private float[,] cooldowns = new float[2, 4]; // [mode, color]
    private float[,] cooldownMax = new float[2, 4] { {5,5,5,5}, {5,5,5,5} }; // ��: �S�X�L��5�b

    void Start()
    {
        modeChangeButton.onClick.AddListener(ChangeMode);
        UpdateUI();
    }

    void Update()
    {
        // �N�[���^�C�����Z
        for (int m = 0; m < 2; m++)
        {
            for (int c = 0; c < 4; c++)
            {
                if (cooldowns[m, c] > 0)
                    cooldowns[m, c] -= Time.deltaTime;
            }
        }
        UpdateCooldownUI();
    }

    public void UseSkill(ColorType color)
    {
        int m = (int)currentMode;
        int c = (int)color;
        if (cooldowns[m, c] <= 0)
        {
            cooldowns[m, c] = cooldownMax[m, c];
            // �X�L����������
            UpdateCooldownUI();
        }
    }

    void ChangeMode()
    {
        currentMode = currentMode == ModeType.White ? ModeType.Black : ModeType.White;
        UpdateUI();
    }

    void UpdateUI()
    {
        // �w�i�ؑ�
        backgroundWhite.gameObject.SetActive(currentMode == ModeType.White);
        backgroundBlack.gameObject.SetActive(currentMode == ModeType.Black);
        // �X�L���A�C�R���ؑ�
        for (int i = 0; i < 4; i++)
        {
            skillIconsWhite[i].gameObject.SetActive(currentMode == ModeType.White);
            skillIconsBlack[i].gameObject.SetActive(currentMode == ModeType.Black);
            cooldownTextsWhite[i].gameObject.SetActive(currentMode == ModeType.White);
            cooldownTextsBlack[i].gameObject.SetActive(currentMode == ModeType.Black);
        }
        modeText.text = currentMode == ModeType.White ? "��" : "��";
        UpdateCooldownUI();
    }

    void UpdateCooldownUI()
    {
        int m = (int)currentMode;
        for (int i = 0; i < 4; i++)
        {
            float cd = Mathf.Max(0, cooldowns[m, i]);
            if (currentMode == ModeType.White)
                cooldownTextsWhite[i].text = cd > 0 ? cd.ToString("F1") : "Ready";
            else
                cooldownTextsBlack[i].text = cd > 0 ? cd.ToString("F1") : "Ready";
        }
    }
}
