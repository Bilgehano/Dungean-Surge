using UnityEngine;

public class Elevation_Exit : MonoBehaviour
{
    [Header("Environment Colliders")]
    public Collider2D[] mountainCollider;
    public Collider2D[] boundaryCollider;

    [Header("Rendering")]
    [SerializeField] private int sortingOrderOnExit = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ApplyExit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        ApplyExit(collision);
    }

    private void ApplyExit(Collider2D collision)
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
                    Physics2D.IgnoreCollision(actorCollider, mountain, false);
                }
            }

            for (int j = 0; j < boundaryCollider.Length; j++)
            {
                Collider2D boundary = boundaryCollider[j];
                if (boundary != null)
                {
                    boundary.enabled = false;
                }
            }
        }

        SpriteRenderer[] renderers = actorObject.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder = sortingOrderOnExit;
        }

        for (int i = 0; i < Enemy_Movement.All.Count; i++)
        {
            Enemy_Movement.All[i].Unfreeze();
        }
    }

}
