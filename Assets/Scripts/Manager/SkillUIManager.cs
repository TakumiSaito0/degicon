using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIManager : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [Header("UI参照")]
    [SerializeField] private Image backgroundWhite;
    [SerializeField] private Image backgroundBlack;
    [SerializeField] private Image[] skillIconsWhite; // 赤青黄緑（白モード用）
    [SerializeField] private Image[] skillIconsBlack; // 赤青黃緑（黒モード用）
    [SerializeField] private TextMeshProUGUI[] cooldownTextsWhite; // 赤青黄緑（白モード用）
    [SerializeField] private TextMeshProUGUI[] cooldownTextsBlack; // 赤青黃緑（黒モード用）
    // [SerializeField] private Text modeText; // モードテキスト削除
    

    private ModeType currentMode = ModeType.White;
    private float[,] cooldowns = new float[2, 4]; // [mode, color]
    private float[,] cooldownMax = new float[2, 4] { {15,20,10,15}, {7,15,5,70} }; // 例: 全スキル5秒

    void Start()
    {
        // Qキーで切り替えるのでボタンイベントは不要
        // 全スキルのクールタイムを0に初期化（ゲーム開始直後から使用可能）
        for (int m = 0; m < 2; m++)
        {
            for (int c = 0; c < 4; c++)
            {
                cooldowns[m, c] = 0f;
            }
        }
        UpdateUI();
    }

    void Update()
    {
        // Qキーで白黒切り替え
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
        {
            ChangeMode();
        }
        // Iキーで赤スキル発動（テスト用）
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Red);
        }
        // Lキーで青スキル発動（白青スキル用）
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Blue);
        }
        // Jキーで緑スキル発動（白緑スキル用）
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.jKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Green);
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
        // modeText.text = ... 削除
        UpdateCooldownUI();
    }

    // 黒緑スキルのUI使用不可表示
    public void SetBlackGreenSkillUnavailable()
    {
        int m = (int)ModeType.Black;
        int c = (int)ColorType.Green;
        // アイコンをグレー化
        skillIconsBlack[c].color = new Color(0.5f, 0.5f, 0.5f, 1f); // グレー
        // テキストを「使用不可」に
        cooldownTextsBlack[c].text = "使用不可";
    }

    // 黒緑スキルのUI表示を更新（近くにPlantがあれば緑、なければグレー）
    public void UpdateBlackGreenSkillIcon(bool canUse)
    {
        int c = (int)ColorType.Green;
        if (canUse)
        {
            // 緑色（例: Color.green）
            skillIconsBlack[c].color = Color.green;
            cooldownTextsBlack[c].text = "Ready";
        }
        else
        {
            // グレー
            skillIconsBlack[c].color = new Color(0.5f, 0.5f, 0.5f, 1f);
            cooldownTextsBlack[c].text = "使用不可";
        }
    }

    void UpdateCooldownUI()
    {
        int m = (int)currentMode;
        for (int i = 0; i < 4; i++)
        {
            // 黒緑スキルはクールタイム管理しない
            if (currentMode == ModeType.Black && i == (int)ColorType.Green)
            {
                // 何もせず（SetBlackGreenSkillUnavailableで制御）
                continue;
            }
            // 0未満になったら0に固定
            if (cooldowns[m, i] < 0) cooldowns[m, i] = 0;
            float cd = cooldowns[m, i];
            if (cd > 0)
            {
                // カウントダウン表示（整数秒）
                if (currentMode == ModeType.White)
                    cooldownTextsWhite[i].text = Mathf.CeilToInt(cd).ToString();
                else
                    cooldownTextsBlack[i].text = Mathf.CeilToInt(cd).ToString();
            }
            else
            {
                // Ready表示
                if (currentMode == ModeType.White)
                    cooldownTextsWhite[i].text = "Ready";
                else
                    cooldownTextsBlack[i].text = "Ready";
            }
        }
    }
}
