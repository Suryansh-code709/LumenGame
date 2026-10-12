using UnityEngine;

public class GoalGate2D : MonoBehaviour
{
    private LumenGame game;
    private SpriteRenderer frame;
    private bool unlocked;

    public void Configure(LumenGame owner) { game = owner; }

    public void UpdateLock(bool isUnlocked)
    {
        unlocked = isUnlocked;
        if (frame == null)
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
                if (renderer.gameObject.name == "Gate Frame") frame = renderer;
        }
        if (frame != null) frame.color = unlocked ? new Color(1f, .78f, .44f) : new Color(.5f, .45f, .67f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (unlocked && other.GetComponent<PlayerController2D>() != null && game != null)
            game.TouchGate();
    }
}
