using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Symbol", menuName = "Symbol")]
public class Symbol : ScriptableObject
{
    [Range(1,10)]
    public new int _ID_;

    public string _NAME_;

    [Range(1, 100)]
    public float _FREQUENCY_;

    public Sprite _SYMBOL_;

    // TODO: PAYOUT TABLE

}
