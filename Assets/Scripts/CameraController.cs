using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject Target;

    public float MoveSpeed;
    public float Deadzone;
    public float MaxDistance;

    public Camera CameraComponent { get; private set; }

    private float GetDisplacement(float targetNorm, float distNorm, float distActual)
    {
        var ratio = (Mathf.Abs(distNorm) - targetNorm) / Mathf.Abs(distNorm);
        return ratio * distActual;
    }

    // Start is called before the first frame update
    void Start()
    {
        CameraComponent = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
        {
            var startPos = transform.position;
            var targetPos = new Vector3(
                Target.transform.position.x,
                Target.transform.position.y,
                transform.position.z);
            // (0, 0) is center, ranges from (-1, -1) to (1, 1) on screen
            var targetScreenPos =
                (CameraComponent.WorldToViewportPoint(targetPos) -
                new Vector3(0.5f, 0.5f, 0.0f)) * 2.0f;

            var newX = startPos.x;
            var newY = startPos.y;

            if (Mathf.Abs(targetScreenPos.x) > MaxDistance)
            {
                newX = startPos.x + GetDisplacement(MaxDistance, targetScreenPos.x, targetPos.x - startPos.x);
            }
            if (Mathf.Abs(targetScreenPos.y) > MaxDistance)
            {
                newY = startPos.y + GetDisplacement(MaxDistance, targetScreenPos.y, targetPos.y - startPos.y);
            }

            if (newX == startPos.x && Mathf.Abs(targetScreenPos.x) > Deadzone)
            {
                var disp = GetDisplacement(Deadzone, targetScreenPos.x, targetPos.x - startPos.x);
                newX = Mathf.Lerp(startPos.x, startPos.x + disp, Time.deltaTime * MoveSpeed);
            }
            if (newY == startPos.y && Mathf.Abs(targetScreenPos.y) > Deadzone)
            {
                var disp = GetDisplacement(Deadzone, targetScreenPos.y, targetPos.y - startPos.y);
                newY = Mathf.Lerp(startPos.y, startPos.y + disp, Time.deltaTime * MoveSpeed);
            }

            transform.position = new(newX, newY, startPos.z);
        }
    }
}
