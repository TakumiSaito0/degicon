using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveInput;
    private PlayerInputActions playerInput;
    private bool facingRight = true;

    // AnimatorController�Q��
    [SerializeField] private AnimatorController animatorController;

    [SerializeField] private float jumpForce = 5f; // �W�����v��
    private bool isJumping = false;
    private bool isAttacking = false;
    private float attackDuration = 0.5f; // 攻撃アニメーションの長さ（秒）
    private float attackTimer = 0f;

    [SerializeField] private Vector3 attackBoxSize = new Vector3(1f, 1f, 1f); // 判定ボックスサイズ
    [SerializeField] public int attackPower = 1; // 攻撃力

    private void OnEnable()
    {
        if (playerInput == null)
            playerInput = new PlayerInputActions();
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += OnMove;
        playerInput.Player.Move.canceled += OnMove;
        playerInput.Player.Jump.performed += OnJump;
        playerInput.Player.Attack.performed += OnAttack; // 近接攻撃イベント登録
    }

    private void OnDisable()
    {
        playerInput.Player.Move.performed -= OnMove;
        playerInput.Player.Move.canceled -= OnMove;
        playerInput.Player.Jump.performed -= OnJump;
        playerInput.Player.Attack.performed -= OnAttack; // 近接攻撃イベント解除
        playerInput.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (animatorController != null)
        {
            animatorController.SetInt("Animation,16"); // Jump
            Debug.Log("Jump Animation Triggered (16)");
            isJumping = true;
        }
        var rb = GetComponent<Rigidbody>();
        if (rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (animatorController != null && !isAttacking)
        {
            animatorController.SetInt("Animation,2"); // Attack
            Debug.Log("Attack Animation Triggered (2)");
            isAttacking = true;
            attackTimer = attackDuration;

            // 攻撃判定
            float offset = facingRight ? 0.5f : -0.5f;
            Vector3 attackCenter = transform.position + new Vector3(offset, 0.7f, 0);
            Collider[] hits = Physics.OverlapBox(attackCenter, attackBoxSize * 0.5f, Quaternion.identity);
            bool hitEnemy = false;
            foreach (var hit in hits)
            {
                if (hit != null && hit.gameObject != null && hit.gameObject.CompareTag("Enemy"))
                {
                    Debug.Log($"敵に攻撃！攻撃力: {attackPower}");
                    hitEnemy = true;
                    // ここで敵のHPを減らす処理などを追加可能
                }
            }
            if (!hitEnemy)
            {
                Debug.Log($"攻撃したが敵はいません。現在の攻撃力: {attackPower}");
            }
            else
            {
                Debug.Log($"現在の攻撃力: {attackPower}");
            }
        }
    }

    void Update()
    {
        Debug.Log("Update called"); // ���ꂪ�o�͂���邩�m�F
        // ���E�ړ��̂�
        Vector3 move = new Vector3(moveInput.x, 0, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // �����̐؂�ւ�
        if (moveInput.x > 0 && !facingRight)
        {
            SetFacing(true);
        }
        else if (moveInput.x < 0 && facingRight)
        {
            SetFacing(false);
        }

        if (animatorController != null)
        {
            // 攻撃中は他のアニメーション値を送らない
            if (isAttacking)
            {
                animatorController.SetInt("Animation,2");
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    isAttacking = false;
                }
                return;
            }

            // ジャンプ中も同様に
            if (isJumping)
            {
                animatorController.SetInt("Animation,16");
                var rb = GetComponent<Rigidbody>();
                if (rb != null && rb.linearVelocity.y == 0)
                {
                    isJumping = false;
                }
                return;
            }

            // 通常のIdle/Moveアニメーション
            float speed = Mathf.Abs(moveInput.x);
            animatorController.SetFloat($"Speed,{speed}");

            int animationValue;
            if (speed < 0.05f)
            {
                animationValue = 13; // Idle
            }
            else if (moveInput.x > 0)
            {
                animationValue = 19; // Move_R
            }
            else if (moveInput.x < 0)
            {
                animationValue = 18; // Move_L
            }
            else
            {
                animationValue = 13;
            }

            animatorController.SetInt($"Animation,{animationValue}");
            Debug.Log($"Set Animation: {animationValue}, Speed: {speed}");
        }

    }

    private void SetFacing(bool right)
    {
        facingRight = right;
        float yRotation = right ? 90f : 270f;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
        // �A�j���[�V������Update�Ő��䂷��̂ł����ł͉������Ȃ�
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float offset = facingRight ? 0.5f : -0.5f;
        Vector3 attackCenter = transform.position + new Vector3(offset, 0.7f, 0);
        Gizmos.DrawWireCube(attackCenter, attackBoxSize);
    }
}