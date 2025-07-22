using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [SerializeField] private Player player; // Player参照
    [SerializeField] private GameObject effectPrefab; // エフェクトPrefab

    private ModeType currentMode = ModeType.White;

    private float lastSkillTime = -999f;
    private float skillCooldown = 5f;

    void Update()
    {
        // Qキーで白黒切り替え
        if (UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
        {
            currentMode = currentMode == ModeType.White ? ModeType.Black : ModeType.White;
            Debug.Log($"モード切替: 現在は{currentMode}");
        }

        // Iキー：赤
        if (UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Red);
        }
        // 他の色は省略
    }

    private void UseSkill(ColorType color)
    {
        if (currentMode == ModeType.White && color == ColorType.Red)
        {
            // クールタイム判定
            if (Time.time - lastSkillTime < skillCooldown)
            {
                Debug.Log("スキルはクールタイム中です");
                return;
            }

            lastSkillTime = Time.time;

            if (player != null)
            {
                player.attackPower = Mathf.RoundToInt(player.attackPower * 1.5f);
                Debug.Log($"白の赤魔法発動！攻撃力: {player.attackPower}");

                // エフェクト発動（1秒後に消す）
                if (effectPrefab != null)
                {
                    GameObject effect = Instantiate(effectPrefab, player.transform.position, Quaternion.identity);
                    Destroy(effect, 1f);
                    Debug.Log("白モードの赤魔法エフェクト発動！（1秒後に消滅）");
                }
                else
                {
                    Debug.Log("effectPrefabがアタッチされていません");
                }
            }
            else
            {
                Debug.Log("Player参照がありません");
            }
        }
        else
        {
            Debug.Log($"現在は{currentMode}の{color}を発動しました");
        }
    }
}