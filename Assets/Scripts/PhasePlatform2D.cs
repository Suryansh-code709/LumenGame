using UnityEngine;

public enum WorldPhase { Moon, Dawn }

/// <summary>A platform that exists in one phase, or in both when phase is not set.</summary>
public class PhasePlatform2D : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private WorldPhase? phase;
    private bool alwaysSolid;
    private Color phaseColor;

    public void Configure(WorldPhase? value, Color color, bool safe = false)
    {
        phase = value;
        phaseColor = color;
        alwaysSolid = safe || !value.HasValue;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
    }

    public void ApplyPhase(WorldPhase current)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        bool solid = alwaysSolid || phase == current;
        boxCollider.enabled = solid;
        spriteRenderer.color = solid ? phaseColor : new Color(phaseColor.r * .42f, phaseColor.g * .42f, phaseColor.b * .5f, .42f);
    }
}
