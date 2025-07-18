using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    
    // ���G���Ԋ֘A
    public float invincibilityDuration = 1f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    // �_�ŃG�t�F�N�g�p
    private Renderer playerRenderer;
    private Color originalColor;

    void Start()
    {
        // �v���C���[��Renderer���擾
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"MoveInput: {moveInput}");
    }

    void Update()
    {
        // �ړ�����
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // ���G���Ԃ̏���
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // �_�ŃG�t�F�N�g
            if (playerRenderer != null)
            {
                float alpha = Mathf.Sin(Time.time * 20f) * 0.5f + 0.5f;
                Color flickerColor = originalColor;
                flickerColor.a = alpha;
                playerRenderer.material.color = flickerColor;
            }

            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                if (playerRenderer != null)
                {
                    playerRenderer.material.color = originalColor;
                }
            }
        }
    }

    // ���G��Ԃ��J�n����i�O������Ăяo���p�j
    public void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    // ���G��Ԃ��ǂ�����Ԃ�
    public bool IsInvincible()
    {
        return isInvincible;
    }

    // �G�Ƃ̏Փˏ���
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !isInvincible)
        {
            // �_���[�W���󂯂�
            if (LifeManager.instance != null)
            {
                LifeManager.instance.TakeDamage(1);
                
                // ���G���Ԃ��J�n
                StartInvincibility();
                
                Debug.Log("�G�ɍU������܂����I");
            }
        }
    }

    // �ʂ̏Փˌ��o���@�iRigidbody���g�p���Ă���ꍇ�j
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            // �_���[�W���󂯂�
            if (LifeManager.instance != null)
            {
                LifeManager.instance.TakeDamage(1);
                
                // ���G���Ԃ��J�n
                StartInvincibility();
                
                Debug.Log("�G�ɍU������܂����I");
            }
        }
    }
}