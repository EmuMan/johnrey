using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public int SceneIndex;

    private Collider2D TransitionCollider;
    private int JohnLayerMask;

    private void Start()
    {
        TransitionCollider = GetComponent<BoxCollider2D>();
        JohnLayerMask = LayerMask.GetMask("John");
    }

    // Update is called once per frame
    void Update()
    {
        if (TransitionCollider.IsTouchingLayers(JohnLayerMask))
        {
            SceneManager.LoadScene(SceneIndex);
        }
    }
}
