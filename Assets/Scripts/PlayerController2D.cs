using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7.2f;
    [SerializeField] private float jumpImpulse = 10.4f;
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = .16f;

    private Rigidbody2D body;
    private CapsuleCollider2D capsule;
    private LumenGame game;
    private SpriteRenderer bodySprite;
    private Transform visuals;
    private float lastGroundedTime = -10f;
    private float jumpPressedTime = -10f;
    private float dashEndTime;
    private float invulnerableUntil;
    private int facing = 1;
    private bool dashing;
    private bool dashReady = true;
    private float moveInput;

    public void Configure(LumenGame owner, SpriteRenderer mainSprite, Transform visualRoot)
    {
        game = owner;
        bodySprite = mainSprite;
        visuals = visualRoot;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
    }

    public void BeginRun()
    {
        body.simulated = true;
        body.linearVelocity = Vector2.zero;
        enabled = true;
    }

    private void Update()
    {
        if (game == null || !game.IsPlaying || Keyboard.current == null) return;

        moveInput = 0;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput += 1;
        if (Mathf.Abs(moveInput) > .01f)
        {
            facing = moveInput > 0 ? 1 : -1;
            if (visuals != null) visuals.localScale = new Vector3(facing, 1, 1);
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            jumpPressedTime = Time.time;

        if (Time.time - jumpPressedTime < .14f && Time.time - lastGroundedTime < .13f && !dashing)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpImpulse);
            jumpPressedTime = -10f;
            lastGroundedTime = -10f;
        }

        if ((Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame) && dashReady && !dashing)
        {
            dashing = true;
            dashReady = false;
            dashEndTime = Time.time + dashDuration;
            body.gravityScale = 0;
            body.linearVelocity = new Vector2(facing * dashSpeed, 0);
        }

        if (dashing && Time.time >= dashEndTime)
        {
            dashing = false;
            body.gravityScale = 3.2f;
            body.linearVelocity = new Vector2(body.linearVelocity.x, .3f);
        }

        if (bodySprite != null && Time.time < invulnerableUntil)
            bodySprite.enabled = Mathf.FloorToInt(Time.time * 14) % 2 == 0;
        else if (bodySprite != null) bodySprite.enabled = true;
    }

    private void FixedUpdate()
    {
        if (game == null || !game.IsPlaying || dashing) return;
        body.linearVelocity = new Vector2(moveInput * moveSpeed, body.linearVelocity.y);

        Vector2 feet = new Vector2(capsule.bounds.center.x, capsule.bounds.min.y + .03f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(feet, Vector2.down, .16f);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != capsule && !hit.collider.isTrigger)
            {
                lastGroundedTime = Time.time;
                dashReady = true;
                break;
            }
        }
    }

    public void ResetAt(Vector2 position)
    {
        body.linearVelocity = Vector2.zero;
        body.position = position;
        body.gravityScale = 3.2f;
        dashing = false;
        invulnerableUntil = Time.time + 1.35f;
        bodySprite.enabled = true;
    }

    public void SetControl(bool active)
    {
        enabled = active;
        if (!active && body != null) body.linearVelocity = Vector2.zero;
    }

    public void OnWispHit()
    {
        if (Time.time >= invulnerableUntil && game != null)
            game.PlayerHit();
    }
}
