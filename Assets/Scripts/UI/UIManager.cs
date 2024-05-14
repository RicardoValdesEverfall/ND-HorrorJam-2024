using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Transform lookAtTarget;
    public Vector3 lookAtPosition;
    public float lerpSpeed = 1f;

    private Animator director;

    // Start is called before the first frame update
    void Awake()
    {
        director = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play()
    {
        StartCoroutine(LerpToPosition());

        director.Play("FadeOut", 0);
    }

    private IEnumerator LerpToPosition()
    {
        Vector3 startPosition = lookAtTarget.position;
        Vector3 target = lookAtPosition;

        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            lookAtTarget.position = Vector3.Lerp(startPosition, target, elapsedTime);
            elapsedTime += Time.deltaTime * lerpSpeed;
            yield return null; // Wait for the next frame
        }

        // Ensure reaching the exact target position
        lookAtTarget.position = target;
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }
}
