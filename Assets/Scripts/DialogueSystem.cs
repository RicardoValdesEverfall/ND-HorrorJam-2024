using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] GameObject Camille;

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
    }

    private void Update()
    {
        if (acceptInput)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Camille.GetComponent<Animator>().SetTrigger("OptionA");
                director.Play("OptionA", 0);
                acceptInput = false;
            }
        }
    }

    public void StartFadeIn()
    {
        director.Play("ShowDialogueUI", 0);
        Debug.Log("Played!");
    }
}
