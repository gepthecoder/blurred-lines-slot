using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slots : MonoBehaviour
{
    public Reel[] reels;
    bool startSpin;

    void Start()
    {
        startSpin = false;
    }

    void Update()
    {
        if (!startSpin) // prevents interference if the reel are still spinning 
        {
            if (Input.GetKeyDown(KeyCode.Space)) //input that starts slot machine
            {
                startSpin = true;
                StartCoroutine(Spinning());
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
    }
}
