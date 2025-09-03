using UnityEngine;
using System.Collections;

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

    private ModeType currentMode = ModeType.White;
    private float lastWhiteRedSkillTime = -15f; // ���ԃX�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj
    private float lastBlackRedSkillTime = -7f; // ���ԃX�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj
    private float lastWhiteBlueSkillTime = -20f; // ���X�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj
    private float lastBlackBlueSkillTime = -15f; // ���X�L���̃N�[���^�C���Ǘ�
    private float lastWhiteGreenSkillTime = -10f; // ���΃X�L���̃N�[���^�C���Ǘ�
 
    
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
        // ���̐F�͏ȗ�
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

        if (currentMode == ModeType.White && color == ColorType.Red)
        {
            const float whiteRedSkillCooldown = 15f;
            const float attackUpDuration = 5f;

            if (player != null && whiteRedEffectPrefab != null)
            {
                // �N�[���^�C������
                if (Time.time - lastWhiteRedSkillTime < whiteRedSkillCooldown)
                {
                    Debug.Log("���ԃX�L���̓N�[���^�C�����ł�");
                    return;
                }
                lastWhiteRedSkillTime = Time.time;

                // �U���̓A�b�v�i5�b�Ԃ̂݁j
                int originalAttackPower = player.attackPower;
                player.attackPower = Mathf.RoundToInt(originalAttackPower * 1.5f);
                Debug.Log($"���̐Ԗ��@�����I�U����: {player.attackPower}�i5�b�ԃA�b�v�j");

                // �̃G�t�F�N�g�����i�U���̓A�b�v���̂ݕ\���j
                GameObject bodyEffect = null;
                if (bodyEffectPrefab != null)
                {
                    Vector3 bodyEffectPos = player.transform.position + new Vector3(0, -0.5f, 0); // Y���W��-0.5������
                    bodyEffect = Instantiate(bodyEffectPrefab, bodyEffectPos, Quaternion.identity, player.transform);
                    Debug.Log("���ԃX�L���̃G�t�F�N�g�����i�������ɕ\���j");
                }

                StartCoroutine(ResetAttackPowerAfterDelayWithEffect(attackUpDuration, originalAttackPower, bodyEffect));

                // ���ԃX�L���A�j���[�V����(7)�Đ��i�X�L���A�j���[�V�����D��j
                player.PlaySkillAnimation(7, 1.0f); // 1�b�ԍĐ��i�K�v�ɉ����Ē����j
                Debug.Log("���ԃX�L���A�j���[�V����(7)�Đ�");

                // �����[�h�̐Ԗ��@�G�t�F�N�g�����iY���W��1f�����Đ����A1�b��ɏ����j
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
                // �N�[���^�C������
                if (Time.time - lastBlackRedSkillTime < blackRedSkillCooldown)
                {
                    Debug.Log("���ԃX�L���̓N�[���^�C�����ł�");
                    return;
                }
                lastBlackRedSkillTime = Time.time;

                // Player�̌����Ŕ��˕���������
                bool isRight = player.facingRight;
                float xOffset = isRight ? 1f : -1f;
                Vector3 spawnPos = player.transform.position + new Vector3(xOffset, 0.5f, 0);
                Quaternion rot = isRight ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
                GameObject fire = Instantiate(fireEffectPrefab, spawnPos, rot);
                Debug.Log("���̐Ԗ��@�����I���G�t�F�N�g�����i���ɐi�ށj");
                // Animation 23 �Đ��i�X�L���A�j���[�V�����D��j
                player.PlaySkillAnimation(23, 1.0f); // 1�b�ԍĐ��i�K�v�ɉ����Ē����j
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
                // �G�t�F�N�g����
                if (whiteBlueEffectPrefab != null)
                {
                    Vector3 spawnPos = player.transform.position + new Vector3(0, 0.5f, 0);
                    GameObject effect = Instantiate(whiteBlueEffectPrefab, spawnPos, Quaternion.identity);
                    Destroy(effect, 1.5f);
                    Debug.Log("���X�L���G�t�F�N�g�����I�i1.5�b��ɏ��Łj");
                }
                // �W�����v�A�j���[�V�����Đ��ianimation7���Đ��j
                player.PlaySkillAnimation(7, 1.0f); // 1�b�ԍĐ��i�K�v�ɉ����Ē����j
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
                // �v���C���[�̑O���ɕX�ǂ𐶐��iY���W��-0.5f������j
                float xOffset = player.facingRight ? 1.0f : -1.0f;
                Vector3 spawnPos = player.transform.position + new Vector3(xOffset, -1f, 0.5f); // Y:-0.5f
                Quaternion rot = player.facingRight ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
                Instantiate(iceWallPrefab, spawnPos, rot);
                // ���X�L���A�j���[�V����(7)�Đ�
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
                    // �������ɍĔ����������ɒn�㕜�A
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