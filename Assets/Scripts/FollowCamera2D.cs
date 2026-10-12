using UnityEngine;

public class FollowCamera2D : MonoBehaviour
{
    private Transform target;
    private Vector3 velocity;

    public void SetTarget(Transform value) { target = value; }

    private void LateUpdate()
    {
        if (target == null) return;
        float x = Mathf.Clamp(target.position.x + 2.5f, -5f, 48f);
        Vector3 desired = new Vector3(x, Mathf.Clamp(target.position.y + 1.1f, .6f, 2.4f), -10f);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, .22f);
    }
}
