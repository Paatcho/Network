using UnityEngine;

public class MarioJumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpVelocity = 12f;
    public float gravity = -30f;
    public float maxFallSpeed = -25f;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground")]
    public float groundY = 0f;

    [Header("Gizmos")]
    public float gizmoLineWidth = 2f;

    private float verticalVelocity = 0f;
    private bool isGrounded = true;

    // 🔹 Apex tracking
    private float currentJumpMaxY;
    private float lastJumpMaxY;
    private bool wasGroundedLastFrame = true;

    void Update()
    {
         float dt = Time.deltaTime;

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            verticalVelocity = jumpVelocity;
            isGrounded = false;

            // Start tracking new jump
            currentJumpMaxY = transform.position.y;
        }

        if (Input.GetKeyUp(jumpKey) && verticalVelocity > 0)
        {
            verticalVelocity *= 0.5f;
        }

        if (!isGrounded)
        {
            verticalVelocity += gravity * dt;
            verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);
        }

        Vector3 pos = transform.position;
        pos.y += verticalVelocity * dt;

        if (pos.y <= groundY)
        {
            pos.y = groundY;
            verticalVelocity = 0;
            isGrounded = true;
        }

        transform.position = pos;

        // -------------------------
        // TRACK APEX
        // -------------------------
        if (!isGrounded)
        {
            currentJumpMaxY = Mathf.Max(currentJumpMaxY, transform.position.y);
        }

        // Detect landing (transition air → ground)
        if (isGrounded && !wasGroundedLastFrame)
        {
            lastJumpMaxY = currentJumpMaxY;
        }

        wasGroundedLastFrame = isGrounded;
    }

    void OnDrawGizmos()
    {
        // Draw horizontal line at last jump height
        if (lastJumpMaxY > groundY)
        {
            Gizmos.color = Color.red;

            Vector3 left = new Vector3(transform.position.x - gizmoLineWidth, lastJumpMaxY, transform.position.z);
            Vector3 right = new Vector3(transform.position.x + gizmoLineWidth, lastJumpMaxY, transform.position.z);

            Gizmos.DrawLine(left, right);
        }
    }
}