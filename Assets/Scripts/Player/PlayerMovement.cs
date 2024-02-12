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

    public float Acceleration;
    public float JumpStrength;
    public float MaxMovementSpeed;
    public float SlowdownRate;

    public TMovementState MovementState { get; private set; }

    private Rigidbody2D PlayerRigidbody;
    private int CollisionCounter;

    // Start is called before the first frame update
    void Start()
    {
        PlayerRigidbody = GetComponent<Rigidbody2D>();
        CollisionCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        HandleSlowdown();
        HandleInput();
    }

    void HandleInput()
    {
        var hInput = Input.GetAxis("Horizontal");
        var xVel = PlayerRigidbody.velocity.x;
        var xVelWasOver = Mathf.Abs(xVel) > MaxMovementSpeed;
        var newXVel = xVel;
        if (Mathf.Abs(xVel) < MaxMovementSpeed || Mathf.Sign(xVel) != hInput)
        {
            newXVel += Acceleration * hInput * Time.deltaTime;
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

        if (MovementState == TMovementState.GROUNDED && Input.GetAxis("Jump") == 1.0)
        {
            PlayerRigidbody.velocity = new(PlayerRigidbody.velocity.x, JumpStrength);
        }
    }

    void HandleSlowdown()
    {
        var origVelocity = PlayerRigidbody.velocity.x;
        var amount = MaxMovementSpeed * SlowdownRate * Mathf.Sign(origVelocity) * Time.deltaTime;
        var newVelocity = PlayerRigidbody.velocity.x - amount;
        if (Mathf.Sign(origVelocity) != Mathf.Sign(newVelocity))
        {
            // the sign flipped, so the velocity passed 0 and should be set there
            newVelocity = 0.0f;
        }
        PlayerRigidbody.velocity = new(
            newVelocity,
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
