using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int damage = 1;
    private Transform player;
    private int health = 2; // �̗�2

    void Start()
    {
        // Player�^�O�̃I�u�W�F�N�g��T��
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Player���������Ă���Βǂ�������
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // �n�ʂɉ����Ĉړ�
            transform.position += direction * moveSpeed * Time.deltaTime;
            // �i�s�����ɑ̂�������
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Player��HP���Q�Ƃ��ă_���[�W��^����
            var playerScript = collision.collider.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log($"Enemy��Player�Ƀ_���[�W: {damage}");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"Enemy�̗̑�: {health}");
        if (health <= 0)
        {
            // �̗͂�0�ɂȂ����ꍇ�̏���
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy�����ł��܂���");
        // ���S�G�t�F�N�g��X�R�A���Z�Ȃǂ̏����������ɒǉ��\
        Destroy(gameObject);
    }
}