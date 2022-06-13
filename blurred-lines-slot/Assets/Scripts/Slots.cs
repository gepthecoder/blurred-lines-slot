using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlotHelper;

public class Slots : MonoBehaviour
{
    [SerializeField] private Button spin_button_;
    private Animator spin_btn_anime_;

    public Reel[] reels;
    bool startSpin;

    void Start()
    {
        spin_button_.onClick.AddListener(TryStartSpinning);
        spin_btn_anime_ = spin_button_.GetComponentInChildren<Animator>();

        startSpin = false;
    }

    void Update()
    {
        if (!startSpin)  
        {
            if (Input.GetKeyDown(KeyCode.Space)) //input that starts slot machine
            {
                TryStartSpinning();
            }
        }
    }

    public void TryStartSpinning() // spin callback
    {
        // prevents interference if the reel are still spinning

        if (!startSpin)
        {
            startSpin = true;
            spin_button_.interactable = false;
            spin_btn_anime_.SetTrigger(Consts.spin_button_trigger_);
            StartCoroutine(Spinning());
        }       
    }

    IEnumerator Spinning()
    {
        foreach (Reel spinner in reels)
        {
            //tells each reel to start spinning
            spinner.spin = true;
        }
        for (int i = 0; i < reels.Length; i++)
        {
            //allow the reel to spin for a random amount of time then stop
            yield return new WaitForSeconds(Random.Range(1,3));
            reels[i].spin = false;
            reels[i].RandomPosition();
        }
        //allows the machine to be started again
        startSpin = false;
        spin_button_.interactable = true;
    }
}
