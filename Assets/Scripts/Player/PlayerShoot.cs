using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public enum TBowState
    {
        LOOSE,
        PULLED,
    }

    public GameObject arrow;
    public Transform shotPoint;
    public float firePower;
    public Camera MainCamera;

    public TBowState BowState { get; private set; }
    public float AimAngleNormalized { get; private set; }

    private PlayerMovement MovementController;
    private TBowState LastBowState;

    private void Start()
    {
        MovementController = GetComponent<PlayerMovement>();
        LastBowState = TBowState.LOOSE;
    }

    // Update is called once per frame
    void Update()
    {
        BowState = Input.GetAxis("Fire1") == 0.0f ?
            TBowState.LOOSE : TBowState.PULLED;

        if (BowState == TBowState.LOOSE && LastBowState == TBowState.PULLED)
        {
            FireArrow();
        }

        LastBowState = BowState;

        UpdateAimAngle();
    }

    public void FireArrow()
    {
        var facing = MovementController.FacingDirection == PlayerMovement.TFacingDirection.LEFT ?
            1.0f : -1.0f;
        var rotation = Quaternion.Euler(0f, 0f, AimAngleNormalized * facing * 180f);
        GameObject arrowInstance = Instantiate(arrow, shotPoint.position, rotation);
        Rigidbody2D rb = arrowInstance.GetComponent<Rigidbody2D>();
        rb.velocity = rotation * Vector3.up * firePower;
    }

    public void UpdateAimAngle()
    {
        var mousePos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new(mousePos.x, mousePos.y);
        var origin = shotPoint.transform.position;
        origin = new(origin.x, origin.y);
        var dispVector = mousePos - origin;
        var angle = Vector3.Angle(Vector3.up, dispVector);
        if (BowState == TBowState.PULLED)
        {
            var newFacing = mousePos.x < origin.x ?
                PlayerMovement.TFacingDirection.LEFT : PlayerMovement.TFacingDirection.RIGHT;
            Debug.Log($"newFacing: {newFacing}, angle: {angle}");
            MovementController.SetFacingDirection(newFacing, overrideAiming: true);
        }
        AimAngleNormalized = angle / 180f;
    }

}
