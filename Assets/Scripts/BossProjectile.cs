using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float rotationSpeed = 360f;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private Transform targetPlayer;

    private float travelTime;
    private float arcHeight;
    private float impactRadius;
    private int damageAmount;

    private float timer;
    private bool hasHit;

    public void Initialize(
        Vector2 start,
        Vector2 target,
        float duration,
        float height,
        float radius,
        int damage,
        Transform player
    )
    {
        startPosition = start;
        targetPosition = target;
        travelTime = Mathf.Max(0.1f, duration);
        arcHeight = height;
        impactRadius = radius;
        damageAmount = damage;
        targetPlayer = player;

        transform.position = startPosition;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / travelTime);

        Vector2 flatPosition = Vector2.Lerp(startPosition, targetPosition, t);
        Vector2 arcOffset = Vector2.up * Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.position = flatPosition + arcOffset;
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (t >= 1f)
        {
            TryDamageAtImpact();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            hasHit = true;
            playerHealth.ChangeHealth(damageAmount);
            Destroy(gameObject);
        }
    }

    private void TryDamageAtImpact()
    {
        if (hasHit || targetPlayer == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer <= impactRadius)
        {
            PlayerHealth playerHealth = targetPlayer.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                hasHit = true;
                playerHealth.ChangeHealth(damageAmount);
            }
        }
    }
}