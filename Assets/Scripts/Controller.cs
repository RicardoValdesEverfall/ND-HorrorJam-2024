using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] DialogueSystem DS;

    void Start()
    {
        DS = GameObject.FindGameObjectWithTag("DialogueSystem").GetComponent<DialogueSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FinishedAnimation()
    {
        DS.StartFadeIn();
        DS.acceptInput = true;
    }
}
