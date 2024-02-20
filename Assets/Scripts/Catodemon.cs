using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catodemon : MonoBehaviour
{
    public GameObject fireball;
    public Transform shotPoint;
    public float firePower;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("LaunchFireball", 2.0f, 2.0f);
    }

    // Update is called once per frame
    void LaunchFireball()
    {
        GameObject fireballInstance = Instantiate(fireball, shotPoint.position, shotPoint.rotation);
        Rigidbody2D rb = fireballInstance.GetComponent<Rigidbody2D>();
        rb.velocity = -transform.right * firePower;
    }
}
