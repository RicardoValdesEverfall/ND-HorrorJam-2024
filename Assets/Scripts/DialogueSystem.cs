using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    private FMOD.Studio.EventInstance cuanInstance;
    private FMOD.Studio.EventInstance sallyInstance;

    public FMODUnity.EventReference dialogueEvent;
    public FMODUnity.EventReference sallyEvent;

    [SerializeField] Animator Camille;
    [SerializeField] GameObject PlayerCamera;
    [SerializeField] Image SkipIcon;
    [SerializeField] float timeToFirstInteraction;
    [SerializeField] CanvasGroup[] choicesArray;
    [SerializeField] TextMeshProUGUI[] textBoxes; //0 = dialogue, 1 = A, 2 = B, 3 = C, 4 = D

    public FMOD.Studio.PLAYBACK_STATE cuanState;
    public FMOD.Studio.PLAYBACK_STATE sallyState;

    private bool cuanCued = false;
    private bool sallyCued = false;

    private Animator director;

    public string currentDialogueID;
    public bool acceptInput = false;
    public bool skippable = false;
    public bool showingDialogueUI = false;
    public bool finishedTalking = false;
    public int triggerIndex = 0;
    public int dialogSeqIndex = 0;
    public int dialogPath = 1;
    public int sallyIndex = 2;
    public int sallySeqIndex = 0;

    private float timer = 0.5f;
    private float skipTimer = 0;
    private bool day_one_started = false;
    private bool day_one_ended = false;
    private bool day_two_ended = false;
    private bool hasProgressed = false;
    private bool completedSeq = false;
    private int day = 1;

    private const float SHORT_DELAY = 0.1f;

    private void Awake()
    {
        cuanInstance = FMODUnity.RuntimeManager.CreateInstance(dialogueEvent);
        sallyInstance = FMODUnity.RuntimeManager.CreateInstance(sallyEvent);

        cuanInstance.setParameterByName("Day 1 to 5", 1);
        cuanInstance.setParameterByName("DialogPath", 1);
        cuanInstance.setParameterByName("DialogSeq", 1);

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

        timeToFirstInteraction = 0.5f;	// Just long enough to let Cuan's first line begin
    }

    private bool IsCuanFinished()
    {
        if (cuanState != FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            cuanCued = false;	// clear this whenever sally has started speaking
            return false;
        }
        if (cuanCued)	// Not currently speaking but we already asked her to...
        {
            return false;
        }
        return true;
    }

    private bool IsSallyFinished()
    {
        if (sallyState != FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            sallyCued = false;	// clear this whenever sally has started speaking
            return false;
        }
        if (sallyCued)	// Not currently speaking but we already asked her to...
        {
            return false;
        }
        return true;
    }

    private void Update()
    {
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.F2)) // DEBUG F2 -> Day 2
		{
			day_one_ended = true;
			DayTwo_Init();
			if (skippable)
				director.Play("IntroFinish", 0);
		}
		else if (Input.GetKeyDown(KeyCode.F3)) // DEBUG F3 -> Day 3
		{
			day_two_ended = true;
		}
#endif

        if (acceptInput)
        {
            if (day==1 && day_one_ended)
            {
                // Listen for Z only otherwise the dialogue UI can reappear
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    director.Play("Sleep", 0);
					DayTwo_Init();
                }
            }
            else
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
        }

        cuanInstance.getPlaybackState(out cuanState);
        sallyInstance.getPlaybackState(out sallyState);

        if (day == 1)
        {
            DayOne_Triggers();

            if (IsSallyFinished() && day_one_ended)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    Camille.Play("end", 0);
                    director.Play("ShowSleepOption", 0);
                    day_one_ended = true;
                    timer = 0.5f;
                }
            }
        }
        else if (day == 2)
        {
            DayTwo_Triggers();

            if (IsSallyFinished() && day_two_ended)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    //Camille.Play("end", 0);
                    timer = 0.5f;
                    day = 3;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (day == 1)
        {
            DayOne_Main(currentDialogueID);
        }
        else if (day == 2)
        {
            DayTwo_Main(currentDialogueID);
        }

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
            if (timeToFirstInteraction <= 0 && IsCuanFinished())	// Wait until Cuan opening line has finished so Sally interrupts
            {
                day_one_started = true;
                director.Play("Day1", 0);
            }
        }
    }

    public void StartFadeIn()
    {
		if (!showingDialogueUI)
		{
			Debug.Log("StartFadeIn");
			director.Play("ShowDialogueUI", 0);
			showingDialogueUI = true;
		}
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
		Debug.Log("Trigger Sally day " + day + "  line " + sallyIndex + "  seq " + dialogSeqIndex );

        sallyInstance.setParameterByName("Dialog ID", sallyIndex);
        sallyInstance.start();
        sallyIndex++;
        triggerIndex++;
        sallyCued = true;
    }

    public void fmodTriggerDialogue(string eventName)
    {
        switch (eventName)
        {
            case "empty":
                dialogPath = 0;
                cuanInstance.setParameterByName("DialogPath", dialogPath);
                break;
            case "A":
                dialogPath = 1;
                cuanInstance.setParameterByName("DialogPath", dialogPath);
              
                break;
            case "B":
                dialogPath = 2;
                cuanInstance.setParameterByName("DialogPath", dialogPath);
               
                break;
            case "C":
                dialogPath = 3;
                cuanInstance.setParameterByName("DialogPath", dialogPath);
               
                break;
            case "D":
                dialogPath = 4;
                cuanInstance.setParameterByName("DialogPath", dialogPath);
                break;
        }

        if (triggerIndex == 4 && completedSeq == false)
        {
            dialogSeqIndex++;
            cuanInstance.setParameterByName("DialogSeq", dialogSeqIndex);
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

        cuanInstance.setParameterByName("Dialog ID", triggerIndex);
        cuanInstance.start();
        cuanCued = true;
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
                textBoxes[3].text = "Why aren�t we in the sub?";
                break;
            case "01-04":
                choicesArray[2].alpha = 0;
                choicesArray[3].alpha = 0;

                textBoxes[0].text = "uh...You drove the sub into the station, dude. Missed the nacelle. Hit the wall. " +
                                    "We barely made it out. No-one's going anywhere until the surface can source a new sub.";

                textBoxes[1].text = "I told you! I said I shouldn't have been driving!";
                textBoxes[2].text = "Heh. No doing anything about it now! And we're alive.";
                break;
        }
    }

    public void DayOne_Triggers()
    {
        if (day_one_ended) return;

        if (sallyIndex == 3 && IsSallyFinished() && finishedTalking && showingDialogueUI == false)
        {
            StartFadeIn();
        }


        if (hasProgressed)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (sallyIndex == 3 && IsSallyFinished() && IsCuanFinished())
                {
                    sallyIndex++;
                    fmodTriggerSallyDialogue();
                    hasProgressed = false;
                    timer = SHORT_DELAY;
                    //timer = 1.5f;
                }
            }
        }

        if (!hasProgressed && sallyIndex == 5)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (sallyIndex == 5 && IsSallyFinished() && IsCuanFinished())
                {
                    sallyIndex++;
                    fmodTriggerDialogue("empty");
                    hasProgressed = true;
                    timer = SHORT_DELAY;
                    //timer = 1f;
                }
            }
        }

        if (triggerIndex == 4 && sallyIndex == 6 && dialogSeqIndex == 2 && IsCuanFinished() && IsSallyFinished())
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                dialogSeqIndex++;
                sallyInstance.setParameterByName("Dialog ID", 4);
                sallyInstance.setParameterByName("DialogSeq", dialogSeqIndex);
                sallyInstance.start();
                timer = SHORT_DELAY;
            }
        }

        if (triggerIndex == 4 && sallyIndex == 6 && dialogSeqIndex == 3 && IsSallyFinished())
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                fmodTriggerDialogue("empty");
                timer = SHORT_DELAY;
            }
        }

        if (triggerIndex == 4 && sallyIndex == 6 && dialogSeqIndex == 4 && IsCuanFinished())
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                dialogSeqIndex++;
                sallyInstance.setParameterByName("Dialog ID", 4);
                sallyInstance.setParameterByName("DialogSeq", dialogSeqIndex);
                sallyInstance.start();
                timer = SHORT_DELAY;
            }
        }

        if (triggerIndex == 4 && sallyIndex == 6 && dialogSeqIndex == 5 && IsSallyFinished())
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                StartFadeIn();
                dialogSeqIndex = 1;
                completedSeq = true;
                timer = SHORT_DELAY;
            }
        }

        if (completedSeq == true && triggerIndex == 5 && dialogSeqIndex == 1 && IsCuanFinished())
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                sallyInstance.setParameterByName("Dialog ID", 5);
                sallyInstance.setParameterByName("DialogSeq", dialogSeqIndex);
                sallyInstance.setParameterByName("DialogPath", dialogPath);
                sallyInstance.start();
                timer = SHORT_DELAY;
                completedSeq = false;
            }
        }

        if (completedSeq == false && triggerIndex == 5 && dialogSeqIndex == 1 && IsCuanFinished() && IsSallyFinished())
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                cuanInstance.setParameterByName("DialogSeq", 1);
                fmodTriggerDialogue("empty");
                timer = SHORT_DELAY;
                dialogSeqIndex++;
            }
        }

        if (completedSeq == false && triggerIndex == 6 && dialogSeqIndex == 2 && IsCuanFinished())
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                sallyInstance.setParameterByName("Dialog ID", sallyIndex);
                sallyInstance.setParameterByName("DialogSeq", dialogSeqIndex);
                sallyInstance.setParameterByName("DialogPath", 1);
                sallyInstance.start();
                timer = SHORT_DELAY;
                dialogSeqIndex++;
            }
        }

        if (completedSeq == false && triggerIndex == 6 && dialogSeqIndex == 3 && IsCuanFinished() && IsSallyFinished())
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                cuanInstance.setParameterByName("Dialog ID", 6);
                cuanInstance.setParameterByName("DialogSeq", dialogSeqIndex);
                cuanInstance.start();
                dialogSeqIndex++;
                timer = SHORT_DELAY;
                completedSeq = true;
            }
        }

        if (completedSeq == true && triggerIndex == 6 && dialogSeqIndex == 4 && IsCuanFinished() && IsSallyFinished())
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                sallyInstance.setParameterByName("Dialog ID", 6);
                sallyInstance.setParameterByName("DialogSeq", dialogSeqIndex);
                sallyInstance.setParameterByName("DialogPath", 1);
                sallyInstance.start();
                timer = 3.5f;
                day_one_ended = true;
            }
        }
    }

    public void DayTwo_Main(string dialogueChoiceID)
    {
        switch (dialogueChoiceID)
        {
            case "02-03":
                choicesArray[3].alpha = 0; // OPTION D

                textBoxes[0].text = "Hey, you're awake! How are you feeling today?";

                textBoxes[1].text = "Fine.";
                textBoxes[2].text = "Good.";
                textBoxes[3].text = "I've crashed a submarine.";
                break;
            case "02-06":
                choicesArray[3].alpha = 0;

                textBoxes[0].text = "Dariyanov has agreed to loan his personal minisub. For evacuation. So we�ll get out of here soon.";

                textBoxes[1].text = "What have I got to go back for?";
                textBoxes[2].text = "Glad to be done with this place.";
                textBoxes[3].text = "Ooh, fancy. Minisub chauffeur.";
                break;
            case "02-08":
                choicesArray[3].alpha = 0;

                textBoxes[0].text = "Gotta ask some questions. You up to it?";

                textBoxes[1].text = "Bloody hate reports. No.";
                textBoxes[2].text = "Bloody hate reports...go on then.";
                textBoxes[3].text = "Bloody hate reports. What choice do I have?";
                break;
            case "02-10":
                textBoxes[0].text = "First question: what can you recall of the dive dated 01/04/51?";

                textBoxes[1].text = "Nothing. Just... nothing.";
                textBoxes[2].text = "I drove my sub into the station wall.";
                textBoxes[3].text = "I swear... I swear I saw something...";
                textBoxes[4].text = "I got the bends, lost control of the sub.";
                break;
            case "02-12":
                choicesArray[3].alpha = 0;	// Hide option 4 until first three are followed

                textBoxes[0].text = "Now. What do you remember before the crash?";

                textBoxes[1].text = "The mission was to the trench.";
                textBoxes[2].text = "Not a biologist. But...";
                textBoxes[3].text = "We got down to the first vent, sampled the water...";
                textBoxes[4].text = "- wait. Wasn�t there? Uhhh...";
                break;

        }
    }


	private void DayTwo_Init()
	{
		Debug.Log("DAY 2 INIT!");
		
		day = 2;
		sallyIndex = 2;
		triggerIndex = 1;

		cuanInstance.setParameterByName("Day 1 to 5", day);
		sallyInstance.setParameterByName("Day 1 to 5", day);
	}

    public void DayTwo_Triggers()
    {
        if (day_two_ended) return;
		if (skippable) return;

		// OK trying a different layout for Day 2 in the hope it's simpler ;)

		if (IsSallyFinished() && IsCuanFinished())	// Do nothing while either character is speaking
		{
			switch (sallyIndex)
			{
			case 3:		// Cuan branching options!
				currentDialogueID = "02-03";
				StartFadeIn();
				break;

			case 6:
			case 8:
			case 10:

				break;

			default:	// Default case: Sally reads her next line and progresses
				fmodTriggerSallyDialogue();
				hasProgressed = false;
				timer = SHORT_DELAY;
				break;
			}


		}

    }


}
