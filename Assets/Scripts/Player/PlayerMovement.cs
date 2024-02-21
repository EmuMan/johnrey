using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public enum TContactState
    {
        GROUNDED,
        AIRBORNE,
        LEFT_WALL,
        RIGHT_WALL,
    }

    public enum TMovementState
    {
        IDLE,
        WALKING,
        JUMPING,
        FALLING,
    }

    public enum TInputDirection
    {
        LEFT,
        RIGHT,
        NONE,
    }

    public enum TFacingDirection
    {
        LEFT,
        RIGHT,
    }

    public enum TGroundType
    {
        NORMAL,
        ICY,
    }

    public float NormalGroundAcceleration;
    public float IcyGroundAcceleration;
    public float AirAcceleration;
    public float JumpStrengthInitial;
    public float JumpStrengthContinued;
    public float MaxJumpTime;
    public float JumpBuffer;
    public float CoyoteTime;
    public float WallJumpStrength;
    public float MaxMovementSpeed;
    public float MaxWallSlideSpeed;
    public float NormalSlowdownRate;
    public float IcySlowdownRate;

    public Collider2D GroundCollider;
    public Collider2D LeftCollider;
    public Collider2D RightCollider;

    public AudioClip DeathSound;
    public AudioClip JumpSound;
    public AudioClip ShootSound;

    public TContactState ContactState { get; private set; }
    public TMovementState MovementState { get; private set; }
    public TInputDirection InputDirection { get; private set; }
    public TFacingDirection FacingDirection { get; private set; }
    public TGroundType GroundType { get; private set; }
    public bool IsAlive { get; private set; }

    private Rigidbody2D PlayerRigidbody;
    private PlayerShoot ShootingController;
    private AudioSource PlayerAudio;
    private float LastHorizontalInput;
    private float JumpTimer;
    private bool JumpPressedLastUpdate;
    private float TimeSinceJumpLastPressed; // for jump buffering
    private float TimeSinceLastGrounded; // for coyote time

    private int GroundLayerMask;
    private int NormalGroundLayerMask;
    private int IcyGroundLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        PlayerRigidbody = GetComponent<Rigidbody2D>();
        ShootingController = GetComponent<PlayerShoot>();
        PlayerAudio = GetComponent<AudioSource>();
        LastHorizontalInput = 0.0f;
        InputDirection = TInputDirection.NONE;
        JumpTimer = 0.0f;
        JumpPressedLastUpdate = false;
        TimeSinceJumpLastPressed = 0.0f;
        TimeSinceLastGrounded = 0.0f;
        GroundLayerMask = LayerMask.GetMask("Icy Ground", "Normal Ground");
        NormalGroundLayerMask = LayerMask.GetMask("Normal Ground");
        IcyGroundLayerMask = LayerMask.GetMask("Icy Ground");

        IsAlive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive)
        {
            return;
        }
        // these calls have to happen in this order due to dependency
        CalculateInputDirection();
        CalculateContactState();
        CalculateMovementState();
        HandleSlowdown();
        HandleInput();
        HandleJump();
    }

    void CalculateInputDirection()
    {
        var hInput = Input.GetAxis("Horizontal");
        if (hInput > 0.0f)
        {
            if (hInput > LastHorizontalInput || hInput == 1.0f)
            {
                InputDirection = TInputDirection.RIGHT;
                SetFacingDirection(TFacingDirection.RIGHT);
            }
            else
            {
                InputDirection = TInputDirection.NONE;
            }
        }
        else if (hInput < 0.0f)
        {
            if (hInput < LastHorizontalInput || hInput == -1.0f)
            {
                InputDirection = TInputDirection.LEFT;
                SetFacingDirection(TFacingDirection.LEFT);
            }
            else
            {
                InputDirection = TInputDirection.NONE;
            }
        }
        LastHorizontalInput = hInput;
    }

    void HandleInput()
    {
        var hInput = InputDirection == TInputDirection.NONE ?
            0.0f : InputDirection == TInputDirection.RIGHT ?
            1.0f : -1.0f;

        var xVel = PlayerRigidbody.velocity.x;
        var xVelWasOver = Mathf.Abs(xVel) > MaxMovementSpeed;
        var newXVel = xVel;
        if (Mathf.Abs(xVel) < MaxMovementSpeed || Mathf.Sign(xVel) != Mathf.Sign(hInput))
        {
            float accel = AirAcceleration;
            if (ContactState == TContactState.GROUNDED)
            {
                accel = GroundType == TGroundType.NORMAL ?
                    NormalGroundAcceleration : IcyGroundAcceleration;
            }
            var delta = accel * hInput * Time.deltaTime;
            newXVel += delta;
            var xVelIsOver = Mathf.Abs(newXVel) > MaxMovementSpeed;
            if (!xVelWasOver && xVelIsOver)
            {
                newXVel = MaxMovementSpeed * Mathf.Sign(newXVel);
            }
        }

        PlayerRigidbody.velocity = new(
            newXVel,
            PlayerRigidbody.velocity.y
        );
    }

    void HandleJump()
    {
        var origYVel = PlayerRigidbody.velocity.y;
        var newXVel = PlayerRigidbody.velocity.x;
        var newYVel = origYVel;
        if (MovementState == TMovementState.JUMPING)
        {
            if (ContactState == TContactState.AIRBORNE)
            {
                newYVel += JumpStrengthContinued * Time.deltaTime;
            }
            else
            {
                PlayerAudio.PlayOneShot(JumpSound);
                switch (ContactState)
                {
                    case TContactState.GROUNDED:
                        newYVel = JumpStrengthInitial;
                        break;
                    case TContactState.LEFT_WALL:
                        newXVel = WallJumpStrength;
                        newYVel = JumpStrengthInitial;
                        break;
                    case TContactState.RIGHT_WALL:
                        newXVel = -WallJumpStrength;
                        newYVel = JumpStrengthInitial;
                        break;
                }
            }
        }

        PlayerRigidbody.velocity = new(
            newXVel,
            newYVel
        );
    }

    void HandleSlowdown()
    {
        var origXVel = PlayerRigidbody.velocity.x;
        var newXVel = origXVel;
        if (origXVel != 0.0f && ContactState == TContactState.GROUNDED)
        {
            var slowdownRate = GroundType == TGroundType.NORMAL ?
                NormalSlowdownRate : IcySlowdownRate;
            var amount = slowdownRate * Mathf.Sign(origXVel) * Time.deltaTime;
            newXVel = PlayerRigidbody.velocity.x - amount;
            if (Mathf.Sign(origXVel) != Mathf.Sign(newXVel))
            {
                // the sign flipped, so the velocity passed 0 and should be set there
                newXVel = 0.0f;
            }
        }

        var newYVel = PlayerRigidbody.velocity.y;
        if (ContactState == TContactState.LEFT_WALL && FacingDirection == TFacingDirection.LEFT ||
            ContactState == TContactState.RIGHT_WALL && FacingDirection == TFacingDirection.RIGHT)
        {
            newYVel = Mathf.Max(newYVel, -MaxWallSlideSpeed);
        }

        PlayerRigidbody.velocity = new(
            newXVel,
            newYVel
        );
    }

    private void CalculateContactState()
    {
        TimeSinceLastGrounded += Time.deltaTime;

        var groundCollision = false;
        if (GroundCollider.IsTouchingLayers(NormalGroundLayerMask))
        {
            groundCollision = true;
            GroundType = TGroundType.NORMAL;
        }
        else if (GroundCollider.IsTouchingLayers(IcyGroundLayerMask))
        {
            groundCollision = true;
            GroundType = TGroundType.ICY;
        }

        if (groundCollision)
        {
            // if was not grounded the frame before
            if (ContactState != TContactState.GROUNDED)
            {
                if (PlayerRigidbody.velocity.x > 0.0)
                {
                    SetFacingDirection(TFacingDirection.RIGHT);
                }
                else
                {
                    SetFacingDirection(TFacingDirection.LEFT);
                }
            }
            ContactState = TContactState.GROUNDED;
            TimeSinceLastGrounded = 0.0f;
        }
        else if (TimeSinceLastGrounded < CoyoteTime)
        {
            ContactState = TContactState.GROUNDED;
        }
        else
        {
            if (LeftCollider.IsTouchingLayers(GroundLayerMask))
            {
                ContactState = TContactState.LEFT_WALL;
            }
            else if (RightCollider.IsTouchingLayers(GroundLayerMask))
            {
                ContactState = TContactState.RIGHT_WALL;
            }
            else
            {
                ContactState = TContactState.AIRBORNE;
            }
        }
    }

    private void CalculateMovementState()
    {
        JumpTimer += Time.deltaTime;
        TimeSinceJumpLastPressed += Time.deltaTime;
        var jumpHeld = Input.GetAxis("Jump") == 1.0;
        if (jumpHeld)
        {
            if (!JumpPressedLastUpdate)
            {
                TimeSinceJumpLastPressed = 0.0f;
            }

            if (ContactState != TContactState.AIRBORNE && TimeSinceJumpLastPressed < JumpBuffer)
            {
                // the player pressed the jump button on the ground
                MovementState = TMovementState.JUMPING;
                JumpTimer = 0.0f;
                // jumping should kill coyote time and buffer
                TimeSinceLastGrounded = CoyoteTime;
                TimeSinceJumpLastPressed = JumpBuffer;
                return;
            }
            else if (JumpTimer < MaxJumpTime && MovementState == TMovementState.JUMPING)
            {
                // the player is holding the jump button in the air
                MovementState = TMovementState.JUMPING;
                return;
            }

            JumpPressedLastUpdate = true;
        }
        else
        {
            JumpPressedLastUpdate = false;
        }

        if (ContactState == TContactState.GROUNDED)
        {
            MovementState = PlayerRigidbody.velocity.x == 0.0f ?
                TMovementState.IDLE : TMovementState.WALKING;
        }
        else
        {
            MovementState = TMovementState.FALLING;
        }
    }

    public void SetFacingDirection(TFacingDirection newDirection, bool overrideAiming = false)
    {
        if (overrideAiming || ShootingController.BowState == PlayerShoot.TBowState.LOOSE)
        {
            FacingDirection = newDirection;
        }
    }

    public void KillPlayer()
    {
        if (!IsAlive)
        {
            return;
        }
        IsAlive = false;

        PlayerAudio.PlayOneShot(DeathSound);

        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        var currentScene = SceneManager.GetActiveScene().buildIndex;
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(currentScene);
    }
}
