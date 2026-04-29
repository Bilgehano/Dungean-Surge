using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float defaultKnockbackForce = 8f;
    public float defaultStunTime = 0.35f;
    public float blinkInterval = 0.08f;


    public Player_Combat playerCombat;


    public Rigidbody2D rb;


    public int facingDirection = 1; // 1 for right, -1 for left
    
    public Animator anim;

    private bool isKnockedBack;
    private Coroutine knockbackRoutine;
    private SpriteRenderer[] spriteRenderers;

    public bool IsHitImmune => isKnockedBack;

    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    void Start()
    {
        rb.freezeRotation = true;
    }
    void Update()
    {
       if(Input.GetButtonDown("Fire1"))
        {
            playerCombat.Attack();
        }
    }
    void FixedUpdate()
    {
        if (isKnockedBack)
        {
            anim.SetFloat("horizontal", 0f);
            anim.SetFloat("vertical", 0f);
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (horizontalInput > 0)
        {
            facingDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput < 0)
        {
            facingDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }


        anim.SetFloat("horizontal", Mathf.Abs(horizontalInput));
        anim.SetFloat("vertical", Mathf.Abs(verticalInput));



        rb.linearVelocity = new Vector2(horizontalInput, verticalInput).normalized * moveSpeed;



    }

    public void ApplyKnockback(Vector2 enemyPosition, float stunTime, float knockbackForce)
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            return;
        }

        if (isKnockedBack)
        {
            return;
        }

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        Vector2 pushDirection = ((Vector2)transform.position - enemyPosition).normalized;
        if (pushDirection.sqrMagnitude < 0.0001f)
        {
            pushDirection = new Vector2(-facingDirection, 0f);
        }

        knockbackRoutine = StartCoroutine(KnockbackRoutine(pushDirection, stunTime, knockbackForce));
    }

    public void ApplyKnockback(Vector2 enemyPosition)
    {
        ApplyKnockback(enemyPosition, defaultStunTime, defaultKnockbackForce);
    }

    private System.Collections.IEnumerator KnockbackRoutine(Vector2 pushDirection, float stunTime, float knockbackForce)
    {
        float safeStunTime = Mathf.Max(0.01f, stunTime);
        float safeBlinkInterval = Mathf.Max(0.02f, blinkInterval);

        isKnockedBack = true;
        rb.linearVelocity = pushDirection * knockbackForce;

        float elapsed = 0f;
        float blinkElapsed = 0f;
        bool visible = true;

        while (elapsed < safeStunTime)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            blinkElapsed += dt;

            if (blinkElapsed >= safeBlinkInterval && spriteRenderers != null && spriteRenderers.Length > 0)
            {
                blinkElapsed = 0f;
                visible = !visible;
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                    {
                        spriteRenderers[i].enabled = visible;
                    }
                }
            }

            yield return null;
        }

        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].enabled = true;
                }
            }
        }

        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
        knockbackRoutine = null;
    }


}
