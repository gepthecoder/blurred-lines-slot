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

    private bool is_auto_play_;
    private int num_auto_spins_;
    public int GetNumOfAutoSpinsLeft() { return num_auto_spins_; }

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
        Debug.Log("TryStartSpinning");

        // prevents interference if the reel are still spinning

        if (!startSpin)
        {
            startSpin = true;
            spin_button_.interactable = false;
            spin_btn_anime_.SetTrigger(Consts.spin_button_trigger_);

            StartCoroutine(Spinning());
        }       
    }

    public void TryStartAutoSpins(int num_spins, out bool success)
    {
        if (!startSpin)
        {
            success = true;
            Debug.Log("TryStartAutoSpins");
            AutoSpins(num_spins);
        }
        else { 
            success = false;
        }
    }

    private void AutoSpins(int amount)
    {
        is_auto_play_ = true;

        num_auto_spins_ = amount;
        Debug.Log($"Num of auto spins set to {amount}");
        // conduct for first spin
        num_auto_spins_--;
        startSpin = true;
        spin_button_.interactable = false;
        spin_btn_anime_.SetTrigger(Consts.spin_button_trigger_);
        StartCoroutine(AutoSpinning(OnSpinFinnished));

    }

    private void OnSpinFinnished(string errorMessage)
    {
        if (errorMessage != null)
        {
            Debug.LogError("Was not able to execute spin: " + errorMessage);
        }


        if (is_auto_play_) { 

            if(num_auto_spins_ <= 0) {
                //stop loop 
                is_auto_play_ = false;
                num_auto_spins_ = 0;
                spin_button_.interactable = true;
                StopAllCoroutines();
            }
            else
            {
                if (!startSpin)
                {
                    num_auto_spins_--;
                    startSpin = true;
                    spin_button_.interactable = false;
                    spin_btn_anime_.SetTrigger(Consts.spin_button_trigger_);
                    StartCoroutine(AutoSpinning(OnSpinFinnished));
                }
            }
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

    private IEnumerator AutoSpinning(System.Action<string> onCompleted)
    {
        foreach (Reel spinner in reels)
        {
            //tells each reel to start spinning
            spinner.spin = true;
        }
        for (int i = 0; i < reels.Length; i++)
        {
            //allow the reel to spin for a random amount of time then stop
            yield return new WaitForSeconds(Random.Range(1, 3));
            reels[i].spin = false;
            reels[i].RandomPosition();
        }
        //allows the machine to be started again
        startSpin = false;

        yield return new WaitForSeconds(1f);
        onCompleted(null);
    }

}
