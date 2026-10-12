using UnityEngine;

public class WispHazard : MonoBehaviour
{
    private Vector2 center;
    private float patrolWidth;
    private float seed;

    public void Configure(Vector2 start, float patrol)
    {
        center = start;
        patrolWidth = patrol;
        seed = start.x * .41f;
    }

    private void Update()
    {
        float x = Mathf.Sin(Time.time * .85f + seed) * patrolWidth;
        float y = Mathf.Sin(Time.time * 1.8f + seed) * .24f;
        transform.position = new Vector3(center.x + x, center.y + y, 0);
        float pulse = 1f + Mathf.Sin(Time.time * 3f + seed) * .08f;
        transform.localScale = Vector3.one * (.72f * pulse);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController2D>();
        if (player != null) player.OnWispHit();
    }
}
