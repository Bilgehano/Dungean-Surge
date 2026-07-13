using System.Collections;
using UnityEngine;

public partial class BossAttackController_VampireBat
{
    private void ShowFilledStompWarningWithColor(
        Color color,
        Vector2 stompOrigin,
        float width,
        float height)
    {
        ShowStompWarningWithColor(
            color,
            stompOrigin,
            width,
            height,
            false,
            0f,
            0f
        );
    }

    private void ShowRingStompWarningWithColor(
        Color color,
        Vector2 stompOrigin,
        float innerWidth,
        float innerHeight,
        float outerWidth,
        float outerHeight)
    {
        ShowStompWarningWithColor(
            color,
            stompOrigin,
            outerWidth,
            outerHeight,
            true,
            innerWidth,
            innerHeight
        );
    }

    private void ShowStompWarningWithColor(
        Color color,
        Vector2 stompOrigin,
        float width,
        float height,
        bool useRingSprite,
        float innerWidth,
        float innerHeight)
    {
        if (bossController == null)
        {
            return;
        }

        EnsureStompWarningVisual();
        CancelScheduledStompWarningHide();

        if (stompWarningObject == null ||
            stompWarningRenderer == null)
        {
            return;
        }

        activeStompWarningWidth = Mathf.Max(
            0.01f,
            width
        );

        activeStompWarningHeight = Mathf.Max(
            0.01f,
            height
        );

        if (useRingSprite)
        {
            float innerWidthRatio =
                Mathf.Clamp(
                    innerWidth / activeStompWarningWidth,
                    0.01f,
                    0.99f
                );

            float innerHeightRatio =
                Mathf.Clamp(
                    innerHeight / activeStompWarningHeight,
                    0.01f,
                    0.99f
                );

            stompWarningRenderer.sprite =
                GetOrCreateStompRingWarningSprite(
                    innerWidthRatio,
                    innerHeightRatio
                );
        }
        else
        {
            stompWarningRenderer.sprite =
                GetOrCreateFilledStompWarningSprite();
        }

        stompWarningObject.transform.position =
            new Vector3(
                stompOrigin.x,
                stompOrigin.y,
                transform.position.z
            );

        ApplyStompWarningWorldSize();

        stompWarningRenderer.color = color;
        UpdateStompWarningSorting();

        stompWarningObject.SetActive(true);
    }

    private void ApplyStompWarningWorldSize()
    {
        if (stompWarningObject == null ||
            stompWarningRenderer == null ||
            stompWarningRenderer.sprite == null)
        {
            return;
        }

        ApplyAreaVisualWorldSize(
            stompWarningRenderer,
            activeStompWarningWidth,
            activeStompWarningHeight
        );
    }

    private void EnsureStompWarningVisual()
    {
        if (stompWarningObject != null &&
            stompWarningRenderer != null)
        {
            return;
        }

        Transform existingChild =
            transform.Find(stompWarningObjectName);

        if (existingChild != null)
        {
            stompWarningObject =
                existingChild.gameObject;

            stompWarningRenderer =
                stompWarningObject.GetComponent<SpriteRenderer>();
        }

        if (stompWarningObject == null)
        {
            stompWarningObject =
                new GameObject(stompWarningObjectName);

            stompWarningObject.transform.SetParent(
                transform,
                false
            );

            stompWarningRenderer =
                stompWarningObject.AddComponent<SpriteRenderer>();
        }

        if (stompWarningRenderer == null)
        {
            stompWarningRenderer =
                stompWarningObject.GetComponent<SpriteRenderer>();

            if (stompWarningRenderer == null)
            {
                stompWarningRenderer =
                    stompWarningObject.AddComponent<SpriteRenderer>();
            }
        }

        stompWarningRenderer.sprite =
            GetOrCreateFilledStompWarningSprite();

        stompWarningRenderer.color =
            stompWarningPreHitColor;

        UpdateStompWarningSorting();
    }

    private void UpdateStompWarningSorting()
    {
        if (stompWarningRenderer == null)
        {
            return;
        }

        UpdateAreaVisualSorting(
            stompWarningRenderer
        );
    }

    private void UpdateAreaVisualSorting(
        SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        SpriteRenderer bossSpriteRenderer =
            GetComponent<SpriteRenderer>();

        if (bossSpriteRenderer == null)
        {
            return;
        }

        renderer.sortingLayerID =
            bossSpriteRenderer.sortingLayerID;

        renderer.sortingOrder =
            bossSpriteRenderer.sortingOrder +
            stompWarningSortingOrderOffset;
    }

    private void ApplyAreaVisualWorldSize(
        SpriteRenderer renderer,
        float width,
        float height)
    {
        if (renderer == null ||
            renderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize =
            renderer.sprite.bounds.size;

        float spriteWidth = Mathf.Max(
            spriteSize.x,
            0.0001f
        );

        float spriteHeight = Mathf.Max(
            spriteSize.y,
            0.0001f
        );

        Vector3 parentScale = Vector3.one;

        if (renderer.transform.parent != null)
        {
            parentScale =
                renderer.transform.parent.lossyScale;
        }

        float parentScaleX = Mathf.Max(
            Mathf.Abs(parentScale.x),
            0.0001f
        );

        float parentScaleY = Mathf.Max(
            Mathf.Abs(parentScale.y),
            0.0001f
        );

        renderer.transform.localScale =
            new Vector3(
                width / (spriteWidth * parentScaleX),
                height / (spriteHeight * parentScaleY),
                1f
            );
    }

    private static Sprite GetOrCreateFilledStompWarningSprite()
    {
        if (cachedFilledStompWarningSprite != null)
        {
            return cachedFilledStompWarningSprite;
        }

        const int size = 128;

        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "RuntimeStompWarningTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(
            (size - 1) * 0.5f,
            (size - 1) * 0.5f
        );

        float radius = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixelPosition =
                    new Vector2(x, y);

                float normalizedDistance =
                    Vector2.Distance(
                        pixelPosition,
                        center
                    ) / radius;

                float alpha;

                if (normalizedDistance <= 0.92f)
                {
                    alpha = 1f;
                }
                else if (normalizedDistance <= 1f)
                {
                    alpha = Mathf.InverseLerp(
                        1f,
                        0.92f,
                        normalizedDistance
                    );
                }
                else
                {
                    alpha = 0f;
                }

                texture.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, alpha)
                );
            }
        }

        texture.Apply();

        cachedFilledStompWarningSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        cachedFilledStompWarningSprite.name =
            "RuntimeStompWarningSprite";

        return cachedFilledStompWarningSprite;
    }

    private static Sprite GetOrCreateStompRingWarningSprite(
        float innerWidthRatio,
        float innerHeightRatio)
    {
        innerWidthRatio = Mathf.Clamp(
            innerWidthRatio,
            0.01f,
            0.99f
        );

        innerHeightRatio = Mathf.Clamp(
            innerHeightRatio,
            0.01f,
            0.99f
        );

        bool canReuseCachedRing =
            cachedRingStompWarningSprite != null &&
            Mathf.Approximately(
                cachedRingInnerWidthRatio,
                innerWidthRatio
            ) &&
            Mathf.Approximately(
                cachedRingInnerHeightRatio,
                innerHeightRatio
            );

        if (canReuseCachedRing)
        {
            return cachedRingStompWarningSprite;
        }

        const int size = 128;

        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "RuntimeStompRingWarningTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(
            (size - 1) * 0.5f,
            (size - 1) * 0.5f
        );

        float radius = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixelPosition =
                    new Vector2(x, y);

                float normalizedX =
                    (pixelPosition.x - center.x) / radius;

                float normalizedY =
                    (pixelPosition.y - center.y) / radius;

                float outerDistance =
                    Mathf.Sqrt(
                        normalizedX * normalizedX +
                        normalizedY * normalizedY
                    );

                float innerDistance =
                    Mathf.Sqrt(
                        (normalizedX / innerWidthRatio) *
                        (normalizedX / innerWidthRatio) +
                        (normalizedY / innerHeightRatio) *
                        (normalizedY / innerHeightRatio)
                    );

                float outerAlpha;

                if (outerDistance <= 0.92f)
                {
                    outerAlpha = 1f;
                }
                else if (outerDistance <= 1f)
                {
                    outerAlpha = Mathf.InverseLerp(
                        1f,
                        0.92f,
                        outerDistance
                    );
                }
                else
                {
                    outerAlpha = 0f;
                }

                float innerCutoutAlpha;

                if (innerDistance <= 0.92f)
                {
                    innerCutoutAlpha = 0f;
                }
                else if (innerDistance <= 1f)
                {
                    innerCutoutAlpha = Mathf.InverseLerp(
                        0.92f,
                        1f,
                        innerDistance
                    );
                }
                else
                {
                    innerCutoutAlpha = 1f;
                }

                float finalAlpha =
                    outerAlpha * innerCutoutAlpha;

                texture.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, finalAlpha)
                );
            }
        }

        texture.Apply();

        cachedRingStompWarningSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        cachedRingStompWarningSprite.name =
            "RuntimeStompRingWarningSprite";

        cachedRingInnerWidthRatio = innerWidthRatio;
        cachedRingInnerHeightRatio = innerHeightRatio;

        return cachedRingStompWarningSprite;
    }

    private void EnsureCastEruptionVisualPool(
        int requiredCount)
    {
        requiredCount =
            Mathf.Max(0, requiredCount);

        for (int i = castEruptionRenderers.Count;
             i < requiredCount;
             i++)
        {
            GameObject visualObject =
                new GameObject(
                    $"{castEruptionObjectNamePrefix}_{i}"
                );

            visualObject.transform.SetParent(
                transform,
                false
            );

            SpriteRenderer renderer =
                visualObject.AddComponent<SpriteRenderer>();

            renderer.sprite =
                GetOrCreateFilledStompWarningSprite();

            renderer.color =
                castEruptionWarningColor;

            UpdateAreaVisualSorting(renderer);

            visualObject.SetActive(false);

            castEruptionRenderers.Add(renderer);
        }

        for (int i = 0;
             i < castEruptionRenderers.Count;
             i++)
        {
            if (castEruptionRenderers[i] != null)
            {
                UpdateAreaVisualSorting(
                    castEruptionRenderers[i]
                );
            }
        }
    }

    private void ShowCastEruptionVisual(
        int visualIndex,
        Vector2 position,
        float width,
        float height,
        Color color)
    {
        if (visualIndex < 0 ||
            visualIndex >= castEruptionRenderers.Count)
        {
            return;
        }

        SpriteRenderer renderer =
            castEruptionRenderers[visualIndex];

        if (renderer == null)
        {
            return;
        }

        renderer.sprite =
            GetOrCreateFilledStompWarningSprite();

        renderer.transform.position =
            new Vector3(
                position.x,
                position.y,
                transform.position.z
            );

        ApplyAreaVisualWorldSize(
            renderer,
            width,
            height
        );

        renderer.color = color;
        UpdateAreaVisualSorting(renderer);

        renderer.gameObject.SetActive(true);
    }

    private IEnumerator HideCastEruptionVisualAfter(
        int visualIndex,
        float delay,
        int visualVersion)
    {
        yield return new WaitForSeconds(delay);

        if (visualVersion != castVisualVersion)
        {
            yield break;
        }

        HideCastEruptionVisual(visualIndex);
    }

    private void HideCastEruptionVisual(
        int visualIndex)
    {
        if (visualIndex < 0 ||
            visualIndex >= castEruptionRenderers.Count)
        {
            return;
        }

        SpriteRenderer renderer =
            castEruptionRenderers[visualIndex];

        if (renderer != null)
        {
            renderer.gameObject.SetActive(false);
        }
    }

    private void HideCastEruptionVisuals()
    {
        for (int i = 0;
             i < castEruptionRenderers.Count;
             i++)
        {
            if (castEruptionRenderers[i] != null)
            {
                castEruptionRenderers[i]
                    .gameObject
                    .SetActive(false);
            }
        }
    }

    private void StartStompWarningHideAfterHit()
    {
        CancelScheduledStompWarningHide();

        hideStompWarningCoroutine = StartCoroutine(
            HideStompWarningAfterHitRoutine()
        );
    }

    private IEnumerator HideStompWarningAfterHitRoutine()
    {
        yield return new WaitForSeconds(
            stompWarningAfterHitDuration
        );

        if (stompWarningObject != null)
        {
            stompWarningObject.SetActive(false);
        }

        hideStompWarningCoroutine = null;
    }

    private void CancelScheduledStompWarningHide()
    {
        if (hideStompWarningCoroutine == null)
        {
            return;
        }

        StopCoroutine(hideStompWarningCoroutine);
        hideStompWarningCoroutine = null;
    }

    private void HideStompWarningImmediately()
    {
        CancelScheduledStompWarningHide();

        if (stompWarningObject != null)
        {
            stompWarningObject.SetActive(false);
        }
    }
}