using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    
    public bool spin;
    int speed;

    void Start()
    {
        //foreach (Transform image in transform)
        //    Debug.Log("image name: " + image.localPosition.y);

        spin = false;
        speed = 5500;
    }

    void Update()
    {
        if (spin)
        {
            foreach (Transform image in transform)
            {
                image.Translate(Vector3.down * Time.smoothDeltaTime * speed);
                //reset position bellow a certain point to the top of the reel
                if (image.transform.localPosition.y <= -1200)
                {

                    image.transform.localPosition = new Vector3(image.transform.localPosition.x, image.transform.localPosition.y + 2700, image.transform.localPosition.z);
                }
            }
        }
    }

    //once the reel finishes spinning the images will be placed in a random position
    public void RandomPosition()
    {
        List<int> parts = new List<int>();

        // ADD ALL OF THE VALUES FOR THE ORIGINAL Y POSITION
        parts.Add(1200);
        parts.Add(900);
        parts.Add(600);
        parts.Add(300);
        parts.Add(0);
        parts.Add(-300);
        parts.Add(-600);
        parts.Add(-900);
        parts.Add(-1200);
        parts.Add(-1500);

        foreach (Transform image in transform)
        {
            int rand = Random.Range(0, parts.Count);

            image.transform.position = new Vector3(image.transform.position.x, parts[rand] + transform.parent.parent.GetComponent<RectTransform>().transform.position.y, image.transform.position.z);
            parts.RemoveAt(rand);
        }

    }

    public void InitRandomSymbolsOnReel(List<Symbol> symbols)
    {
        List<int> parts = new List<int>();

        // ADD ALL OF THE VALUES FOR THE ORIGINAL Y POSITION
        parts.Add(1200);
        parts.Add(900);
        parts.Add(600);
        parts.Add(300);
        parts.Add(0);
        parts.Add(-300);
        parts.Add(-600);
        parts.Add(-900);
        parts.Add(-1200);
        parts.Add(-1500);

        int index = 0;

        foreach (Transform image in transform)
        {
            Debug.Assert(index <= symbols.Count);

            image.GetComponent<Image>().sprite = symbols[index]._SYMBOL_;
            image.name = symbols[index]._NAME_;

            int rand = Random.Range(0, parts.Count);
            // track reel ids for parts (300, 0, -300)

            image.transform.position = new Vector3(image.transform.position.x, parts[rand] + transform.parent.parent.GetComponent<RectTransform>().transform.position.y, image.transform.position.z);
            parts.RemoveAt(rand);
            index++;
        }
    }
}
