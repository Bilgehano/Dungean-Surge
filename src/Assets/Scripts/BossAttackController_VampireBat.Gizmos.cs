using UnityEngine;

public partial class BossAttackController_VampireBat
{
    private void OnDrawGizmos()
    {
        BossController controller = bossController != null
            ? bossController
            : GetComponent<BossController>();

        if (controller == null)
        {
            return;
        }

        if (!showNormalMovesetGizmos &&
            !showEnhancedMovesetGizmos)
        {
            return;
        }

        SpriteRenderer bossSpriteRenderer =
            GetComponent<SpriteRenderer>();

        bool facesRight =
            bossSpriteRenderer != null &&
            bossSpriteRenderer.flipX;

        float direction = facesRight ? 1f : -1f;

        Vector2 facingDirection =
            facesRight
                ? Vector2.right
                : Vector2.left;

        Vector3 normalOrigin =
            controller.GetAttackOrigin(normalAttackCenter);

        Vector3 heavyOrigin =
            controller.GetAttackOrigin(heavyAttackCenter);

        Vector3 stompOrigin =
            controller.GetAttackOrigin(stompAttackCenter);

        Vector3 castOrigin =
            controller.GetAttackOrigin(castAttackCenter);

        Transform castPreviewTransform =
            castProjectileSpawnPoint != null
                ? castProjectileSpawnPoint
                : castAttackCenter;

        Vector3 castSpawnOrigin =
            controller.GetAttackOrigin(castPreviewTransform);

        if (showNormalMovesetGizmos)
        {
            DrawAttackBoxGizmo(
                normalOrigin,
                direction,
                normalAttackWidth,
                normalAttackHeight,
                HexColor("#2196F3")
            );

            DrawAttackBoxGizmo(
                heavyOrigin,
                direction,
                heavyAttackWidth,
                heavyAttackHeight,
                HexColor("#F44336")
            );

            DrawEllipseGizmo(
                stompOrigin,
                stompAttackWidth,
                stompAttackHeight,
                HexColor("#FF4FD8")
            );

            Gizmos.color = HexColor("#4CAF50");

            Gizmos.DrawWireSphere(
                castOrigin,
                castMaxRange
            );

            Gizmos.DrawWireSphere(
                castOrigin,
                castMinRange
            );

            DrawCastEruptionGizmos(
                castSpawnOrigin,
                facingDirection,
                1,
                0f,
                castEruptionCount,
                castEruptionStartDistance,
                castEruptionSpacing,
                castEruptionWidth,
                castEruptionHeight,
                HexColor("#4CAF50")
            );
        }

        if (showEnhancedMovesetGizmos)
        {
            DrawAttackBoxGizmo(
                normalOrigin,
                direction,
                enhancedNormalAttackWidth,
                enhancedNormalAttackHeight,
                HexColor("#0D47A1")
            );

            DrawAttackBoxGizmo(
                heavyOrigin,
                direction,
                enhancedHeavyAttackWidth,
                enhancedHeavyAttackHeight,
                HexColor("#8B0000")
            );

            DrawEllipseGizmo(
                stompOrigin,
                enhancedStompAttackWidth,
                enhancedStompAttackHeight,
                HexColor("#8E24AA")
            );

            if (enableSecondStompEllipse)
            {
                DrawEllipseRingGizmo(
                    stompOrigin,
                    enhancedStompAttackWidth,
                    enhancedStompAttackHeight,
                    secondStompAttackWidth,
                    secondStompAttackHeight,
                    HexColor("#FF9800")
                );
            }

            Gizmos.color = HexColor("#1B5E20");

            Gizmos.DrawWireSphere(
                castOrigin,
                enhancedCastMaxRange
            );

            Gizmos.DrawWireSphere(
                castOrigin,
                enhancedCastMinRange
            );

            DrawCastEruptionGizmos(
                castSpawnOrigin,
                facingDirection,
                enhancedCastLineCount,
                enhancedCastSideAngle,
                enhancedCastEruptionCount,
                enhancedCastEruptionStartDistance,
                enhancedCastEruptionSpacing,
                enhancedCastEruptionWidth,
                enhancedCastEruptionHeight,
                HexColor("#1B5E20")
            );
        }
    }

    private void DrawCastEruptionGizmos(
        Vector3 origin,
        Vector2 baseDirection,
        int lineCount,
        float sideAngle,
        int eruptionCount,
        float startDistance,
        float spacing,
        float width,
        float height,
        Color color)
    {
        lineCount = Mathf.Max(1, lineCount);
        eruptionCount = Mathf.Max(1, eruptionCount);

        for (int lineIndex = 0;
             lineIndex < lineCount;
             lineIndex++)
        {
            Vector2 lineDirection =
                GetCastLineDirection(
                    baseDirection,
                    lineIndex,
                    lineCount,
                    sideAngle
                );

            for (int eruptionIndex = 0;
                 eruptionIndex < eruptionCount;
                 eruptionIndex++)
            {
                float distance =
                    startDistance +
                    spacing * eruptionIndex;

                Vector3 eruptionPosition =
                    origin +
                    (Vector3)(lineDirection * distance);

                DrawEllipseGizmo(
                    eruptionPosition,
                    width,
                    height,
                    color
                );
            }
        }
    }

    private void DrawAttackBoxGizmo(
        Vector3 origin,
        float direction,
        float width,
        float height,
        Color color)
    {
        Vector3 boxCenter =
            origin +
            Vector3.right * direction *
            (width * 0.5f);

        Gizmos.color = color;

        Gizmos.DrawWireCube(
            boxCenter,
            new Vector3(
                width,
                height,
                0.1f
            )
        );
    }

    private void DrawEllipseGizmo(
        Vector3 origin,
        float width,
        float height,
        Color color)
    {
        Matrix4x4 previousMatrix =
            Gizmos.matrix;

        Gizmos.color = color;

        Gizmos.matrix = Matrix4x4.TRS(
            origin,
            Quaternion.identity,
            new Vector3(
                width,
                height,
                0.1f
            )
        );

        Gizmos.DrawWireSphere(
            Vector3.zero,
            0.5f
        );

        Gizmos.matrix = previousMatrix;
    }

    private void DrawEllipseRingGizmo(
        Vector3 origin,
        float innerWidth,
        float innerHeight,
        float outerWidth,
        float outerHeight,
        Color color)
    {
        DrawEllipseGizmo(
            origin,
            outerWidth,
            outerHeight,
            color
        );

        DrawEllipseGizmo(
            origin,
            innerWidth,
            innerHeight,
            color
        );
    }

    private Color HexColor(
        string hex)
    {
        if (ColorUtility.TryParseHtmlString(
                hex,
                out Color color))
        {
            return color;
        }

        return Color.white;
    }
}