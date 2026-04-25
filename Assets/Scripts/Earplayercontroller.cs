using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Earplayercontroller : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Walk speed in units per second.")]
    public float moveSpeed = 5f;

    [Tooltip("How fast the player accelerates / decelerates. Higher = snappier.")]
    public float acceleration = 15f;

    [Tooltip("Jump height in Unity units.")]
    public float jumpHeight = 1.2f;

    [Tooltip("Gravity scale. Unity default gravity is -9.81.")]
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    [Tooltip("Horizontal and vertical mouse sensitivity.")]
    public float mouseSensitivity = 2f;

    [Tooltip("Maximum angle the camera can look up.")]
    public float maxLookUp = 80f;

    [Tooltip("Maximum angle the camera can look down.")]
    public float maxLookDown = 80f;

    [Header("Camera Position")]
    [Tooltip("Local offset of the camera from CameraRoot. X = shoulder side, Y = vertical nudge, Z = distance behind player.")]
    public Vector3 cameraOffset = new Vector3(0.5f, 0f, -2.5f);

    [Tooltip("Collision layer mask for camera obstruction check. Set to your geometry layers.")]
    public LayerMask cameraCollisionMask = ~0; // everything by default

    [Header("References")]
    [Tooltip("The empty GameObject that rotates vertically (parent of the camera). Position it at Y = 1.6 on the player.")]
    public Transform cameraRoot;

    [Tooltip("The Main Camera itself. Reset its local position to zero — offset is driven by cameraOffset above.")]
    public Camera playerCamera;

    // -- private state --
    private CharacterController _cc;
    private Vector3 _velocity;          // current XZ movement velocity (smoothed)
    private float _verticalVelocity;    // Y velocity (gravity + jump)
    private float _cameraPitch;         // current vertical camera angle

    // ---------------------------------------------------------------

    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        // Auto-find camera references if not assigned in Inspector
        if (cameraRoot == null)
        {
            cameraRoot = transform.Find("CameraRoot");
            if (cameraRoot == null)
                Debug.LogWarning("[EarPlayerController] No CameraRoot found. Create a child GameObject named 'CameraRoot'.");
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        // Lock and hide cursor for mouse-look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------------------------------------------------------------

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleGravityAndJump();
    }

    // ---------------------------------------------------------------

    /// <summary>
    /// Rotates the player body left/right and the camera root up/down.
    /// </summary>
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // Rotate player body on Y axis (left / right)
        transform.Rotate(Vector3.up * mouseX);

        // Clamp and apply vertical pitch to camera root
        _cameraPitch -= mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -maxLookUp, maxLookDown);

        if (cameraRoot != null)
            cameraRoot.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    // ---------------------------------------------------------------

    /// <summary>
    /// Reads WASD / arrow keys and moves the CharacterController with smoothed acceleration.
    /// </summary>
    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        // Build movement direction relative to where the player is facing
        Vector3 inputDir = (transform.right * h + transform.forward * v).normalized;
        Vector3 targetVelocity = inputDir * moveSpeed;

        // Smooth acceleration / deceleration
        _velocity = Vector3.MoveTowards(_velocity, targetVelocity, acceleration * Time.deltaTime);

        // Apply XZ movement (vertical handled separately)
        Vector3 move = _velocity * Time.deltaTime;
        move.y = 0f;
        _cc.Move(move);
    }

    // ---------------------------------------------------------------

    /// <summary>
    /// Applies gravity and allows jumping when grounded.
    /// </summary>
    void HandleGravityAndJump()
    {
        bool grounded = _cc.isGrounded;

        // Reset downward velocity when grounded
        if (grounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f; // small negative keeps isGrounded reliable

        // Jump: v = sqrt(h * -2 * g)
        if (Input.GetButtonDown("Jump") && grounded)
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Apply gravity
        _verticalVelocity += gravity * Time.deltaTime;

        _cc.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
    }

    // ---------------------------------------------------------------

    /// <summary>
    /// Positions the camera behind the player with wall-collision correction.
    /// Runs after Update so movement is fully applied before we reposition.
    /// Also handles cursor lock/unlock for Editor testing.
    /// </summary>
    void LateUpdate()
    {
        HandleCursorLock();
        HandleCameraPosition();
    }

    // ---------------------------------------------------------------

    /// <summary>
    /// Computes the desired camera position from cameraOffset, then raycasts back
    /// from the CameraRoot pivot. If a wall is in the way, the camera is pushed
    /// forward to the hit point so it never clips inside geometry.
    /// </summary>
    void HandleCameraPosition()
    {
        if (cameraRoot == null || playerCamera == null) return;

        // Desired world-space position = pivot + offset rotated with the camera root
        Vector3 desiredPos = cameraRoot.TransformPoint(cameraOffset);

        // Cast a line from the pivot to the desired camera position
        Vector3 pivot = cameraRoot.position;
        Vector3 dir = desiredPos - pivot;
        float distance = dir.magnitude;

        RaycastHit hit;
        if (Physics.SphereCast(pivot, 0.15f, dir.normalized, out hit, distance, cameraCollisionMask))
        {
            // Pull the camera to just in front of the hit surface (0.1f buffer)
            playerCamera.transform.position = hit.point + hit.normal * 0.1f;
        }
        else
        {
            playerCamera.transform.position = desiredPos;
        }

        // Keep the camera looking in the same direction as CameraRoot
        playerCamera.transform.rotation = cameraRoot.rotation;
    }

    // ---------------------------------------------------------------

    /// <summary>
    /// Escape unlocks the cursor for Editor use; clicking re-locks it.
    /// </summary>
    void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

