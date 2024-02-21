using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BigJohn : MonoBehaviour
{
    private bool PlayedEnd;
    private Collider2D BigJohnCollider;
    private AudioSource BigJohnAudioSource;
    private int JohnLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        PlayedEnd = false;
        BigJohnCollider = GetComponent<BoxCollider2D>();
        BigJohnAudioSource = GetComponent<AudioSource>();
        JohnLayerMask = LayerMask.GetMask("John");
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayedEnd && BigJohnCollider.IsTouchingLayers(JohnLayerMask))
        {
            PlayedEnd = true;
            BigJohnAudioSource.Play();
            StartCoroutine(MenuAfterDelay());
        }
    }

    private IEnumerator MenuAfterDelay()
    {
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene(0);
    }
}
