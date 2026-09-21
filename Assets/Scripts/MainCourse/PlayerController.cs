using UnityEngine;
using UnityEngine.InputSystem;

namespace MainCourse
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 7f;
        public float sprintSpeed = 11f;
        public float groundAcceleration = 16f;
        public float airAcceleration = 10f;
        public float gravity = -30f;
        public float maxFallSpeed = -42f;
        public float jumpHeight = 2.3f;
        public float coyoteTime = 0.18f;
        public float jumpBufferTime = 0.14f;

        [Header("Look")]
        public float mouseSensitivity = 0.16f;
        public float pitchLimit = 85f;
        public Transform cameraPivot;
        public float sprintFov = 84f;
        public float fovLerpSpeed = 8f;

        [Header("Feel")]
        public float bobRate = 9f;
        public float bobAmplitude = 0.028f;

        [Header("Arms")]
        public Transform armsRoot;

        [Header("Dash")]
        public float dashSpeed = 24f;
        public float dashDuration = 0.16f;
        public float dashCooldown = 0.9f;
        public float dashTapWindow = 0.22f;
        public float dashFov = 93f;
        public float dashTimeScale = 0.2f;
        public TrailRenderer dashTrail;
        public event System.Action OnDash;

        [Header("Slide")]
        public float slideHeight = 0.85f;
        public float slideFriction = 1.6f;
        public float slideFov = 96f;
        public float crouchCamYScale = 0.6f;

        Camera cam;
        float startFov;
        float bobPhase;
        Vector3 camBasePos;
        ArmsAnimator arms;
        float shiftHoldTimer;
        float dashTimer;
        float dashCooldownTimer;
        Vector3 dashDir;
        bool trailOn;
        bool sliding;
        bool sprintHeld;
        bool ctrlHeld;
        float capsuleHeight;
        float baseCapsuleHeight;

        [Header("Wall Run")]
        public float wallCheckDistance = 0.95f;
        public float wallRunGravityScale = 0.35f;
        public float wallRunMaxTime = 1.6f;
        public float wallJumpForce = 9f;

        [Header("Fall")]
        public float killY = -15f;

        CharacterController cc;
        Vector3 velocity;
        Vector3 wishInput;
        float yaw;
        float pitch;
        float coyoteTimer;
        float jumpBufferTimer;
        float wallRunTimer;
        float speed;
        bool wallRunning;
        bool hasWall;
        Vector3 wallSurfaceNormal;

        public bool IsGrounded { get { return cc != null && cc.isGrounded; } }
        public bool WallRunning { get { return wallRunning; } }
        public float Speed { get { return speed; } }
        public bool IsMoving
        {
            get { return new Vector2(velocity.x, velocity.z).sqrMagnitude > 0.04f; }
        }

        public bool Dashing { get { return dashTimer > 0f; } }
        public bool DashReady { get { return dashCooldownTimer <= 0f; } }
        public bool Sliding { get { return sliding; } }

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            baseCapsuleHeight = cc.height;
            capsuleHeight = cc.height;
            if (cameraPivot == null)
                cameraPivot = transform.Find("Camera");
            if (cameraPivot != null)
            {
                cam = cameraPivot.GetComponent<Camera>();
                camBasePos = cameraPivot.localPosition;
                arms = cameraPivot.GetComponentInChildren<ArmsAnimator>(true);
            }
            if (arms != null) arms.enabled = true;
            if (cam != null) startFov = cam.fieldOfView;
        }

        void OnEnable()
        {
            if (Mouse.current != null)
                Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            HandleLook();
            ReadInput();
            UpdateTimers();
            UpdateSlideCapsule();
            Move();
            ApplyFovAndBob();
            CheckKillFloor();
        }

        void OnDestroy()
        {
            RestoreTimeScale();
        }

        void HandleLook()
        {
            if (Mouse.current == null) return;
            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * mouseSensitivity;
            pitch -= delta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void ReadInput()
        {
            var kb = Keyboard.current;
            Vector2 m = Vector2.zero;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) m.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) m.y -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) m.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) m.x -= 1f;
                if (kb.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
                speed = kb.shiftKey.isPressed ? sprintSpeed : walkSpeed;
                sprintHeld = kb.shiftKey.isPressed;
                ctrlHeld = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;

                if (kb.shiftKey.wasPressedThisFrame)
                    shiftHoldTimer = 0f;
                else if (kb.shiftKey.isPressed)
                    shiftHoldTimer += Time.deltaTime;
                if (kb.shiftKey.wasReleasedThisFrame &&
                    shiftHoldTimer >= 0f && shiftHoldTimer <= dashTapWindow &&
                    dashCooldownTimer <= 0f)
                    StartDash();
            }
            wishInput = new Vector3(m.x, 0f, m.y);
            if (wishInput.sqrMagnitude > 1f) wishInput.Normalize();
            wishInput = transform.TransformDirection(wishInput);
        }

        void UpdateTimers()
        {
            if (cc.isGrounded) coyoteTimer = coyoteTime;
            else coyoteTimer -= Time.deltaTime;

            if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
            if (wallRunTimer > 0f) wallRunTimer -= Time.deltaTime;

            if (dashTimer > 0f)
            {
                dashTimer -= Time.unscaledDeltaTime;
                if (dashTimer <= 0f) RestoreTimeScale();
            }
            else if (trailOn)
            {
                ApplyTrail(false);
            }
            if (dashCooldownTimer > 0f)
                dashCooldownTimer -= Time.deltaTime;

            if (!cc.isGrounded && coyoteTimer <= 0f)
                DetectWall();
            else
            {
                hasWall = false;
                wallRunning = false;
            }
        }

        void DetectWall()
        {
            hasWall = false;
            Vector3 origin = transform.position + Vector3.up * 0.6f;
            float probeRadius = Mathf.Max(cc.radius, 0.4f) * 0.25f;
            RaycastHit hit;
            Vector3 right = transform.right;
            if (Physics.SphereCast(origin, probeRadius, right, out hit, wallCheckDistance))
            {
                hasWall = true;
                wallSurfaceNormal = hit.normal;
                return;
            }
            if (Physics.SphereCast(origin, probeRadius, -right, out hit, wallCheckDistance))
            {
                hasWall = true;
                wallSurfaceNormal = hit.normal;
            }
        }

        void Move()
        {
            bool dashing = dashTimer > 0f;
            if (dashing)
            {
                float sdt = Time.unscaledDeltaTime;
                velocity = new Vector3(dashDir.x * dashSpeed, velocity.y, dashDir.z * dashSpeed);
                velocity.y = Mathf.Lerp(velocity.y, 0f, 8f * sdt);
                if (cc.isGrounded) velocity.y = -1f;
                cc.Move(velocity * sdt);
                return;
            }

            bool grounded = cc.isGrounded;
            float accel = grounded ? groundAcceleration : airAcceleration;

            Vector3 hor = velocity;
            hor.y = 0f;

            if (ctrlHeld && !sliding && grounded && sprintHeld && hor.sqrMagnitude > walkSpeed * walkSpeed * 0.6f)
                sliding = true;
            if (sliding && (!ctrlHeld || !grounded || hor.sqrMagnitude < walkSpeed * walkSpeed * 0.32f))
                sliding = false;

            if (sliding)
            {
                float damp = Mathf.Clamp01(1f - slideFriction * Time.deltaTime);
                hor *= damp;
                hor += wishInput * 2.2f * Time.deltaTime;
                speed = Mathf.Max(hor.magnitude, walkSpeed * 0.6f);
            }
            else
            {
                hor = Vector3.Lerp(hor, wishInput * speed, accel * Time.deltaTime);
                if (hor.sqrMagnitude > speed * speed) hor = hor.normalized * speed;
            }

            bool wantJump = jumpBufferTimer > 0f;

            if (!grounded && coyoteTimer <= 0f && hasWall && !wantJump)
            {
                float towardWall = Vector3.Dot(hor, -wallSurfaceNormal);
                if (towardWall > 0.01f && velocity.y < 2f)
                {
                    if (!wallRunning) wallRunTimer = wallRunMaxTime;
                    wallRunning = true;
                    hor += -wallSurfaceNormal * 1.2f;
                }
                else wallRunning = false;
            }
            else wallRunning = false;

            if (wallRunning && wallRunTimer <= 0f)
                wallRunning = false;

            if (grounded && velocity.y < 0f)
                velocity.y = -2f;

            if (wantJump)
            {
                if (coyoteTimer > 0f)
                {
                    velocity.y = JumpSpeed();
                    jumpBufferTimer = 0f;
                    coyoteTimer = 0f;
                }
                else if (wallRunning)
                {
                    velocity.y = JumpSpeed();
                    velocity += -wallSurfaceNormal * wallJumpForce;
                    jumpBufferTimer = 0f;
                    wallRunning = false;
                }
            }

            if (wallRunning && !grounded && !wantJump)
            {
                velocity.y += gravity * wallRunGravityScale * Time.deltaTime;
                if (velocity.y < -6f) velocity.y = -6f;
            }
            else if (!grounded)
            {
                velocity.y += gravity * Time.deltaTime;
                if (velocity.y < maxFallSpeed) velocity.y = maxFallSpeed;
            }

            velocity = new Vector3(hor.x, velocity.y, hor.z);
            cc.Move(velocity * Time.deltaTime);
        }

        float JumpSpeed()
        {
            return Mathf.Sqrt(2f * -gravity * jumpHeight);
        }

        void ApplyFovAndBob()
        {
            bool sprinting = speed > walkSpeed + 0.5f;

            if (cc.isGrounded && IsMoving)
                bobPhase += Time.deltaTime * (bobRate + speed * 0.55f);

            if (cam != null)
            {
                float target = Dashing ? dashFov :
                    sliding ? slideFov :
                    (sprinting && cc.isGrounded) ? sprintFov : startFov;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, target, fovLerpSpeed * Time.deltaTime);
            }

            if (cameraPivot != null)
            {
                float camY = sliding ? camBasePos.y * crouchCamYScale : camBasePos.y;
                float bob = (cc.isGrounded && IsMoving && !sliding) ? Mathf.Sin(bobPhase) * bobAmplitude : 0f;
                cameraPivot.localPosition = new Vector3(camBasePos.x, camY + bob, camBasePos.z);
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (GameManager.Instance == null) return;
            if (Dashing) return;
            if (hit.collider == null) return;
            if (hit.collider.GetComponentInParent<Hazard>() != null)
                GameManager.Instance.PlayerHitHazard();
        }

        void StartDash()
        {
            Vector3 dir = wishInput.sqrMagnitude > 0.01f ? wishInput.normalized : transform.forward;
            dashDir = dir;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            velocity = dashDir * dashSpeed;
            velocity.y = 0f;
            Time.timeScale = dashTimeScale;
            ApplyTrail(true);
            if (dashTrail != null) dashTrail.time = 0.34f;
            if (OnDash != null) OnDash();
        }

        void ApplyTrail(bool on)
        {
            trailOn = on;
            if (dashTrail != null) dashTrail.emitting = on;
        }

        void UpdateSlideCapsule()
        {
            if (cc == null) return;
            float target = sliding ? slideHeight : baseCapsuleHeight;
            if (Mathf.Abs(capsuleHeight - target) < 0.002f) return;
            capsuleHeight = Mathf.MoveTowards(capsuleHeight, target, 9f * Time.deltaTime);
            if (!cc.enabled) return;
            float dh = capsuleHeight - cc.height;
            cc.enabled = false;
            transform.position += Vector3.up * (dh * 0.5f);
            cc.height = capsuleHeight;
            cc.center = new Vector3(0f, capsuleHeight * 0.5f, 0f);
            cc.enabled = true;
        }

        void RestoreTimeScale()
        {
            if (HUDController.IsPaused) return;
            if (Mathf.Abs(Time.timeScale - 1f) > 0.001f)
                Time.timeScale = 1f;
        }

        void CheckKillFloor()
        {
            if (!cc.enabled) return;
            if (transform.position.y < killY)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.PlayerFell();
            }
        }

        public void Teleport(Transform target)
        {
            if (target == null) return;
            Quaternion rot = target.rotation;
            rot = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);
            Teleport(target.position, rot);
        }

        public void Teleport(Vector3 pos, Quaternion rot)
        {
            bool usable = cc.enabled;
            cc.enabled = false;
            velocity = Vector3.zero;
            dashTimer = 0f;
            ApplyTrail(false);
            shiftHoldTimer = 9f;
            sliding = false;
            capsuleHeight = baseCapsuleHeight;
            cc.height = baseCapsuleHeight;
            cc.center = new Vector3(0f, baseCapsuleHeight * 0.5f, 0f);
            RestoreTimeScale();
            transform.SetPositionAndRotation(pos, rot);
            cc.enabled = usable;
        }
    }
}