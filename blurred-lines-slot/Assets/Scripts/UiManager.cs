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
    [SerializeField] private Sprite auto_play_symbol_start_;
    [SerializeField] private Sprite auto_play_symbol_stop_;
    public Sprite GetAutoPlayBtnSprite() { return auto_play_btn_sprite_; }
    public Sprite GetStopAutoPlayBtnSprite() { return stop_auto_play_btn_sprite_; }

    public Sprite GetAutoPlaySymbolStart() { return auto_play_symbol_start_; }
    public Sprite GetAutoPlaySymbolStop() { return auto_play_symbol_stop_; }

    [Header("Turbo Mode")]
    [Space(5)]
    [SerializeField] private Sprite turbo_mode_btn_sprite_;
    [SerializeField] private Sprite stop_turbo_mode_btn_sprite_;

    public Sprite GetTurboModeBtnSprite() { return turbo_mode_btn_sprite_; }
    public Sprite GetStopTurboModeBtnSprite() { return stop_turbo_mode_btn_sprite_; }

    private void Awake()
    {
        instance_ = this;
    }

}
