using UnityEngine;

public class IceWallEffect : MonoBehaviour
{
    [Header("�X�ǂ̃_���[�W��")]
    public int damage = 1;
    [Header("�X�ǂ̎������ԁi�b�j")]
    public float duration = 2f;
    private float activeTime = 0.2f; // �ŏ���0.2�b�����_���[�W����
    private float timer = 0f;
    private bool canDamage = true;

    void Start()
    {
        activeTime = Mathf.Min(0.2f, duration); // �ǂ̎������Ԃ��Z��
        timer = 0f;
        canDamage = true;
        Destroy(gameObject, duration); // ��������
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (canDamage && timer > activeTime)
        {
            canDamage = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return;
        var enemy = other.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"IceWall��{enemy.gameObject.name}��{damage}�_���[�W");
        }
    }
}
