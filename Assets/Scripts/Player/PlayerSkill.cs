using UnityEngine;
using System.Collections;

public class PlayerSkill : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [SerializeField] private Player player; // Player�Q��
    [SerializeField] private GameObject fireEffectPrefab; // ���ԃX�L���p�G�t�F�N�g
    [SerializeField] private GameObject whiteRedEffectPrefab; // ���ԃX�L���p�G�t�F�N�g

    private ModeType currentMode = ModeType.White;
    private float lastWhiteRedSkillTime = -4f; // ���ԃX�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj
    private float lastBlackRedSkillTime = -7f; // ���ԃX�L���̃N�[���^�C���Ǘ��i�����l��-�N�[���^�C���Ɂj

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
        // ���̐F�͏ȗ�
    }

    private void UseSkill(ColorType color)
    {
        if (currentMode == ModeType.White && color == ColorType.Red)
        {
            const float whiteRedSkillCooldown = 4f;
            const float blackRedSkillCooldown = 7f;

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

                StartCoroutine(ResetAttackPowerAfterDelay(blackRedSkillCooldown, originalAttackPower));

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
            }
            else
            {
                Debug.Log("Player�Q�Ƃ܂���fireEffectPrefab������܂���");
            }
        }
        else
        {
            Debug.Log($"���݂�{currentMode}��{color}�𔭓����܂���");
        }
    }

    private IEnumerator ResetAttackPowerAfterDelay(float delay, int originalPower)
    {
        yield return new WaitForSeconds(delay);
        if (player != null)
        {
            player.attackPower = originalPower;
            Debug.Log($"�U���͂����ɖ߂�܂���: {player.attackPower}");
        }
    }
}