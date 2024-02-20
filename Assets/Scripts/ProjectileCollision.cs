using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectileCollision : MonoBehaviour
{
  

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -7) {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.gameObject.CompareTag("John")) {
            Destroy(gameObject);
        }
        else {
            SceneManager.LoadScene(3);
        }
    }
}
