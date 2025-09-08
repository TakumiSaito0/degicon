using UnityEngine;
using System.Collections;
using System; // enumキャスト用

public class PlayerSkill : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [SerializeField] private Player player; // Player参照
    [SerializeField] private GameObject fireEffectPrefab; // 黒赤スキル用エフェクト
    [SerializeField] private GameObject whiteRedEffectPrefab; // 白赤スキル用エフェクト
    [SerializeField] private GameObject whiteBlueEffectPrefab; // 白青スキル用エフェクト
    [SerializeField] private GameObject bodyEffectPrefab; // 白赤スキル攻撃力アップ時の体エフェクト
    [SerializeField] private GameObject iceWallPrefab; // 黒青スキル用氷壁Prefab
    [SerializeField] private GameObject plantPrefab; // 黒緑スキル用植物Prefab
    [SerializeField] private GameObject whiteYellowEffectPrefab; // 白黄スキル用エフェクト

    private ModeType currentMode = ModeType.White;
    private float lastWhiteRedSkillTime = -15f; // 白赤スキルのクールタイム管理（初期値を-クールタイムに）
    private float lastBlackRedSkillTime = -7f; // 黒赤スキルのクールタイム管理（初期値を-クールタイムに）
    private float lastWhiteBlueSkillTime = -20f; // 白青スキルのクールタイム管理（初期値を-クールタイムに）
    private float lastBlackBlueSkillTime = -15f; // 黒青スキルのクールタイム管理
    private float lastWhiteGreenSkillTime = -10f; // 白緑スキルのクールタイム管理
    private float lastWhiteYellowSkillTime = -15f; // 白黄スキルのクールタイム管理（初期値を-クールタイムに）
    private float lastBlackYellowSkillTime = -70f; // 黒黄スキルのクールタイム管理
 
    
    // WhiteGreen
    private const float burrowDuration = 3f;
    private Vector3 burrowStartPos; // 潜伏開始位置
    private bool isBurrowCoroutineRunning = false;
    private Coroutine burrowCoroutineRef = null; // 潜伏コルーチン参照

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
        // Jキー：緑（白緑スキル発動）
        if (UnityEngine.InputSystem.Keyboard.current.jKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Green);
        }
        // Kキー：黄（白黄スキル発動）
        if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Yellow);
        }
        // 他の色は省略
    }

    // 黒緑スキルが使用可能か判定する
    public bool CanUseBlackGreenSkill()
    {
        if (currentMode != ModeType.Black) return false;
        // プレイヤーの前方にplantPrefabがあるか判定
        Vector3 checkPos = player.transform.position + new Vector3(player.facingRight ? 1.0f : -1.0f, 0, 0);
        Collider[] hits = Physics.OverlapSphere(checkPos, 1.0f);
        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != null && hit.gameObject.CompareTag("Plant"))
            {
                return true;
            }
        }
        return false;
    }

    // 黒緑スキル発動処理
    private void UseBlackGreenSkill()
    {
        // プレイヤーの前方にPlantがある場合のみ使用可能
        Vector3 checkPos = player.transform.position + new Vector3(player.facingRight ? 1.0f : -1.0f, 0, 0);
        Collider[] hits = Physics.OverlapSphere(checkPos, 1.0f);
        bool foundPlant = false;
        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != null && hit.gameObject.CompareTag("Plant"))
            {
                var grow = hit.gameObject.GetComponent<PlantGrow>();
                if (grow != null)
                {
                    foundPlant = true;
                    grow.Grow();
                    Debug.Log("黒緑スキル：植物を急成長させました");
                }
            }
        }
        // foundPlantがfalseなら何もせずreturn（UI通知はUseSkillで行う）
        return;
    }

    private void UseWhiteYellowSkill()
    {
        if (player == null) return;
        float teleportDistance = 5f;
        Vector3 direction = player.facingRight ? Vector3.right : Vector3.left;
        Vector3 start = player.transform.position;
        RaycastHit hit;
        Vector3 target;
        if (Physics.Raycast(start, direction, out hit, teleportDistance))
        {
            // 障害物の手前まで
            target = hit.point - direction * 0.5f;
        }
        else
        {
            // 障害物がなければ最大距離
            target = start + direction * teleportDistance;
        }
        player.transform.position = target;
        // エフェクト生成（任意）
        if (whiteYellowEffectPrefab != null)
        {
            Instantiate(whiteYellowEffectPrefab, target, Quaternion.identity);
        }
        // 瞬間移動アニメーション（例: 21）
        player.PlaySkillAnimation(21, 0.5f);
        Debug.Log($"白黄スキル：{target}に瞬間移動しました");
    }

    private void UseBlackYellowSkill()
    {
        // HP回復（LifeManager参照）
        if (LifeManager.instance != null)
        {
            LifeManager.instance.Heal(1);
            Debug.Log("黒黄スキル：HPを1回復しました");
        }
        else
        {
            Debug.Log("LifeManagerが見つかりません");
        }
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

        if (currentMode == ModeType.Black && color == ColorType.Yellow)
        {
            const float blackYellowSkillCooldown = 70f;
            if (Time.time - lastBlackYellowSkillTime < blackYellowSkillCooldown)
            {
                Debug.Log("黒黄スキルはクールタイム中です");
                return;
            }
            lastBlackYellowSkillTime = Time.time;
            SkillUIManager ui = FindObjectOfType<SkillUIManager>();
            if (ui != null) ui.UseSkill((SkillUIManager.ColorType)Enum.Parse(typeof(SkillUIManager.ColorType), color.ToString()));
            UseBlackYellowSkill();
        }
        else if (currentMode == ModeType.White && color == ColorType.Red)
        {
            const float whiteRedSkillCooldown = 15f;
            const float attackUpDuration = 5f;
            if (player != null && whiteRedEffectPrefab != null)
            {
                if (Time.time - lastWhiteRedSkillTime < whiteRedSkillCooldown)
                {
                    Debug.Log("白赤スキルはクールタイム中です");
                    return;
                }
                lastWhiteRedSkillTime = Time.time;
                int originalAttackPower = player.attackPower;
                player.attackPower = Mathf.RoundToInt(originalAttackPower * 1.5f);
                Debug.Log($"白の赤魔法発動！攻撃力: {player.attackPower}（5秒間アップ）");
                GameObject bodyEffect = null;
                if (bodyEffectPrefab != null)
                {
                    Vector3 bodyEffectPos = player.transform.position + new Vector3(0, -0.5f, 0);
                    bodyEffect = Instantiate(bodyEffectPrefab, bodyEffectPos, Quaternion.identity, player.transform);
                    Debug.Log("白赤スキル体エフェクト生成（少し下に表示）");
                }
                StartCoroutine(ResetAttackPowerAfterDelayWithEffect(attackUpDuration, originalAttackPower, bodyEffect));
                player.PlaySkillAnimation(7, 1.0f);
                Debug.Log("白赤スキルアニメーション(7)再生");
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
                if (Time.time - lastBlackRedSkillTime < blackRedSkillCooldown)
                {
                    Debug.Log("黒赤スキルはクールタイム中です");
                    return;
                }
                lastBlackRedSkillTime = Time.time;
                bool isRight = player.facingRight;
                float xOffset = isRight ? 1f : -1f;
                Vector3 spawnPos = player.transform.position + new Vector3(xOffset, 0.5f, 0);
                Quaternion rot = isRight ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
                GameObject fire = Instantiate(fireEffectPrefab, spawnPos, rot);
                Debug.Log("黒の赤魔法発動！炎エフェクト生成（横に進む）");
                player.PlaySkillAnimation(23, 1.0f);
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
                if (whiteBlueEffectPrefab != null)
                {
                    Vector3 spawnPos = player.transform.position + new Vector3(0, 0.5f, 0);
                    GameObject effect = Instantiate(whiteBlueEffectPrefab, spawnPos, Quaternion.identity);
                    Destroy(effect, 1.5f);
                    Debug.Log("白青スキルエフェクト発動！（1.5秒後に消滅）");
                }
                player.PlaySkillAnimation(7, 1.0f);
                Debug.Log("白青スキルアニメーション(7)再生");
                Debug.Log("白青スキル発動！次のジャンプ力が10fになります（1回のみ）");
            }
            else
            {
                Debug.Log("Player参照がありません");
            }
        }
        else if (currentMode == ModeType.Black && color == ColorType.Blue)
        {
            const float blackBlueSkillCooldown = 5f;
            if (Time.time - lastBlackBlueSkillTime < blackBlueSkillCooldown)
            {
                Debug.Log("黒青スキルはクールタイム中です");
                return;
            }
            lastBlackBlueSkillTime = Time.time;
            if (player != null && iceWallPrefab != null)
            {
                float xOffset = player.facingRight ? 1.0f : -1.0f;
                Vector3 spawnPos = player.transform.position + new Vector3(xOffset, -1f, 0.5f);
                Quaternion rot = player.facingRight ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
                Instantiate(iceWallPrefab, spawnPos, rot);
                player.PlaySkillAnimation(7, 1.0f);
                Debug.Log("黒青スキル（氷壁）発動！");
            }
            else
            {
                Debug.Log("Player/iceWallPrefab参照がありません");
            }
        }
        else if (currentMode == ModeType.White && color == ColorType.Green)
        {
            const float whiteGreenSkillCooldown = 10f;
            if (Time.time - lastWhiteGreenSkillTime < whiteGreenSkillCooldown)
            {
                Debug.Log("白緑スキルはクールタイム中です");
                return;
            }
            if (player != null)
            {
                if (!player.isBurrowing && !isBurrowCoroutineRunning)
                {
                    lastWhiteGreenSkillTime = Time.time;
                    burrowCoroutineRef = StartCoroutine(BurrowCoroutine());
                    Debug.Log("白緑スキル発動！地面に潜って移動可能（3秒間）");
                }
                else if (player.isBurrowing && isBurrowCoroutineRunning)
                {
                    if (burrowCoroutineRef != null)
                    {
                        StopCoroutine(burrowCoroutineRef);
                        burrowCoroutineRef = null;
                    }
                    EndBurrow();
                    Debug.Log("白緑スキル再発動→地上復帰");
                }
            }
            else
            {
                Debug.Log("Player参照がありません");
            }
        }
        else if (currentMode == ModeType.Black && color == ColorType.Green)
        {
            if (!CanUseBlackGreenSkill())
            {
                SkillUIManager ui = FindObjectOfType<SkillUIManager>();
                if (ui != null) ui.SetBlackGreenSkillUnavailable();
                Debug.Log("黒緑スキルは使用できません（植物が目の前にありません）");
                return;
            }
            UseBlackGreenSkill();
        }
        else if (currentMode == ModeType.White && color == ColorType.Yellow)
        {
            const float whiteYellowSkillCooldown = 15f;
            if (Time.time - lastWhiteYellowSkillTime < whiteYellowSkillCooldown)
            {
                Debug.Log("白黄スキルはクールタイム中です");
                return;
            }
            lastWhiteYellowSkillTime = Time.time;
            SkillUIManager ui = FindObjectOfType<SkillUIManager>();
            if (ui != null) ui.UseSkill((SkillUIManager.ColorType)Enum.Parse(typeof(SkillUIManager.ColorType), color.ToString()));
            UseWhiteYellowSkill();
        }
        else
        {
            Debug.Log($"現在は{currentMode}の{color}を発動しました");
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

    // 白緑スキル：地面に潜って移動できる
    private IEnumerator BurrowCoroutine()
    {
        isBurrowCoroutineRunning = true;
        player.isBurrowing = true;
        player.PlaySkillAnimation(31, 0.5f);
        yield return new WaitForSeconds(0.5f);
        // 現在位置から深く潜る（Y:-2.0f）
        player.transform.position += new Vector3(0, -2.0f, 0);
        Collider col = player.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = false;
        player.PlaySkillAnimation(25, burrowDuration);
        yield return new WaitForSeconds(burrowDuration);
        EndBurrow();
        burrowCoroutineRef = null;
    }

    private void EndBurrow()
    {
        player.isBurrowing = false;
        Vector3 pos = player.transform.position;
        player.transform.position = new Vector3(pos.x, pos.y + 2.0f, pos.z);
        Collider col = player.GetComponent<Collider>();
        if (col != null) col.enabled = true;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = true;
        isBurrowCoroutineRunning = false;
        burrowCoroutineRef = null;
        player.isSkillAnimPlaying = false;
        Debug.Log("白緑スキル終了。通常状態に復帰");
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

// 仮のPlantGrowクラス（本来は植物の成長処理を持つ）
public class PlantGrow : MonoBehaviour
{
    public void Grow()
    {
        // 植物の成長処理
    }
}