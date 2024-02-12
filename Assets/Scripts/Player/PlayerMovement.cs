using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum TMovementState
    {
        GROUNDED,
        AIRBORNE,
    }

    public enum TInputDirection
    {
        LEFT,
        RIGHT,
        NONE,
    }

    public float Acceleration;
    public float JumpStrength;
    public float MaxMovementSpeed;
    public float SlowdownRate;

    public TMovementState MovementState { get; private set; }
    public TInputDirection InputDirection { get; private set; }

    private Rigidbody2D PlayerRigidbody;
    private int CollisionCounter;
    private float LastHorizontalInput;

    // Start is called before the first frame update
    void Start()
    {
        PlayerRigidbody = GetComponent<Rigidbody2D>();
        CollisionCounter = 0;
        LastHorizontalInput = 0.0f;
        InputDirection = TInputDirection.NONE;
    }

    // Update is called once per frame
    void Update()
    {
        CalculateInputDirection();
        HandleSlowdown();
        HandleInput();
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
            var delta = Acceleration * hInput * Time.deltaTime;
            newXVel += delta;
            var xVelIsOver = Mathf.Abs(newXVel) > MaxMovementSpeed;
            if (!xVelWasOver && xVelIsOver)
            {
                newXVel = MaxMovementSpeed * Mathf.Sign(newXVel);
            }
        }

        var newYVel = PlayerRigidbody.velocity.y;
        if (MovementState == TMovementState.GROUNDED && Input.GetAxis("Jump") == 1.0)
        {
            newYVel = JumpStrength;
        }

        PlayerRigidbody.velocity = new(
            newXVel,
            newYVel
        );
    }

    void HandleSlowdown()
    {
        var origVelocity = PlayerRigidbody.velocity.x;
        if (origVelocity == 0.0f)
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
        CalculateMovementState();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CollisionCounter--;
        CalculateMovementState();
    }

    private void CalculateMovementState()
    {
        if (CollisionCounter > 0)
        {
            MovementState = TMovementState.GROUNDED;
        }
        else
        {
            MovementState = TMovementState.AIRBORNE;
        }
    }
}
