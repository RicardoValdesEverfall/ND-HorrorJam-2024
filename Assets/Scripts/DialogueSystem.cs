using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Profiling.RawFrameDataView;

public class DialogueSystem : MonoBehaviour
{
    private FMOD.Studio.EventInstance instance;
    private FMOD.Studio.EventInstance sallyInstance;

    public FMODUnity.EventReference dialogueEvent;
    public FMODUnity.EventReference sallyEvent;

    [SerializeField] Animator Camille;
    [SerializeField] GameObject PlayerCamera;
    [SerializeField] Image SkipIcon;
    [SerializeField] float timeToFirstInteraction;
    [SerializeField] CanvasGroup[] choicesArray;
    [SerializeField] TextMeshProUGUI[] textBoxes; //0 = dialogue, 1 = A, 2 = B, 3 = C, 4 = D

    public FMOD.Studio.PLAYBACK_STATE state;
    public FMOD.Studio.PLAYBACK_STATE sallyState;

    private Animator director;

    public string currentDialogueID;
    public bool acceptInput = false;
    public bool skippable = false;
    public bool showingDialogueUI = false;
    public bool finishedTalking = false;
    public int triggerIndex = 0;
    public int sallyIndex = 2;
    public int sallySeqIndex = 0;
    public int dialogSeqIndex = 0;

    private float timer = 0.5f;
    private float skipTimer = 0;
    private bool day_one_started = false;
    private bool hasProgressed = false;

    private void Awake()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(dialogueEvent);
        sallyInstance = FMODUnity.RuntimeManager.CreateInstance(sallyEvent);

        instance.setParameterByName("Day 1 to 5", 1);
        instance.setParameterByName("DialogPath", 1);
        instance.setParameterByName("DialogSeq", 1);

        sallyInstance.setParameterByName("Day 1 to 5", 1);
        sallyInstance.setParameterByName("DialogPath", 1);
        sallyInstance.setParameterByName("DialogSeq", 1);

        if (Camille == null)
        {
            Camille = GameObject.FindGameObjectWithTag("Camille").GetComponent<Animator>();
        }

        director = GetComponent<Animator>();
        director.Play("IntroCinematic", 0);
        skippable = true;

        timeToFirstInteraction = Random.Range(8f, 15f);
    }

    private void Update()
    {
        if (acceptInput)
        {
            if (Input.GetKeyDown(KeyCode.Q)) // OPTION A
            {
                director.Play("OptionA", 0);
                acceptInput = false;
            }

            if (Input.GetKeyDown(KeyCode.W)) // OPTION B
            {
                director.Play("OptionB", 0);
                acceptInput = false;
            }

            if (Input.GetKeyDown(KeyCode.E)) // OPTION C
            {
                director.Play("OptionC", 0);
                acceptInput = false;
            }

            if (Input.GetKeyDown(KeyCode.R)) // OPTION D
            {
                director.Play("OptionD", 0);
                acceptInput = false;
            }
        }

        instance.getPlaybackState(out state);
        sallyInstance.getPlaybackState(out sallyState);

        if (sallyIndex == 3 && sallyState == FMOD.Studio.PLAYBACK_STATE.STOPPED && finishedTalking && showingDialogueUI == false)
        {
            StartFadeIn();
        }

        if (hasProgressed)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (sallyIndex == 3 && sallyState == FMOD.Studio.PLAYBACK_STATE.STOPPED && state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    sallyIndex++;
                    fmodTriggerSallyDialogue();
                    hasProgressed = false;
                    timer = 1.5f;
                }
            }
        }

        if (!hasProgressed && sallyIndex == 5)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (sallyIndex == 5 && sallyState == FMOD.Studio.PLAYBACK_STATE.STOPPED && state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    sallyIndex++;
                    fmodTriggerDialogue("empty");
                    hasProgressed = true;
                    timer = 1f;
                }
            }
        }

        if (triggerIndex == 4 && sallyIndex == 6 && dialogSeqIndex == 2 && state == FMOD.Studio.PLAYBACK_STATE.STOPPED && sallyState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                dialogSeqIndex++;
                sallyInstance.setParameterByName("Dialog ID", 4);
                sallyInstance.setParameterByName("DialogSeq", dialogSeqIndex);
                sallyInstance.start();
                timer = 0.5f;
            }
        }

        if (triggerIndex == 4 && sallyIndex == 6 && dialogSeqIndex == 3 && sallyState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                fmodTriggerDialogue("empty");
                timer = 0.5f;
            }
        }

        if (triggerIndex == 4 && sallyIndex == 6 && dialogSeqIndex == 4 && state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                dialogSeqIndex++;
                sallyInstance.setParameterByName("Dialog ID", 4);
                sallyInstance.setParameterByName("DialogSeq", dialogSeqIndex);
                sallyInstance.start();
                timer = 0.5f;
            }
        }

    }

    private void LateUpdate()
    {
        DayOne_Main(currentDialogueID);

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

        if (!day_one_started && !skippable)
        {
            timeToFirstInteraction -= Time.deltaTime;
            Debug.Log(timeToFirstInteraction);

            if (timeToFirstInteraction <= 0)
            {
                day_one_started = true;
                director.Play("Day1", 0);
            }
        }
    }

    public void StartFadeIn()
    {
        director.Play("ShowDialogueUI", 0);
        showingDialogueUI = true;
    }

    public void EnableInput()
    {
        acceptInput = true;
    }

    public void EnableProgression()
    {
        hasProgressed = true;
    }

    public void FinishedTalking()
    {
        finishedTalking = true;
    }

    public void FinishedIntroCinematic()
    {
        PlayerCamera.SetActive(true);
        skippable = false;
    }
    //###################################################### FMOD SCRIPTING ###################################################

    public void fmodTriggerSallyDialogue()
    {
        sallyInstance.setParameterByName("Dialog ID", sallyIndex);
        sallyInstance.start();
        sallyIndex++;
        triggerIndex++;
    }

    public void fmodTriggerDialogue(string eventName)
    {
        switch (eventName)
        {
            case "empty":
                instance.setParameterByName("DialogPath", 0);
                break;
            case "A":
                instance.setParameterByName("DialogPath", 1);
              
                break;
            case "B":
                instance.setParameterByName("DialogPath", 2);
               
                break;
            case "C":
                instance.setParameterByName("DialogPath", 3);
               
                break;
            case "D":
                instance.setParameterByName("DialogPath", 4);
                break;
        }

        if (triggerIndex == 4)
        {
            dialogSeqIndex++;
            instance.setParameterByName("DialogSeq", dialogSeqIndex);
            currentDialogueID = "01-04";

            if (dialogSeqIndex == 5)
            {
                triggerIndex++;
            }
        }

        else
        {
            triggerIndex++;
        }

        instance.setParameterByName("Dialog ID", triggerIndex);
        instance.start();
    }


    //#################################################### DAY SCENES SCRIPTING ##############################################
    public void DayOne_Main(string dialogueChoiceID)
    {
        switch (dialogueChoiceID)
        {
            case "main":
                choicesArray[3].alpha = 0; // OPTION D

                textBoxes[0].text = "He's Awake! Guys, he's awake! ... Cuan? can you hear me? Are you okay in there?";
                textBoxes[1].text = "Very groggy. What's going on?";
                textBoxes[2].text = "I saw- I saw- What did I see?";
                textBoxes[3].text = "Why aren’t we in the sub?";
                break;
            case "01-04":
                choicesArray[2].alpha = 0;
                choicesArray[3].alpha = 0;

                textBoxes[0].text = "You- you don't remember? We were coming up from the white smoker and- uh..." +
                                    "You drove the sub into the station, dude. Missed the nacelle. Hit the wall. " +
                                    "She's a write-off, mate. We barely made it out. No-one's going anywhere until they can get";
                textBoxes[1].text = "I told you! I said I shouldn't have been driving!";
                textBoxes[2].text = "Heh. No doing anything about it now! And we're alive.";
                break;
        }
    }
}
