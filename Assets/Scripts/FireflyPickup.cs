using UnityEngine;

public class FireflyPickup : MonoBehaviour
{
    private LumenGame game;
    private float phaseOffset;
    private Vector3 startPosition;

    public void Configure(LumenGame owner, float offset)
    {
        game = owner;
        phaseOffset = offset;
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * 2.2f + phaseOffset * 8) * .16f);
        transform.Rotate(0, 0, 38f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController2D>() == null || game == null || !game.IsPlaying) return;
        game.CollectFirefly();
        Destroy(gameObject);
    }
}
