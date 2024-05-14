using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Transform lookAtTarget;
    public Vector3 lookAtPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        lookAtTarget.position = lookAtPosition;
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }
}
