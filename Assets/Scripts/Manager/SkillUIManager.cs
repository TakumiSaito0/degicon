using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [Header("UI参照")]
    [SerializeField] private Image backgroundWhite;
    [SerializeField] private Image backgroundBlack;
    [SerializeField] private Image[] skillIconsWhite; // 赤青黄緑（白モード用）
    [SerializeField] private Image[] skillIconsBlack; // 赤青黃緑（黒モード用）
    [SerializeField] private Text[] cooldownTextsWhite; // 赤青黄緑（白モード用）
    [SerializeField] private Text[] cooldownTextsBlack; // 赤青黃緑（黒モード用）
    [SerializeField] private Text modeText;
    

    private ModeType currentMode = ModeType.White;
    private float[,] cooldowns = new float[2, 4]; // [mode, color]
    private float[,] cooldownMax = new float[2, 4] { {4,5,5,5}, {5,5,5,5} }; // 例: 全スキル5秒

    void Start()
    {
        // Qキーで切り替えるのでボタンイベントは不要
        UpdateUI();
    }

    void Update()
    {
        // Qキーで白黒切り替え
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
        {
            ChangeMode();
        }
        // クールタイム減算
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
            // スキル発動処理
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
        // 背景切替
        backgroundWhite.gameObject.SetActive(currentMode == ModeType.White);
        backgroundBlack.gameObject.SetActive(currentMode == ModeType.Black);
        // スキルアイコン切替
        for (int i = 0; i < 4; i++)
        {
            skillIconsWhite[i].gameObject.SetActive(currentMode == ModeType.White);
            skillIconsBlack[i].gameObject.SetActive(currentMode == ModeType.Black);
            cooldownTextsWhite[i].gameObject.SetActive(currentMode == ModeType.White);
            cooldownTextsBlack[i].gameObject.SetActive(currentMode == ModeType.Black);
        }
        modeText.text = currentMode == ModeType.White ? "白" : "黒";
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
