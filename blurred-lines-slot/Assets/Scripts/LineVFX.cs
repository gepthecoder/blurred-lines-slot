using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineVFX : MonoBehaviour
{
    public int line_id_;

    public void DisableLine() { gameObject.SetActive(false); }
    public void EnableLine() { gameObject.SetActive(true); }

}
