using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    private GameObject PlayerSprite;
    private Animator PlayerAnimator;
    private PlayerMovement MovementController;
    private PlayerShoot ShootingController;
    private Rigidbody2D PlayerRigidbody;

    private float InitialXScale;
    private float TimeSinceLastGrounded;

    private float DeathRotation;

    // Start is called before the first frame update
    void Start()
    {
        PlayerSprite = transform.GetChild(0).gameObject;
        PlayerAnimator = PlayerSprite.GetComponent<Animator>();
        MovementController = GetComponent<PlayerMovement>();
        ShootingController = GetComponent<PlayerShoot>();
        PlayerRigidbody = GetComponent<Rigidbody2D>();
        InitialXScale = PlayerSprite.transform.localScale.x;
        TimeSinceLastGrounded = 0.0f;
        DeathRotation = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (MovementController.IsAlive)
        {
            SetCharacterDirection();
            UpdateAnimatorParameters();
        }
        else
        {
            ResetAnimatorParameters();
            PerformDeathAnimation();
        }
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

        var isOnWall = MovementController.ContactState == PlayerMovement.TContactState.LEFT_WALL ||
            MovementController.ContactState == PlayerMovement.TContactState.RIGHT_WALL;
        PlayerAnimator.SetBool("Is On Wall", isOnWall);

        PlayerAnimator.SetFloat("X-Speed", Mathf.Abs(PlayerRigidbody.velocity.x));

        PlayerAnimator.SetFloat("Aim Angle", ShootingController.AimAngleNormalized);

        var targetBowPull = ShootingController.BowState == PlayerShoot.TBowState.LOOSE ?
            0.0f : 1.0f;
        var currentBowPull = PlayerAnimator.GetFloat("Bow Pull");
        var lerpedBowPull = Mathf.Lerp(currentBowPull, targetBowPull, Time.deltaTime * 10f);
        PlayerAnimator.SetFloat("Bow Pull", lerpedBowPull);
        PlayerAnimator.SetLayerWeight(1, lerpedBowPull);
    }

    void ResetAnimatorParameters()
    {
        PlayerAnimator.SetBool("Is Airborne", false);
        PlayerAnimator.SetBool("Is On Wall", false);
        PlayerAnimator.SetFloat("X-Speed", 0);
        PlayerAnimator.SetFloat("Bow Pull", 0f);
        PlayerAnimator.SetLayerWeight(1, 0f);
    }

    void PerformDeathAnimation()
    {
        DeathRotation += Time.deltaTime * 1000f;
        PlayerSprite.transform.localRotation = Quaternion.Euler(new(
            0f, 0f, DeathRotation));
        PlayerSprite.transform.position += new Vector3(0f, -4f * Time.deltaTime);
    }
}
