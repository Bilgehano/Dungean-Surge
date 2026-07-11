using UnityEngine;

public class Elevation_Entry : MonoBehaviour
{
    [Header("Environment Colliders")]
    public Collider2D[] mountainCollider;
    public Collider2D[] boundaryCollider;

    [Header("Rendering")]
    [SerializeField] private int sortingOrderOnEntry = 15;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ApplyEntry(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        ApplyEntry(collision);
    }

    private void ApplyEntry(Collider2D collision)
    {
        GameObject actorObject = collision.attachedRigidbody != null ? collision.attachedRigidbody.gameObject : collision.gameObject;
        if (!actorObject.CompareTag("Player"))
        {
            return;
        }

        Collider2D[] actorColliders = actorObject.GetComponentsInChildren<Collider2D>(true);

        for (int i = 0; i < actorColliders.Length; i++)
        {
            Collider2D actorCollider = actorColliders[i];
            if (actorCollider == null)
            {
                continue;
            }

            for (int j = 0; j < mountainCollider.Length; j++)
            {
                Collider2D mountain = mountainCollider[j];
                if (mountain != null)
                {
                    Physics2D.IgnoreCollision(actorCollider, mountain, true);
                }
            }

            for (int j = 0; j < boundaryCollider.Length; j++)
            {
                Collider2D boundary = boundaryCollider[j];
                if (boundary != null)
                {
                    boundary.enabled = true;
                }
            }
        }

        SpriteRenderer[] renderers = actorObject.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder = sortingOrderOnEntry;
        }

        for (int i = 0; i < Enemy_Movement.All.Count; i++)
        {
            Enemy_Movement.All[i].Freeze();
        }
    }

}
