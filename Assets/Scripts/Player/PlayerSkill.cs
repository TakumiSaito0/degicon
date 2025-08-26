using UnityEngine;
using System.Collections;

public class PlayerSkill : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [SerializeField] private Player player; // Player参照
    [SerializeField] private GameObject fireEffectPrefab; // 黒赤スキル用エフェクト
    [SerializeField] private GameObject whiteRedEffectPrefab; // 白赤スキル用エフェクト

    private ModeType currentMode = ModeType.White;
    private float lastWhiteRedSkillTime = -4f; // 白赤スキルのクールタイム管理（初期値を-クールタイムに）
    private float lastBlackRedSkillTime = -7f; // 黒赤スキルのクールタイム管理（初期値を-クールタイムに）

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
            const float whiteRedSkillCooldown = 4f;
            const float blackRedSkillCooldown = 7f;

            if (player != null && whiteRedEffectPrefab != null)
            {
                // クールタイム判定
                if (Time.time - lastWhiteRedSkillTime < whiteRedSkillCooldown)
                {
                    Debug.Log("白赤スキルはクールタイム中です");
                    return;
                }
                lastWhiteRedSkillTime = Time.time;

                // 攻撃力アップ（5秒間のみ）
                int originalAttackPower = player.attackPower;
                player.attackPower = Mathf.RoundToInt(originalAttackPower * 1.5f);
                Debug.Log($"白の赤魔法発動！攻撃力: {player.attackPower}（5秒間アップ）");

                StartCoroutine(ResetAttackPowerAfterDelay(blackRedSkillCooldown, originalAttackPower));

                // 白モードの赤魔法エフェクト発動（Y座標を1f下げて生成、1秒後に消す）
                Vector3 spawnPos = player.transform.position + new Vector3(0, 0f, 0);
                GameObject effect = Instantiate(whiteRedEffectPrefab, spawnPos, Quaternion.identity);
                Destroy(effect, 1f);
                Debug.Log("白モードの赤魔法エフェクト発動！（1秒後に消滅）");
            }
            else
            {
                Debug.Log("Player参照またはwhiteRedEffectPrefabがありません");
            }
        }
        else if (currentMode == ModeType.Black && color == ColorType.Red)
        {
            const float blackRedSkillCooldown = 7f;
            if (player != null && fireEffectPrefab != null)
            {
                // クールタイム判定
                if (Time.time - lastBlackRedSkillTime < blackRedSkillCooldown)
                {
                    Debug.Log("黒赤スキルはクールタイム中です");
                    return;
                }
                lastBlackRedSkillTime = Time.time;

                // Playerの向きで発射方向を決定
                bool isRight = player.facingRight;
                float xOffset = isRight ? 1f : -1f;
                Vector3 spawnPos = player.transform.position + new Vector3(xOffset, 0.5f, 0);
                Quaternion rot = isRight ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
                GameObject fire = Instantiate(fireEffectPrefab, spawnPos, rot);
                Debug.Log("黒の赤魔法発動！炎エフェクト生成（横に進む）");
            }
            else
            {
                Debug.Log("Player参照またはfireEffectPrefabがありません");
            }
        }
        else
        {
            Debug.Log($"現在は{currentMode}の{color}を発動しました");
        }
    }

    private IEnumerator ResetAttackPowerAfterDelay(float delay, int originalPower)
    {
        yield return new WaitForSeconds(delay);
        if (player != null)
        {
            player.attackPower = originalPower;
            Debug.Log($"攻撃力が元に戻りました: {player.attackPower}");
        }
    }
}