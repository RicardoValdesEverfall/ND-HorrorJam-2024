using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Profiling.RawFrameDataView;

public class DialogueSystem : MonoBehaviour
{
    private FMOD.Studio.EventInstance instance;
    public FMODUnity.EventReference dialogueEvent;

    [SerializeField] Animator Camille;
    [SerializeField] GameObject PlayerCamera;
    [SerializeField] Image SkipIcon;
    [SerializeField] float timeToFirstInteraction;
    [SerializeField] CanvasGroup[] choicesArray;
    [SerializeField] TextMeshProUGUI[] textBoxes; //0 = dialogue, 1 = A, 2 = B, 3 = C, 4 = D

    private Animator director;

    public string currentDialogueID;
    public bool acceptInput = false;
    public bool skippable = false;
    public int triggerIndex = 0;

    private float skipTimer = 0;
    private bool day_one_started = false;

    private void Awake()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(dialogueEvent);
        instance.setParameterByName("Day 1 to 5", 1);

        if (Camille == null)
        {
            Camille = GameObject.FindGameObjectWithTag("Camille").GetComponent<Animator>();
        }

        director = GetComponent<Animator>();
        director.Play("IntroCinematic", 0);
        skippable = true;

        timeToFirstInteraction = Random.Range(20f, 120f);
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

    private void LateUpdate()
    {
        if (skippable)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                skipTimer += Time.deltaTime;
                SkipIcon.fillAmount = skipTimer;

                if (SkipIcon.fillAmount >= 0.99f)
                {
                    director.Play("IntroFinish", 0);
                }
            }

            else
            {
                skipTimer = -Time.deltaTime * 1.4f;
                SkipIcon.fillAmount = skipTimer;
            }
        }

        if (!day_one_started)
        {
            timeToFirstInteraction -= Time.deltaTime;

            if (timeToFirstInteraction <= 0)
            {
                day_one_started = true;
                director.Play("Day1", 0);
            }
        }

        else if (day_one_started)
        {
            DayOne_Main("main");
        }
    }

    public void StartFadeIn()
    {
        director.Play("ShowDialogueUI", 0);
    }

    public void FinishedIntroCinematic()
    {
        PlayerCamera.SetActive(true);
        skippable = false;
    }
    //###################################################### FMOD SCRIPTING ###################################################
    public void fmodDialogueChoice(int choice)
    {
        instance.setParameterByName("DialogID", choice);
    }


    public void fmodTriggerDialogue(string eventName)
    {
        instance.setParameterByName("Dialog ID", triggerIndex);
        instance.start();

        triggerIndex++;
    }


    //#################################################### DAY SCENES SCRIPTING ###############################################

    public void DayOne_Main(string dialogueChoiceID)
    {
        switch (dialogueChoiceID)
        {
            case "main":
                choicesArray[2].alpha = 0; // OPTION C
                choicesArray[3].alpha = 0; // OPTION D

                textBoxes[0].text = "";
                break;
        }
    }
}
