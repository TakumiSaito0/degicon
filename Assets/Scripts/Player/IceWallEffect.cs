using UnityEngine;

public class IceWallEffect : MonoBehaviour
{
    [Header("�X�ǂ̃_���[�W��")]
    public int damage = 1;
    [Header("�X�ǂ̎������ԁi�b�j")]
    public float duration = 2f;
    [Header("����オ�艉�o�̍����iY���W�j")]
    public float riseHeight = 1.0f;
    [Header("����オ�艉�o�̎��ԁi�b�j")]
    public float riseTime = 0.3f;
    private float activeTime = 0.2f; // �ŏ���0.2�b�����_���[�W����
    private float timer = 0f;
    private bool canDamage = true;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float riseTimer = 0f;
    private bool rising = true;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + new Vector3(0, riseHeight, 0);
        transform.position = startPos;
        riseTimer = 0f;
        rising = true;
        activeTime = Mathf.Min(0.2f, duration);
        timer = 0f;
        canDamage = true;
        Destroy(gameObject, duration);
    }

    void Update()
    {
        // ����オ�艉�o
        if (rising)
        {
            riseTimer += Time.deltaTime;
            float t = Mathf.Clamp01(riseTimer / riseTime);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (t >= 1f)
            {
                rising = false;
            }
        }
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
