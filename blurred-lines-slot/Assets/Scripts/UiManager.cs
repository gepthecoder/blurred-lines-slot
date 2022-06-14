using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager instance_;

    [Header("AutoPlay Options")]
    [Space(5)]
    [SerializeField] private Sprite auto_play_btn_sprite_;
    [SerializeField] private Sprite stop_auto_play_btn_sprite_;

    public Sprite GetAutoPlayBtnSprite() { return auto_play_btn_sprite_; }
    public Sprite GetStopAutoPlayBtnSprite() { return stop_auto_play_btn_sprite_; }

    private void Awake()
    {
        instance_ = this;
    }

}
