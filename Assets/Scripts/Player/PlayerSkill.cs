using UnityEngine;
using System.Collections;

public class PlayerSkill : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [SerializeField] private Player player; // Player参照
    [SerializeField] private GameObject fireEffectPrefab; // 黒赤スキル用エフェクト
    [SerializeField] private GameObject whiteRedEffectPrefab; // 白赤スキル用エフェクト
    [SerializeField] private GameObject whiteBlueEffectPrefab; // 白青スキル用エフェクト
    [SerializeField] private GameObject bodyEffectPrefab; // 白赤スキル攻撃力アップ時の体エフェクト

    private ModeType currentMode = ModeType.White;
    private float lastWhiteRedSkillTime = -15f; // 白赤スキルのクールタイム管理（初期値を-クールタイムに）
    private float lastBlackRedSkillTime = -7f; // 黒赤スキルのクールタイム管理（初期値を-クールタイムに）
    private float lastWhiteBlueSkillTime = -20f; // 白青スキルのクールタイム管理（初期値を-クールタイムに）
    private bool[,] skillUnlocked = new bool[2, 4]; // [mode, color]

    void Awake()
    {
        // テスト用：全スキル解放
        for (int m = 0; m < 2; m++)
            for (int c = 0; c < 4; c++)
                skillUnlocked[m, c] = true;
    }

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
        // Lキー：青（白青スキル発動）
        if (UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Blue);
        }
        // 他の色は省略
    }

    private void UseSkill(ColorType color)
    {
        int m = (int)currentMode;
        int c = (int)color;
        if (!skillUnlocked[m, c])
        {
            Debug.Log("このスキルはまだ使えません");
            return;
        }

        if (currentMode == ModeType.White && color == ColorType.Red)
        {
            const float whiteRedSkillCooldown = 15f;
            const float attackUpDuration = 5f;

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

                // 体エフェクト生成（攻撃力アップ中のみ表示）
                GameObject bodyEffect = null;
                if (bodyEffectPrefab != null)
                {
                    Vector3 bodyEffectPos = player.transform.position + new Vector3(0, -0.5f, 0); // Y座標を-0.5下げる
                    bodyEffect = Instantiate(bodyEffectPrefab, bodyEffectPos, Quaternion.identity, player.transform);
                    Debug.Log("白赤スキル体エフェクト生成（少し下に表示）");
                }

                StartCoroutine(ResetAttackPowerAfterDelayWithEffect(attackUpDuration, originalAttackPower, bodyEffect));

                // 白赤スキルアニメーション(7)再生（スキルアニメーション優先）
                player.PlaySkillAnimation(7, 1.0f); // 1秒間再生（必要に応じて調整）
                Debug.Log("白赤スキルアニメーション(7)再生");

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
                // Animation 23 再生（スキルアニメーション優先）
                player.PlaySkillAnimation(23, 1.0f); // 1秒間再生（必要に応じて調整）
                Debug.Log("黒赤スキルアニメーション(23)再生");
            }
            else
            {
                Debug.Log("Player参照またはfireEffectPrefabがありません");
            }
        }
        else if (currentMode == ModeType.White && color == ColorType.Blue)
        {
            const float whiteBlueSkillCooldown = 20f;
            if (Time.time - lastWhiteBlueSkillTime < whiteBlueSkillCooldown)
            {
                Debug.Log("白青スキルはクールタイム中です");
                return;
            }
            lastWhiteBlueSkillTime = Time.time;
            if (player != null)
            {
                player.BoostNextJump();
                // エフェクト生成
                if (whiteBlueEffectPrefab != null)
                {
                    Vector3 spawnPos = player.transform.position + new Vector3(0, 0.5f, 0);
                    GameObject effect = Instantiate(whiteBlueEffectPrefab, spawnPos, Quaternion.identity);
                    Destroy(effect, 1.5f);
                    Debug.Log("白青スキルエフェクト発動！（1.5秒後に消滅）");
                }
                // ジャンプアニメーション再生（animation7を再生）
                player.PlaySkillAnimation(7, 1.0f); // 1秒間再生（必要に応じて調整）
                Debug.Log("白青スキルアニメーション(7)再生");
                Debug.Log("白青スキル発動！次のジャンプ力が10fになります（1回のみ）");
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

    private IEnumerator ResetAttackPowerAfterDelay(float delay, int originalPower)
    {
        yield return new WaitForSeconds(delay);
        if (player != null)
        {
            player.attackPower = originalPower;
            Debug.Log($"攻撃力が元に戻りました: {player.attackPower}");
        }
    }

    private IEnumerator ResetAttackPowerAfterDelayWithEffect(float delay, int originalPower, GameObject bodyEffect)
    {
        yield return new WaitForSeconds(delay);
        if (player != null)
        {
            player.attackPower = originalPower;
            Debug.Log($"攻撃力が元に戻りました: {player.attackPower}");
        }
        if (bodyEffect != null)
        {
            Destroy(bodyEffect);
            Debug.Log("白赤スキル体エフェクト消滅");
        }
    }

    void UnlockSkillsForStage(int stage)
    {
        // 例: ステージ1は白赤のみ
        for (int m = 0; m < 2; m++)
            for (int c = 0; c < 4; c++)
                skillUnlocked[m, c] = false;

        if (stage == 1)
            skillUnlocked[(int)ModeType.White, (int)ColorType.Red] = true;
        else if (stage == 2)
        {
            skillUnlocked[(int)ModeType.White, (int)ColorType.Red] = true;
            skillUnlocked[(int)ModeType.Black, (int)ColorType.Red] = true;
        }
        // ...以降ステージごとに解放
    }
}