using UnityEngine;
using System.Collections;
using System; // enum�L���X�g�p

public class PlayerSkill : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [SerializeField] private Player player; // Player�Q��
    [SerializeField] private GameObject fireEffectPrefab; // ���ԃX�L���p�G�t�F�N�g
    [SerializeField] private GameObject whiteRedEffectPrefab; // ���ԃX�L���p�G�t�F�N�g
    [SerializeField] private GameObject whiteBlueEffectPrefab; // ���X�L���p�G�t�F�N�g
    [SerializeField] private GameObject bodyEffectPrefab; // ���ԃX�L���U���̓A�b�v���̑̃G�t�F�N�g
    [SerializeField] private GameObject iceWallPrefab; // ���X�L���p�X��Prefab
    [SerializeField] private GameObject plantPrefab; // ���΃X�L���p�A��Prefab
    [SerializeField] private GameObject whiteYellowEffectPrefab; // �����X�L���p�G�t�F�N�g

    private ModeType currentMode = ModeType.White;
    private float lastWhiteRedSkillTime = -15f; // ���ԃX�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj
    private float lastBlackRedSkillTime = -7f; // ���ԃX�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj
    private float lastWhiteBlueSkillTime = -20f; // ���X�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj
    private float lastBlackBlueSkillTime = -15f; // ���X�L���̃N�[���^�C���Ǘ�
    private float lastWhiteGreenSkillTime = -10f; // ���΃X�L���̃N�[���^�C���Ǘ�
    private float lastWhiteYellowSkillTime = -15f; // �����X�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj
    private float lastBlackYellowSkillTime = -70f; // �����X�L���̃N�[���^�C���Ǘ�
 
    
    // WhiteGreen
    private const float burrowDuration = 3f;
    private Vector3 burrowStartPos; // �����J�n�ʒu
    private bool isBurrowCoroutineRunning = false;
    private Coroutine burrowCoroutineRef = null; // �����R���[�`���Q��

    private bool[,] skillUnlocked = new bool[2, 4]; // [mode, color]
   

    void Awake()
    {
        // �e�X�g�p�F�S�X�L�����
        for (int m = 0; m < 2; m++)
            for (int c = 0; c < 4; c++)
                skillUnlocked[m, c] = true;
    }

    void Update()
    {
        // Q�L�[�Ŕ����؂�ւ�
        if (UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
        {
            currentMode = currentMode == ModeType.White ? ModeType.Black : ModeType.White;
            Debug.Log($"���[�h�ؑ�: ���݂�{currentMode}");
        }

        // I�L�[�F��
        if (UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Red);
        }
        // L�L�[�F�i���X�L�������j
        if (UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Blue);
        }
        // J�L�[�F�΁i���΃X�L�������j
        if (UnityEngine.InputSystem.Keyboard.current.jKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Green);
        }
        // K�L�[�F���i�����X�L�������j
        if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            UseSkill(ColorType.Yellow);
        }
        // ���̐F�͏ȗ�
    }

    // ���΃X�L�����g�p�\�����肷��
    public bool CanUseBlackGreenSkill()
    {
        if (currentMode != ModeType.Black) return false;
        // �v���C���[�̑O����plantPrefab�����邩����
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

    // ���΃X�L����������
    private void UseBlackGreenSkill()
    {
        // �v���C���[�̑O����Plant������ꍇ�̂ݎg�p�\
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
                    Debug.Log("���΃X�L���F�A�����}���������܂���");
                }
            }
        }
        // foundPlant��false�Ȃ牽������return�iUI�ʒm��UseSkill�ōs���j
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
            // ��Q���̎�O�܂�
            target = hit.point - direction * 0.5f;
        }
        else
        {
            // ��Q�����Ȃ���΍ő勗��
            target = start + direction * teleportDistance;
        }
        player.transform.position = target;
        // �G�t�F�N�g�����i�C�Ӂj
        if (whiteYellowEffectPrefab != null)
        {
            Instantiate(whiteYellowEffectPrefab, target, Quaternion.identity);
        }
        // �u�Ԉړ��A�j���[�V�����i��: 21�j
        player.PlaySkillAnimation(21, 0.5f);
        Debug.Log($"�����X�L���F{target}�ɏu�Ԉړ����܂���");
    }

    private void UseBlackYellowSkill()
    {
        // HP�񕜁iLifeManager�Q�Ɓj
        if (LifeManager.instance != null)
        {
            LifeManager.instance.Heal(1);
            Debug.Log("�����X�L���FHP��1�񕜂��܂���");
        }
        else
        {
            Debug.Log("LifeManager��������܂���");
        }
    }

    private void UseSkill(ColorType color)
    {
        int m = (int)currentMode;
        int c = (int)color;
        if (!skillUnlocked[m, c])
        {
            Debug.Log("���̃X�L���͂܂��g���܂���");
            return;
        }

        if (currentMode == ModeType.Black && color == ColorType.Yellow)
        {
            const float blackYellowSkillCooldown = 70f;
            if (Time.time - lastBlackYellowSkillTime < blackYellowSkillCooldown)
            {
                Debug.Log("�����X�L���̓N�[���^�C�����ł�");
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
                    Debug.Log("���ԃX�L���̓N�[���^�C�����ł�");
                    return;
                }
                lastWhiteRedSkillTime = Time.time;
                int originalAttackPower = player.attackPower;
                player.attackPower = Mathf.RoundToInt(originalAttackPower * 1.5f);
                Debug.Log($"���̐Ԗ��@�����I�U����: {player.attackPower}�i5�b�ԃA�b�v�j");
                GameObject bodyEffect = null;
                if (bodyEffectPrefab != null)
                {
                    Vector3 bodyEffectPos = player.transform.position + new Vector3(0, -0.5f, 0);
                    bodyEffect = Instantiate(bodyEffectPrefab, bodyEffectPos, Quaternion.identity, player.transform);
                    Debug.Log("���ԃX�L���̃G�t�F�N�g�����i�������ɕ\���j");
                }
                StartCoroutine(ResetAttackPowerAfterDelayWithEffect(attackUpDuration, originalAttackPower, bodyEffect));
                player.PlaySkillAnimation(7, 1.0f);
                Debug.Log("���ԃX�L���A�j���[�V����(7)�Đ�");
                Vector3 spawnPos = player.transform.position + new Vector3(0, 0f, 0);
                GameObject effect = Instantiate(whiteRedEffectPrefab, spawnPos, Quaternion.identity);
                Destroy(effect, 1f);
                Debug.Log("�����[�h�̐Ԗ��@�G�t�F�N�g�����I�i1�b��ɏ��Łj");
            }
            else
            {
                Debug.Log("Player�Q�Ƃ܂���whiteRedEffectPrefab������܂���");
            }
        }
        else if (currentMode == ModeType.Black && color == ColorType.Red)
        {
            const float blackRedSkillCooldown = 7f;
            if (player != null && fireEffectPrefab != null)
            {
                if (Time.time - lastBlackRedSkillTime < blackRedSkillCooldown)
                {
                    Debug.Log("���ԃX�L���̓N�[���^�C�����ł�");
                    return;
                }
                lastBlackRedSkillTime = Time.time;
                bool isRight = player.facingRight;
                float xOffset = isRight ? 1f : -1f;
                Vector3 spawnPos = player.transform.position + new Vector3(xOffset, 0.5f, 0);
                Quaternion rot = isRight ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
                GameObject fire = Instantiate(fireEffectPrefab, spawnPos, rot);
                Debug.Log("���̐Ԗ��@�����I���G�t�F�N�g�����i���ɐi�ށj");
                player.PlaySkillAnimation(23, 1.0f);
                Debug.Log("���ԃX�L���A�j���[�V����(23)�Đ�");
            }
            else
            {
                Debug.Log("Player�Q�Ƃ܂���fireEffectPrefab������܂���");
            }
        }
        else if (currentMode == ModeType.White && color == ColorType.Blue)
        {
            const float whiteBlueSkillCooldown = 20f;
            if (Time.time - lastWhiteBlueSkillTime < whiteBlueSkillCooldown)
            {
                Debug.Log("���X�L���̓N�[���^�C�����ł�");
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
                    Debug.Log("���X�L���G�t�F�N�g�����I�i1.5�b��ɏ��Łj");
                }
                player.PlaySkillAnimation(7, 1.0f);
                Debug.Log("���X�L���A�j���[�V����(7)�Đ�");
                Debug.Log("���X�L�������I���̃W�����v�͂�10f�ɂȂ�܂��i1��̂݁j");
            }
            else
            {
                Debug.Log("Player�Q�Ƃ�����܂���");
            }
        }
        else if (currentMode == ModeType.Black && color == ColorType.Blue)
        {
            const float blackBlueSkillCooldown = 5f;
            if (Time.time - lastBlackBlueSkillTime < blackBlueSkillCooldown)
            {
                Debug.Log("���X�L���̓N�[���^�C�����ł�");
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
                Debug.Log("���X�L���i�X�ǁj�����I");
            }
            else
            {
                Debug.Log("Player/iceWallPrefab�Q�Ƃ�����܂���");
            }
        }
        else if (currentMode == ModeType.White && color == ColorType.Green)
        {
            const float whiteGreenSkillCooldown = 10f;
            if (Time.time - lastWhiteGreenSkillTime < whiteGreenSkillCooldown)
            {
                Debug.Log("���΃X�L���̓N�[���^�C�����ł�");
                return;
            }
            if (player != null)
            {
                if (!player.isBurrowing && !isBurrowCoroutineRunning)
                {
                    lastWhiteGreenSkillTime = Time.time;
                    burrowCoroutineRef = StartCoroutine(BurrowCoroutine());
                    Debug.Log("���΃X�L�������I�n�ʂɐ����Ĉړ��\�i3�b�ԁj");
                }
                else if (player.isBurrowing && isBurrowCoroutineRunning)
                {
                    if (burrowCoroutineRef != null)
                    {
                        StopCoroutine(burrowCoroutineRef);
                        burrowCoroutineRef = null;
                    }
                    EndBurrow();
                    Debug.Log("���΃X�L���Ĕ������n�㕜�A");
                }
            }
            else
            {
                Debug.Log("Player�Q�Ƃ�����܂���");
            }
        }
        else if (currentMode == ModeType.Black && color == ColorType.Green)
        {
            if (!CanUseBlackGreenSkill())
            {
                SkillUIManager ui = FindObjectOfType<SkillUIManager>();
                if (ui != null) ui.SetBlackGreenSkillUnavailable();
                Debug.Log("���΃X�L���͎g�p�ł��܂���i�A�����ڂ̑O�ɂ���܂���j");
                return;
            }
            UseBlackGreenSkill();
        }
        else if (currentMode == ModeType.White && color == ColorType.Yellow)
        {
            const float whiteYellowSkillCooldown = 15f;
            if (Time.time - lastWhiteYellowSkillTime < whiteYellowSkillCooldown)
            {
                Debug.Log("�����X�L���̓N�[���^�C�����ł�");
                return;
            }
            lastWhiteYellowSkillTime = Time.time;
            SkillUIManager ui = FindObjectOfType<SkillUIManager>();
            if (ui != null) ui.UseSkill((SkillUIManager.ColorType)Enum.Parse(typeof(SkillUIManager.ColorType), color.ToString()));
            UseWhiteYellowSkill();
        }
        else
        {
            Debug.Log($"���݂�{currentMode}��{color}�𔭓����܂���");
        }
    }

    private IEnumerator ResetAttackPowerAfterDelayWithEffect(float delay, int originalPower, GameObject bodyEffect)
    {
        yield return new WaitForSeconds(delay);
        if (player != null)
        {
            player.attackPower = originalPower;
            Debug.Log($"�U���͂����ɖ߂�܂���: {player.attackPower}");
        }
        if (bodyEffect != null)
        {
            Destroy(bodyEffect);
            Debug.Log("���ԃX�L���̃G�t�F�N�g����");
        }
    }

    // ���΃X�L���F�n�ʂɐ����Ĉړ��ł���
    private IEnumerator BurrowCoroutine()
    {
        isBurrowCoroutineRunning = true;
        player.isBurrowing = true;
        player.PlaySkillAnimation(31, 0.5f);
        yield return new WaitForSeconds(0.5f);
        // ���݈ʒu����[������iY:-2.0f�j
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
        Debug.Log("���΃X�L���I���B�ʏ��Ԃɕ��A");
    }

    void UnlockSkillsForStage(int stage)
    {
        // ��: �X�e�[�W1�͔��Ԃ̂�
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
        // ...�ȍ~�X�e�[�W���Ƃɉ��
    }
}

// ����PlantGrow�N���X�i�{���͐A���̐������������j
public class PlantGrow : MonoBehaviour
{
    public void Grow()
    {
        // �A���̐�������
    }
}