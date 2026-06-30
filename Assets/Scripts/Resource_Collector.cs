using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Resource_Collector : MonoBehaviour
{
    [Header("Resource")]
    [SerializeField] private int goldValue = 1;
    [SerializeField] private string playerTag = "Player";

    [Header("Collect Animation")]
    [SerializeField] private float pullSpeed = 12f;
    [SerializeField] private float pullAcceleration = 24f;
    [SerializeField] private float collectDistance = 0.2f;
    [SerializeField] private float vanishDuration = 0.18f;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] [Range(0f, 1f)] private float collectVolume = 1f;

    private Transform targetPlayer;
    private PlayerResources playerResources;
    private bool isCollecting;
    private float currentPullSpeed;
    private Vector3 initialScale;
    private float collectStartTime;
    private Collider2D triggerCollider;
    private SpriteRenderer[] renderers;
    private Color[] initialColors;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        initialColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            initialColors[i] = renderers[i].color;
        }

        initialScale = transform.localScale;
    }

    private void Update()
    {
        if (!isCollecting || targetPlayer == null)
        {
            return;
        }

        currentPullSpeed += pullAcceleration * Time.deltaTime;
        Vector3 targetPos = targetPlayer.position;
        targetPos.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, currentPullSpeed * Time.deltaTime);

        float t = Mathf.Clamp01((Time.time - collectStartTime) / Mathf.Max(0.01f, vanishDuration));
        transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
        ApplyAlpha(1f - t);

        if ((targetPos - transform.position).sqrMagnitude <= collectDistance * collectDistance)
        {
            if (playerResources != null)
            {
                playerResources.AddGold(goldValue);
            }

            if (collectSound != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(collectSound, collectVolume);
                }
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryStartCollect(other);
    }

    private void TryStartCollect(Collider2D other)
    {
        if (isCollecting)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        PlayerResources resources = other.GetComponent<PlayerResources>();
        if (resources == null)
        {
            resources = other.GetComponentInParent<PlayerResources>();
        }
        if (resources == null)
        {
            resources = other.GetComponentInChildren<PlayerResources>();
        }
        if (resources == null)
        {
            return;
        }

        targetPlayer = other.transform;
        playerResources = resources;
        isCollecting = true;
        currentPullSpeed = pullSpeed;
        collectStartTime = Time.time;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private void ApplyAlpha(float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            Color c = initialColors[i];
            c.a *= alpha;
            renderers[i].color = c;
        }
    }
}
