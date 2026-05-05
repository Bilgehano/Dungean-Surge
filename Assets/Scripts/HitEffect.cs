using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.25f;
    [SerializeField] private Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 endScale = new Vector3(1.2f, 1.2f, 1f);

    private float timer;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = startScale;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;
        transform.localScale = Vector3.Lerp(startScale, endScale, t);
        
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - t;
            sr.color = c;
        }
    }
}