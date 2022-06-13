using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slots : MonoBehaviour
{
    [SerializeField] private Button spin_button_;

    public Reel[] reels;
    bool startSpin;

    void Start()
    {
        spin_button_.onClick.AddListener(StartSpinning);
        startSpin = false;
    }

    void Update()
    {
        if (!startSpin) // prevents interference if the reel are still spinning 
        {
            if (Input.GetKeyDown(KeyCode.Space)) //input that starts slot machine
            {
                startSpin = true;
                spin_button_.interactable = false;
                StartCoroutine(Spinning());
            }
        }
    }

    public void StartSpinning() // button callback
    {
        if (!startSpin)
        {
            startSpin = true;
            spin_button_.interactable = false;
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
