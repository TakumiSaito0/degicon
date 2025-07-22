using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [SerializeField] private Player player; // Player�Q��
    [SerializeField] private GameObject effectPrefab; // �G�t�F�N�gPrefab

    private ModeType currentMode = ModeType.White;

    private float lastSkillTime = -999f;
    private float skillCooldown = 5f;

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
            // �N�[���^�C������
            if (Time.time - lastSkillTime < skillCooldown)
            {
                Debug.Log("�X�L���̓N�[���^�C�����ł�");
                return;
            }

            lastSkillTime = Time.time;

            if (player != null)
            {
                player.attackPower = Mathf.RoundToInt(player.attackPower * 1.5f);
                Debug.Log($"���̐Ԗ��@�����I�U����: {player.attackPower}");

                // �G�t�F�N�g�����i1�b��ɏ����j
                if (effectPrefab != null)
                {
                    GameObject effect = Instantiate(effectPrefab, player.transform.position, Quaternion.identity);
                    Destroy(effect, 1f);
                    Debug.Log("�����[�h�̐Ԗ��@�G�t�F�N�g�����I�i1�b��ɏ��Łj");
                }
                else
                {
                    Debug.Log("effectPrefab���A�^�b�`����Ă��܂���");
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
}