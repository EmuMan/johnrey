using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Arrow : MonoBehaviour
{

    Rigidbody2D rb;
    public AudioClip ArrowHitSound;

    private AudioSource ArrowAudio;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ArrowAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if (transform.position.y < -7) {
            Destroy(gameObject);
        }
    }


    void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.gameObject.CompareTag("Catodemon")) {
            Destroy(gameObject);
        }
        else {
            ArrowAudio.PlayOneShot(ArrowHitSound);
            Destroy(collision.gameObject);
            StartCoroutine(WaitThenDestroy());
        }
    }

    private IEnumerator WaitThenDestroy()
    {
        yield return new WaitForSeconds(2.0f);
    }
}
