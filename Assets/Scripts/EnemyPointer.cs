using UnityEngine;

public class EnemyPointer : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 10f;
    public float hoverDistance = 1.5f;
    public float hideDistance = 4f; // Hide if closest enemy is closer than this

    [Header("Pixel Animation")]
    public Sprite[] animationFrames;
    public float animationFPS = 8f;

    private Transform player;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        if (mainCamera == null) mainCamera = Camera.main;

        Transform closestEnemy = FindTargetEnemy();

        if (closestEnemy != null)
        {
            if (spriteRenderer != null && !spriteRenderer.enabled) 
                spriteRenderer.enabled = true;

            // Position and Rotate
            Vector3 direction = (closestEnemy.position - player.position).normalized;
            transform.position = player.position + direction * hoverDistance;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // Pixel Animation swapping
            if (animationFrames != null && animationFrames.Length > 0 && spriteRenderer != null)
            {
                int frameIndex = Mathf.FloorToInt(Time.time * animationFPS) % animationFrames.Length;
                spriteRenderer.sprite = animationFrames[frameIndex];
            }
        }
        else
        {
            if (spriteRenderer != null && spriteRenderer.enabled) 
                spriteRenderer.enabled = false;
        }
    }

    private Transform FindTargetEnemy()
    {
        Enemy_Health[] normalEnemies = Object.FindObjectsByType<Enemy_Health>(FindObjectsInactive.Exclude);
        BossHealth[] bossEnemies = Object.FindObjectsByType<BossHealth>(FindObjectsInactive.Exclude);

        int totalCount = normalEnemies.Length + bossEnemies.Length;
        if (totalCount == 0) return null;

        bool anyVisible = false;
        Transform closest = null;
        float minDistance = float.MaxValue;

        // Check normal enemies for visibility and find closest
        foreach (var enemy in normalEnemies)
        {
            if (IsOnScreen(enemy.transform.position))
            {
                anyVisible = true;
                break;
            }

            float dist = Vector3.Distance(player.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy.transform;
            }
        }

        // Check bosses if no normal enemy was visible
        if (!anyVisible)
        {
            foreach (var boss in bossEnemies)
            {
                if (IsOnScreen(boss.transform.position))
                {
                    anyVisible = true;
                    break;
                }

                float dist = Vector3.Distance(player.position, boss.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = boss.transform;
                }
            }
        }

        // Rule: Hide if ANY enemy is on screen
        if (anyVisible) return null;

        // Rule: Hide if closest enemy is too close
        if (closest != null && minDistance < hideDistance) return null;

        return closest;
    }

    private bool IsOnScreen(Vector3 position)
    {
        if (mainCamera == null) return false;
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
    }
}
