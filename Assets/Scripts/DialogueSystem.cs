using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] GameObject Camille;
    [SerializeField] GameObject PlayerCamera;

    private Animator director;
    public CanvasGroup[] canvasGroup;
    public float fadeDuration = 1f;
    public float targetAlpha = 1f;

    public bool acceptInput = false;

    private void Awake()
    {
        if (Camille == null)
        {
            Camille = GameObject.FindGameObjectWithTag("Camille");
        }

        director = GetComponent<Animator>();

        director.Play("IntroCinematic", 0);
    }

    private void Update()
    {
        if (acceptInput)
        {
            if (Input.GetKeyDown(KeyCode.Q)) // OPTION A
            {
                Camille.GetComponent<Animator>().SetTrigger("OptionA");
                director.Play("OptionA", 0);
                acceptInput = false;
            }

            if (Input.GetKeyDown(KeyCode.W)) // OPTION B
            {
                director.Play("OptionB", 0);
            }

            if (Input.GetKeyDown(KeyCode.E)) // OPTION C
            {
                director.Play("OptionC", 0);
            }

            if (Input.GetKeyDown(KeyCode.R)) // OPTION D
            {
                director.Play("OptionD", 0);
            }
        }
    }

    public void StartFadeIn()
    {
        director.Play("ShowDialogueUI", 0);
        Debug.Log("Played!");
    }

    public void FinishedIntroCinematic()
    {
        PlayerCamera.SetActive(true);
    }
}
