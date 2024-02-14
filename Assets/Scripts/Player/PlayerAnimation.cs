using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    private Animator PlayerAnimator;
    private PlayerMovement MovementController;

    private float InitialXScale;
    private PlayerMovement.TMovementState PreviousMovementState;

    // Start is called before the first frame update
    void Start()
    {
        PlayerAnimator = GetComponent<Animator>();
        MovementController = GetComponent<PlayerMovement>();
        InitialXScale = transform.localScale.x;
        PreviousMovementState = PlayerMovement.TMovementState.IDLE;
    }

    // Update is called once per frame
    void Update()
    {
        var xScale = transform.localScale.x;
        switch (MovementController.InputDirection)
        {
            case PlayerMovement.TInputDirection.LEFT:
                xScale = -InitialXScale;
                break;
            case PlayerMovement.TInputDirection.RIGHT:
                xScale = InitialXScale;
                break;
        }
        transform.localScale = new(
            xScale,
            transform.localScale.y,
            transform.localScale.z
        );

        if (!PreviousMovementStateIsSame())
        {
            switch (MovementController.MovementState)
            {
                case PlayerMovement.TMovementState.IDLE:
                    PlayerAnimator.SetTrigger("Idle");
                    Debug.Log("Setting Idle...");
                    break;
                case PlayerMovement.TMovementState.WALKING:
                    PlayerAnimator.SetTrigger("Walk");
                    Debug.Log("Setting Walk...");
                    break;
                case PlayerMovement.TMovementState.JUMPING:
                case PlayerMovement.TMovementState.FALLING:
                    if (!PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("John Jumping"))
                    {
                        PlayerAnimator.SetTrigger("Jump");
                        Debug.Log("Setting Jump...");
                    }
                    break;
            }
        }

        PreviousMovementState = MovementController.MovementState;
    }

    bool PreviousMovementStateIsSame()
    {
        switch (PreviousMovementState)
        {
            case PlayerMovement.TMovementState.JUMPING:
            case PlayerMovement.TMovementState.FALLING:
                return MovementController.MovementState == PlayerMovement.TMovementState.JUMPING ||
                    MovementController.MovementState == PlayerMovement.TMovementState.FALLING;
            case PlayerMovement.TMovementState.IDLE:
            case PlayerMovement.TMovementState.WALKING:
                return MovementController.MovementState == PreviousMovementState;
            default:
                break;
        }
        return false;
    }
}
