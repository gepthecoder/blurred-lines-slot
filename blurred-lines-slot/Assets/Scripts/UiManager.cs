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

    [Header("Music & Sounds")]
    [Space(5)]
    [SerializeField] private Button sound_on_off_btn_;
    [SerializeField] private Image sound_on_off_symbol_img_;
    [SerializeField] private Sprite sound_on_symbol_;
    [SerializeField] private Sprite sound_off_symbol_;

    [Header("Info")]
    [Space(5)]
    [SerializeField] private GameObject info_section_ui_;

    [SerializeField] private Button show_info_btn_;
    [SerializeField] private Button close_info_btn_;

    private bool is_info_shown_ = false;

    private void Awake()
    {
        instance_ = this;

        sound_on_off_btn_.onClick.AddListener(OnSoundOnOffClicked);

        show_info_btn_.onClick.AddListener(ShowHideInfoUi);
        close_info_btn_.onClick.AddListener(ShowHideInfoUi);
    }

    private void ShowHideInfoUi()
    {
        is_info_shown_ = !is_info_shown_;
        info_section_ui_.SetActive(is_info_shown_);
    }

    private void OnSoundOnOffClicked()
    {
        bool muted;
        AudioManager.instance_.HandleAudio(out muted);

        sound_on_off_symbol_img_.sprite = muted ? sound_off_symbol_ : sound_on_symbol_;
    }

}
