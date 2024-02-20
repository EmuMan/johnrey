using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject arrow;
    public Transform shotPoint;
    public float firePower;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            GameObject arrowInstance = Instantiate(arrow, shotPoint.position, shotPoint.rotation);
            Rigidbody2D rb = arrowInstance.GetComponent<Rigidbody2D>();
            rb.velocity = transform.right * firePower;
        }
    }
}
