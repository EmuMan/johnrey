using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject fireball;
    public Transform shotPoint;
    public float firePower;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 spawnPoint = transform.position + transform.right * 2 + transform.up * 2;
            GameObject fireballInstance = Instantiate(fireball, spawnPoint, Quaternion.identity);
            Rigidbody2D rb = fireballInstance.GetComponent<Rigidbody2D>();
            rb.velocity = transform.right * firePower;
        }
    }
}
