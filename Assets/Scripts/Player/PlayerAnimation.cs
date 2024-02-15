using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    private GameObject PlayerSprite;
    private Animator PlayerAnimator;
    private PlayerMovement MovementController;
    private Rigidbody2D PlayerRigidbody;

    private float InitialXScale;
    private float TimeSinceLastGrounded;

    // Start is called before the first frame update
    void Start()
    {
        PlayerSprite = transform.GetChild(0).gameObject;
        PlayerAnimator = PlayerSprite.GetComponent<Animator>();
        MovementController = GetComponent<PlayerMovement>();
        PlayerRigidbody = GetComponent<Rigidbody2D>();
        InitialXScale = PlayerSprite.transform.localScale.x;
        TimeSinceLastGrounded = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        SetCharacterDirection();
        UpdateAnimatorParameters();
    }

    void SetCharacterDirection()
    {
        var xScale = MovementController.FacingDirection == PlayerMovement.TFacingDirection.LEFT ?
            -InitialXScale : InitialXScale;
        PlayerSprite.transform.localScale = new(
            xScale,
            PlayerSprite.transform.localScale.y,
            PlayerSprite.transform.localScale.z
        );
    }

    void UpdateAnimatorParameters()
    {
        TimeSinceLastGrounded += Time.deltaTime;
        bool isAirborne;
        if (MovementController.MovementState == PlayerMovement.TMovementState.JUMPING)
        {
            isAirborne = true;
        }
        else if (MovementController.MovementState != PlayerMovement.TMovementState.FALLING)
        {
            isAirborne = false;
            TimeSinceLastGrounded = 0.0f;
        }
        else
        {
            isAirborne = TimeSinceLastGrounded > 0.1f;
        }
        PlayerAnimator.SetBool("Is Airborne", isAirborne);
        PlayerAnimator.SetFloat("X-Speed", Mathf.Abs(PlayerRigidbody.velocity.x));
    }
}
