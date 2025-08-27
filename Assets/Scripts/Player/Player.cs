using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveInput;
    private PlayerInputActions playerInput;
    public bool facingRight = true;

    // AnimatorController�Q��
    [SerializeField] public AnimatorController animatorController;

    [SerializeField] private float jumpForce = 5f; // �W�����v��
    private bool isJumping = false;
    private bool isAttacking = false;
    private float attackDuration = 0.5f; // 攻撃アニメーションの長さ（秒）
    private float attackTimer = 0f;

    [SerializeField] private Vector3 attackBoxSize = new Vector3(1f, 1f, 1f); // 判定ボックスサイズ
    [SerializeField] public int attackPower = 1; // 攻撃力

    private bool isInvincible = false; // 無敵状態フラグ
    private int health = 3; // プレイヤーのHP
    private bool isDead = false;

    private bool nextJumpBoosted = false; // 次のジャンプがブーストされるフラグ
    private bool isSkillAnimPlaying = false;
    private float skillAnimTimer = 0f;

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
            float force = nextJumpBoosted ? 10f : jumpForce;
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
            if (nextJumpBoosted)
            {
                nextJumpBoosted = false;
                Debug.Log("ジャンプ力増加ジャンプ（10f）");
            }
        }
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (animatorController != null && !isAttacking)
        {
            animatorController.SetInt("Animation,2"); // Attack
         //   Debug.Log("Attack Animation Triggered (2)");
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
              //      Debug.Log($"敵に攻撃！攻撃力: {attackPower}");
                    hitEnemy = true;
                    // EnemyのHPを減らす処理
                    var enemyScript = hit.gameObject.GetComponent<Enemy>();
                    if (enemyScript != null)
                    {
                        enemyScript.TakeDamage(attackPower);
                        Debug.Log($"Enemyに{attackPower}ダメージを与えた");
                    }
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

    public void PlaySkillAnimation(int animValue, float duration)
    {
        if (animatorController != null)
        {
            animatorController.SetInt("Animation," + animValue);
            isSkillAnimPlaying = true;
            skillAnimTimer = duration;
        }
    }

    void Update()
    {
        if (isDead) return;
        // スキルアニメーション優先
        if (isSkillAnimPlaying)
        {
            skillAnimTimer -= Time.deltaTime;
            if (skillAnimTimer <= 0f)
            {
                isSkillAnimPlaying = false;
            }
            return;
        }
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

   

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        health -= damage;
   //     Debug.Log($"ダメージ！残りHP: {health}");
        if (LifeManager.instance != null)
        {
            LifeManager.instance.TakeDamage(damage);
        }
        if (health <= 0)
        {
       //     Debug.Log("プレイヤー死亡");
            // 死亡処理
        }
        else
        {
            StartCoroutine(InvincibleCoroutine());
        }
    }

    public void StartInvincibility()
    {
        if (!isInvincible)
            StartCoroutine(InvincibleCoroutine());
    }

    public void SetDead(bool dead)
    {
        isDead = dead;
        if (dead)
        {
            // 入力・物理停止
            moveInput = Vector2.zero;
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            if (animatorController != null)
            {
                animatorController.SetFloat("Speed,0");
            }
        }
        else
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }

    public void BoostNextJump()
    {
        nextJumpBoosted = true;
    }

    private System.Collections.Generic.IEnumerator<UnityEngine.WaitForSeconds> InvincibleCoroutine()
    {
        isInvincible = true;
        yield return new UnityEngine.WaitForSeconds(2f);
        isInvincible = false;
    }
}