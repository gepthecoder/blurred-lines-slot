using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlotHelper;

public class Reel : MonoBehaviour
{
    public int id;

    [SerializeField] private Vector3 reel_symbol_ids_;

    public List<Transform> reel_symbol_transforms_;

    public bool spin;
    int speed;

    void Start()
    {
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
    public List<int> SetReelOutcome()
    {
        List<int> parts = new List<int>();
        Dictionary<int, Transform> dict = new Dictionary<int, Transform>();

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

            dict.Add(parts[rand], image);

            image.transform.position = new Vector3(image.transform.position.x, parts[rand] + transform.parent.parent.GetComponent<RectTransform>().transform.position.y, image.transform.position.z);
            parts.RemoveAt(rand);
        }

        reel_symbol_transforms_ = new List<Transform>();
        // store visible symbols
        List<int> reel_symbol_ids_ = new List<int>();
        Transform _symbol_;
        dict.TryGetValue(300, out _symbol_);
        reel_symbol_ids_.Add((Enums.SymbolToId(Enums.StringToSymbol(_symbol_.name))));
        reel_symbol_transforms_.Add(_symbol_);

        dict.TryGetValue(0, out _symbol_);
        reel_symbol_ids_.Add(Enums.SymbolToId(Enums.StringToSymbol(_symbol_.name)));
        reel_symbol_transforms_.Add(_symbol_);

        dict.TryGetValue(-300, out _symbol_);
        reel_symbol_ids_.Add(Enums.SymbolToId(Enums.StringToSymbol(_symbol_.name)));
        reel_symbol_transforms_.Add(_symbol_);

        // emmit symbols ids
        return reel_symbol_ids_;
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

            image.GetComponentInChildren<Image>().sprite = symbols[index]._SYMBOL_;
            image.name = symbols[index]._NAME_;

            int rand = Random.Range(0, parts.Count);

            image.transform.position = new Vector3(image.transform.position.x, parts[rand] + transform.parent.parent.GetComponent<RectTransform>().transform.position.y, image.transform.position.z);
            parts.RemoveAt(rand);
            index++;
        }
    }
}
