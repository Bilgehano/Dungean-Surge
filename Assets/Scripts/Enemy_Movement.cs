using UnityEngine;

public class Enemy_Movement : MonoBehaviour

{
        [SerializeField] private float moveSpeed = 3f;

        private Rigidbody2D rb;
        private Vector3 baseScale;
        public Transform player;
        private EnemyState enemyState;

        private Animator anim;



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

        FacePlayer();

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
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
            
        }
        else if (newState == EnemyState.Chasing)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isMoving", true);
            
        }
        else if (newState == EnemyState.Attacking)
        {
            // Handle attacking state (e.g., play attack animation)
            anim.Play("Attack");
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

