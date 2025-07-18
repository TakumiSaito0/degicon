using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public enum ModeType { White, Black }
    public enum ColorType { Red, Blue, Green, Yellow }

    [SerializeField] private Player player; // Player�Q��

    private ModeType currentMode = ModeType.White;

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
            if (player != null)
            {
                player.attackPower = Mathf.RoundToInt(player.attackPower * 1.5f);
                Debug.Log($"���̐Ԗ��@�����I�U����: {player.attackPower}");
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