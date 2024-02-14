using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum TContactState
    {
        GROUNDED,
        AIRBORNE,
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

    public float GroundAcceleration;
    public float AirAcceleration;
    public float JumpStrengthInitial;
    public float JumpStrengthContinued;
    public float MaxJumpTime;
    public float JumpBuffer;
    public float CoyoteTime;
    public float MaxMovementSpeed;
    public float SlowdownRate;


    public TContactState ContactState { get; private set; }
    public TMovementState MovementState { get; private set; }
    public TInputDirection InputDirection { get; private set; }

    private Rigidbody2D PlayerRigidbody;
    private int CollisionCounter;
    private float LastHorizontalInput;
    private float JumpTimer;
    private bool JumpPressedLastUpdate;
    private float TimeSinceJumpLastPressed; // for jump buffering
    private float TimeSinceLastGrounded; // for coyote time

    // Start is called before the first frame update
    void Start()
    {
        PlayerRigidbody = GetComponent<Rigidbody2D>();
        CollisionCounter = 0;
        LastHorizontalInput = 0.0f;
        InputDirection = TInputDirection.NONE;
        JumpTimer = 0.0f;
        JumpPressedLastUpdate = false;
        TimeSinceJumpLastPressed = 0.0f;
        TimeSinceLastGrounded = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
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
            var accel = ContactState == TContactState.GROUNDED ? GroundAcceleration : AirAcceleration;
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
        var newYVel = origYVel;
        if (MovementState == TMovementState.JUMPING)
        {
            if (ContactState == TContactState.GROUNDED)
            {
                newYVel = JumpStrengthInitial;
            }
            else
            {
                newYVel += JumpStrengthContinued * Time.deltaTime;
            }
        }

        PlayerRigidbody.velocity = new(
            PlayerRigidbody.velocity.x,
            newYVel
        );
    }

    void HandleSlowdown()
    {
        var origVelocity = PlayerRigidbody.velocity.x;
        if (origVelocity == 0.0f || ContactState == TContactState.AIRBORNE)
        {
            return;
        }
        var amount = SlowdownRate * Mathf.Sign(origVelocity) * Time.deltaTime;
        var newXVel = PlayerRigidbody.velocity.x - amount;
        if (Mathf.Sign(origVelocity) != Mathf.Sign(newXVel))
        {
            // the sign flipped, so the velocity passed 0 and should be set there
            newXVel = 0.0f;
        }
        PlayerRigidbody.velocity = new(
            newXVel,
            PlayerRigidbody.velocity.y
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionCounter++;
        CalculateContactState();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CollisionCounter--;
        CalculateContactState();
    }

    private void CalculateContactState()
    {
        TimeSinceLastGrounded += Time.deltaTime;
        if (CollisionCounter > 0)
        {
            ContactState = TContactState.GROUNDED;
            TimeSinceLastGrounded = 0.0f;
        }
        else if (TimeSinceLastGrounded < CoyoteTime)
        {
            ContactState = TContactState.GROUNDED;
        }
        else
        {
            ContactState = TContactState.AIRBORNE;
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

            if (ContactState == TContactState.GROUNDED && TimeSinceJumpLastPressed < JumpBuffer)
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
}
