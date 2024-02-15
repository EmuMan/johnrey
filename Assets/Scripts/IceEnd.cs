using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IceEnd : MonoBehaviour
{   

    // Update is called once per frame
    void Update()
    {
            if (transform.position.x >= 17) {
                SceneManager.LoadScene(3);
            }
            if (transform.position.y < -7) {
                SceneManager.LoadScene(2);
            }
    }
}
