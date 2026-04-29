using UnityEngine;

public class Enemy_Movement : MonoBehaviour

{
        [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private string idleStateName = "Idle";

        private Rigidbody2D rb;
        private Vector3 baseScale;
        public Transform player;
        private EnemyState enemyState;
        public float attackRange = 2f;
        private Animator anim;
    private bool isAttackInProgress;
    private float idleUntilTime;

        



    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Enemy_Movement requires a Rigidbody2D component.", this);
            enabled = false;
            return;
        }

        baseScale = transform.localScale;
        rb.freezeRotation = true;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        ChangeState(EnemyState.Chasing);

    }


    void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        switch (enemyState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Chasing:
            case EnemyState.Attacking:
                HandleCombat();
                break;
        }
    }

    void HandleIdle()
    {
        rb.linearVelocity = Vector2.zero;

        if (player == null)
        {
            return;
        }

        FacePlayer();
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        if (Time.time >= idleUntilTime)
        {
            ChangeState(EnemyState.Attacking);
            isAttackInProgress = true;
        }
    }

    void HandleCombat()
    {
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        FacePlayer();

        if (isAttackInProgress)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            isAttackInProgress = true;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        ChangeState(EnemyState.Chasing);
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    // Call this from the last frame of the attack animation via Animation Event.
    public void OnAttackAnimationFinished()
    {
        isAttackInProgress = false;
        idleUntilTime = Time.time + attackCooldown;
        ChangeState(EnemyState.Idle);

        // Fallback: force the animator to leave Attack even if transitions are misconfigured.
        if (anim != null)
        {
            int idleHash = Animator.StringToHash(idleStateName);
            if (anim.HasState(0, idleHash))
            {
                anim.CrossFade(idleHash, 0.05f, 0);
            }
            else
            {
                Debug.LogWarning("Idle state name is not found on Animator layer 0: " + idleStateName, this);
            }
        }
    }

    void FacePlayer()
    {
        float horizontalOffset = player.position.x - transform.position.x;

        if (Mathf.Abs(horizontalOffset) < 0.01f)
        {
            return;
        }

        float facingX = horizontalOffset > 0 ? Mathf.Abs(baseScale.x) : -Mathf.Abs(baseScale.x);
        transform.localScale = new Vector3(facingX, baseScale.y, baseScale.z);
    }


    void ChangeState(EnemyState newState)
    {
        if (anim == null)
        {
            enemyState = newState;
            return;
        }

        if (newState == EnemyState.Idle)
        {
            anim.SetBool("isMoving", false);
            anim.SetBool("isIdle", true);
            anim.SetBool("isAttacking", false);
            
        }
        else if (newState == EnemyState.Chasing)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isMoving", true);
            anim.SetBool("isAttacking", false);
            
        }
        else if (newState == EnemyState.Attacking)
        {
            anim.SetBool("isMoving", false);
            anim.SetBool("isIdle", false);
            anim.SetBool("isAttacking", true);
        }

        enemyState = newState;
    }
    
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking
}

